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


class PropDbReaderCad(PropReader):
    def __int__(self, urn, token, region="US", manifest_item: [ManifestItem] = None):
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

    # TODO : AcDbLayerTableRecord
    def get_all_layers(self):
        db_layers = []
        for i, s in enumerate(self.ids):
            properties = self.enumerate_properties(i)
            if properties:
                for p in properties:
                    if p.name == "Layer":
                        db_layers.append(p.value)
        db_layers = list(set(db_layers))
        db_layers.sort()
        return db_layers

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

    def get_data_by_category(self, category) -> pd.DataFrame:
        """
        Get data by cad category : eg: MText, Line, Circle, ...
        :param category: the category name of cad file, e.g : MText, Line, Circle,Tables ...
        :return: pandas dataframe
        """
        db_categories = self.get_all_categories()
        category_ids = [k for k, v in db_categories.items() if v == category]
        childs = self.get_children(category_ids[0])
        return self.get_recursive_ids(childs)
