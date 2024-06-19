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
        """
        Get document info of source file
        :return: pd.DataFrame - dataframe contains document info
        """
        props = {}
        for prop in self.enumerate_properties(1):
            props[prop.display_name] = prop.value
        df = pd.DataFrame.from_dict(props, orient='index')
        df = df.rename(columns={0: "value"})
        df = df.reset_index()
        df = df.rename(columns={"index": "property"})
        return df

    def get_all_categories(self) -> List[str]:
        """
        Get all categories in the model e.g. Item, Element, ...
        :return:  List[str] - list of categories contains in navisworks model
        """
        categories = []
        rg = re.compile(r'^__\w+__$')
        for i in range(1, len(self.attrs)):
            if self.attrs[i][1] not in categories and not rg.match(self.attrs[i][1]):
                categories.append(self.attrs[i][1])
        return categories

    def get_all_cats_params(self) -> pd.DataFrame:
        """
        Get all categories and parameters in the model, return a dataframe with columns: Category, Parameter \n
        Category: category of Parameter \n
        Parameter: parameter name
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

    def get_data_by_categories(self, categories: List[str], sep="|") -> pd.DataFrame:
        """
        Get data by categories in the model, e.g. Item, Element, ...
        :param categories: List[str] - list of categories
        :param sep: str - separator between category and parameter
        :return: pd.DataFrame - dataframe contains data by categories and parameters
        """
        db_ids = [1]
        df = self._get_recursive_ids_by_categories(db_ids, categories, sep)
        df = df.dropna(axis=0, how='all', subset=df.columns.difference(['DbId']))
        return df

    def get_all_sources_files(self) -> List[str]:
        """
        Get all sub sources files in the model like .rvt .dwg .nwd ,...
        :return: List[str] - list of sources files
        """
        childs = self.get_children(1)
        sources = self._get_recursive_ids_sources_files(childs)
        return sources

    def _get_recursive_ids_sources_files(self, db_ids: List[int]) -> List[str]:
        sources = []
        for id in db_ids:
            props = self.enumerate_properties(id)
            # list objects props to dataframe
            properties_dicts = [prop.__dict__ for prop in props]
            df_props = pd.DataFrame(properties_dicts)
            # stop tree from layer
            if not df_props[(df_props['display_name'] == 'Type') & (df_props['value'] == 'Layer')].empty:
                continue
            # see if any row have column 'display_name' with value is Type and  column 'Value' is File
            if not df_props[(df_props['display_name'] == 'Type') & (df_props['value'] == 'File')].empty:
                row_value = df_props[(df_props['category'] == 'Item') & (df_props['display_name'] == 'Name')].iloc[0]
                value = row_value['value']
                sources.append(value)
                child_ids = self.get_children(id)
                if len(child_ids) > 0:
                    sources = sources + self._get_recursive_ids_sources_files(child_ids)
        return sources

    def get_all_data_resources(self) -> pd.DataFrame:
        """
       Get data by sub sources files in the model, the data will add new column 'ModelName' to dataframe
       :return: pd.DataFrame - dataframe contains data by resources
       """
        child_ids = self.get_children(1)
        cats = self.get_all_categories()
        df = self._get_recursive_data_by_resources(child_ids, cats)
        df = df.dropna(axis=0, how='all', subset=df.columns.difference(['DbId']))
        return df

    def get_data_resources_by_categories(self, categories: List[str], sep: str = '|') -> pd.DataFrame:
        """
        Get data by resources model append to model
        :param categories: List[str] - list of categories like Item, Element, ...
        :param sep: str - separator between category and parameter
        :return: pd.DataFrame - dataframe contains data by resources
        """
        child_ids = self.get_children(1)
        df = self._get_recursive_data_by_resources(child_ids, categories, sep)
        df = df.dropna(axis=0, how='all', subset=df.columns.difference(['DbId']))
        return df

    def _get_recursive_data_by_resources(self, db_ids: list[int], categories: list[str], sep="|") -> pd.DataFrame:
        dataframe = pd.DataFrame()
        for id in db_ids:
            props = self.enumerate_properties(id)
            # list objects props to dataframe
            properties_dicts = [prop.__dict__ for prop in props]
            df_props = pd.DataFrame(properties_dicts)
            # see if any row have column 'display_name' with value is Type and  column 'Value' is File
            if not df_props[(df_props['display_name'] == 'Type') & (df_props['value'] == 'File')].empty:
                row_value = df_props[(df_props['category'] == 'Item') & (df_props['display_name'] == 'Name')].iloc[0]
                source_name = row_value['value']
                child_ids = self.get_children(id)
                if len(child_ids) > 0:
                    df = self._get_recursive_elements(source_name, child_ids, categories, sep)
                    dataframe = pd.concat([dataframe, df], ignore_index=True)
        if dataframe.empty:
            # use main model
            props = self.enumerate_properties(1)
            properties_dicts = [prop.__dict__ for prop in props]
            df_props = pd.DataFrame(properties_dicts)
            # see if any row have column 'display_name' with value is Type and  column 'Value' is File
            if not df_props[(df_props['display_name'] == 'Type') & (df_props['value'] == 'File')].empty:
                row_value = df_props[(df_props['category'] == 'Item') & (df_props['display_name'] == 'Name')].iloc[0]
                source_name = row_value['value']
                child_ids = self.get_children(id)
                if len(child_ids) > 0:
                    df = self._get_recursive_elements(source_name, child_ids, categories, sep)
                    dataframe = pd.concat([dataframe, df], ignore_index=True)
        return dataframe

    def _get_recursive_elements(self, model_name, source_ids: list[int], categories: list[str],
                                sep='|') -> pd.DataFrame:
        all_properties = []

        def collect_properties(id):
            props = self.enumerate_properties(id)
            properties = {"DbId": id, "ModelName": model_name}

            for p in props:
                if p.category not in categories:
                    continue
                key = p.category + sep + str(p.display_name) if p.category else str(p.display_name)
                if key:
                    properties[key] = p.value

            return properties

        def process_ids(ids):
            for id in ids:
                properties = collect_properties(id)
                if len(properties) > 2:  # More than DbId and ModelName
                    all_properties.append(properties)
                children = self.get_children(id)
                if children:
                    process_ids(children)

        process_ids(source_ids)

        dataframe = pd.DataFrame(all_properties)
        return dataframe

    def _get_recursive_ids_by_categories(self, db_ids: List[int], categories: List[str], sep='|') -> pd.DataFrame:
        """
        Recursive get data by categories in the model
        :param db_ids:  List[int] - list of db_ids
        :param categories:  List[str] - list of categories
        :param sep:  str - separator between category and parameter
        :return: pd.DataFrame - dataframe contains data by categories
        """
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
                            key = p.category + sep + str(p.display_name)
                            properties[key] = p.value
            if len(properties) > 1:
                singleDF = pd.DataFrame(properties, index=[0])
                dataframe = pd.concat([dataframe, singleDF], ignore_index=True)
            children = self.get_children(id)
            df = self._get_recursive_ids_by_categories(children, categories, sep)
            dataframe = pd.concat([dataframe, df], ignore_index=True)
        return dataframe

    def get_all_data(self) -> pd.DataFrame:
        """
        Get all data in the model by categories e.g. Item, Element, ...
        :return:  pd.DataFrame - dataframe contains all data in the model
        """
        cats = self.get_all_categories()
        df = self.get_data_by_categories(cats)
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
