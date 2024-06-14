"""
Copyright (C) 2024  chuongmep.com

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
"""
from typing import List
import re
import pandas as pd
import json
import requests
from .PropReader import PropReader
from .ManifestItem import ManifestItem
from .Token import Token
import warnings


class PropDbReaderNavis(PropReader):
    def __int__(self, urn, token: Token, region="US", manifest_item: [ManifestItem] = None):
        super().__init__(urn, token, region, manifest_item)

    def _get_recursive_child(self, output, id, name):
        children = self.get_children(id)
        for child in children:
            properties = self.enumerate_properties(child)
            property = [prop.value for prop in properties if prop.name == name]
            if len(property) == 0:
                self._get_recursive_child(output, child, name)
            else:
                if str(property[0]) == "": continue
                output[child] = property[0].strip()

    def get_external_id(self, id) -> str:
        return self.ids[id]

    def get_document_info(self) -> pd.DataFrame:
        props = {}
        for prop in self.enumerate_properties(1):
            props[prop.display_name] = prop.value
        df = pd.DataFrame.from_dict(props, orient='index')
        df = df.rename(columns={0: "value"})
        df = df.reset_index()
        df = df.rename(columns={"index": "property"})
        return df

    def get_all_categories(self) -> List[str]:
        categories = []
        rg = re.compile(r'^__\w+__$')
        for i in range(1, len(self.attrs)):
            if self.attrs[i][1] not in categories and not rg.match(self.attrs[i][1]):
                categories.append(self.attrs[i][1])
        return categories

    def get_all_parameters(self) -> pd.DataFrame:
        """
        Get all parameters in the model, return a dataframe with columns: Category, Parameter
        :return: pd.DataFrame - dataframe contains all categories - parameters in the model
        """
        categories = []
        rg = re.compile(r'^__\w+__$')
        df = pd.DataFrame()
        for i in range(1, len(self.attrs)):
            if self.attrs[i][1] not in categories and not rg.match(self.attrs[i][1]):
                category = self.attrs[i][1]
                parameter = self.attrs[i][5]
                singleDF = pd.DataFrame({"Category": [category], "Parameter": [parameter]})
                df = pd.concat([df, singleDF], ignore_index=True)
        df = df.sort_values(by=['Category'])
        return df

    def get_data_by_categories(self, categories: List[str]) -> pd.DataFrame:
        """
        Get data by categories and parameters
        :param categories: List[str] - list of categories
        :param parameters: List[str] - list of parameters
        :return: pd.DataFrame - dataframe contains data by categories and parameters
        """
        db_ids = [1]
        df = self._get_recursive_ids_by_categories(db_ids, categories)
        df = df.dropna(axis=0, how='all', subset=df.columns.difference(['DbId']))
        return df

    def _get_recursive_ids_by_categories(self, db_ids: List[int], categories: List[str]) -> pd.DataFrame:
        dataframe = pd.DataFrame()
        props_ignore = ['parent', 'instanceof_objid', 'child', "viewable_in"]
        if len(db_ids) == 0:
            return dataframe
        for id in db_ids:
            props = self.enumerate_properties(id)
            properties = {}
            for p in props:
                if p.category in categories:
                    properties["DbId"] = id
                    for p in props:
                        if p.name not in props_ignore and p.category in categories:
                            properties[p.display_name] = p.value
            if len(properties) > 1:
                singleDF = pd.DataFrame(properties, index=[0])
                dataframe = pd.concat([dataframe, singleDF], ignore_index=True)
            children = self.get_children(id)
            df = self._get_recursive_ids_by_categories(children, categories)
            dataframe = pd.concat([dataframe, df], ignore_index=True)
        return dataframe

    def get_all_data(self) -> pd.DataFrame:
        cates = self.get_all_categories()
        df = pd.DataFrame()
        for cate in cates:
            df_single = self.get_data_by_category(cate)
            df = pd.concat([df, df_single], ignore_index=True)
        return df

    def get_data_by_category(self, category: str) -> pd.DataFrame:
        warnings.warn("This method is deprecated, use get_data_by_categories instead", DeprecationWarning)
        db_ids = [1]
        df = self._get_recursive_ids_by_category(db_ids, category)
        df = df.drop_duplicates(subset=df.columns.difference(['DbId']))
        return df

    def _get_recursive_ids_by_category(self, db_ids: List[int], category: str) -> pd.DataFrame:
        dataframe = pd.DataFrame()
        props_ignore = ['parent', 'instanceof_objid', 'child', "viewable_in"]
        if len(db_ids) == 0:
            return dataframe
        for id in db_ids:
            props = self.enumerate_properties(id)
            properties = {}
            flag = [p for p in props if p.category == category]
            if flag:
                properties["DbId"] = id
                for p in props:
                    if p.name not in props_ignore and p.category == category:
                        properties[p.display_name] = p.value
                singleDF = pd.DataFrame(properties, index=[0])
                dataframe = pd.concat([dataframe, singleDF], ignore_index=True)
            children = self.get_children(id)
            df = self._get_recursive_ids_by_category(children, category)
            dataframe = pd.concat([dataframe, df], ignore_index=True)
        return dataframe
