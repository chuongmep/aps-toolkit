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
import re
from typing import List
import pandas as pd
from .ManifestItem import ManifestItem
from .PropReader import PropReader
from .Token import Token


class PropDbReaderCad(PropReader):
    def __int__(self, urn: str, token: Token, region: str = "US", manifest_item: [ManifestItem] = None):
        super().__init__(urn, token, region, manifest_item)

    def get_document_info(self) -> pd.Series:
        schema_name = "DocumentData"
        indexs = []
        # get index at value DocumentData
        for i, s in enumerate(self.ids):
            if s == schema_name:
                indexs.append(i)
        df = self.get_recursive_ids(indexs)
        series = df.iloc[0]
        return series

    def get_all_layers(self):
        db_layers = []
        df = pd.DataFrame()
        for i, s in enumerate(self.ids):
            properties = self.enumerate_properties(i)
            flag_layer = [p for p in properties if p.name == "type" and p.value == "AcDbLayerTableRecord"]
            if flag_layer:
                df_single = self.get_recursive_ids([i])
                df = pd.concat([df, df_single])
        return df

    def get_all_categories(self) -> dict:
        db_categories = {}
        for i, s in enumerate(self.ids):
            properties = self.enumerate_properties(i)
            type = "AcDbBlockTableRecord"
            dbid = None
            flag = [p for p in properties if p.name == "type" and p.value == type]
            if flag:
                dbid = i
            if dbid:
                childs = self.get_children(dbid)
                for child in childs:
                    properties = self.enumerate_properties(child)
                    for p in properties:
                        if p.name == "type":
                            db_categories[child] = p.value
        return db_categories

    def get_data_by_category(self, category: str) -> pd.DataFrame:
        """
        Get data by cad category : eg: MText, Line, Circle, ...
        :param category: the category name of cad file, e.g : MText, Line, Circle,Tables ...
        :return: pandas dataframe
        """
        db_categories = self.get_all_categories()
        category_ids = [k for k, v in db_categories.items() if v.lower() == category.lower()]
        childs = self.get_children(category_ids[0])
        return self.get_recursive_ids(childs)

    def get_data_by_categories(self, categories: List[str]) -> pd.DataFrame:
        """
        Get data by multiple cad categories
        :param categories: list of category names
        :return: pandas dataframe
        """
        df = pd.DataFrame()
        for category in categories:
            df = pd.concat([df, self.get_data_by_category(category)])
        return df

    def get_data_by_categories_and_params(self, categories: List[str], params: List[str]) -> pd.DataFrame:
        """
        Get data by multiple cad categories and multiple parameters
        :param categories: list of category names
        :param params: list of parameters
        :return: pandas dataframe
        """
        db_categories = self.get_all_categories()
        category_ids = [k for k, v in db_categories.items() if v.lower() in [c.lower() for c in categories]]
        childs = [self.get_children(category_id) for category_id in category_ids]
        # flatten list of list
        childs = [item for sublist in childs for item in sublist]
        df = self.get_recursive_ids_by_parameters(childs, params)
        return df

    def get_all_data(self) -> pd.DataFrame:
        """
        Get all data from cad file
        :return: pandas dataframe
        """
        cates = self.get_all_categories()
        df = pd.DataFrame()
        for k, v in cates.items():
            childs = self.get_children(k)
            df = pd.concat([df, self.get_recursive_ids(childs)])
        return df
