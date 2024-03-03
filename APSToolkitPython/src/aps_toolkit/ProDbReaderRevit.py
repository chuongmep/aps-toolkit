import gzip
import json
import re
import codecs
from typing import List
from .PropReader import PropReader
import pandas as pd


class PropDbReaderRevit(PropReader):
    def __int__(self, urn, token):
        super().__init__(urn, token)

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

    def get_document_info(self) -> pd.Series:
        properties = self.get_properties(1)
        instances = self.get_instance(1)
        for instance in instances:
            types = self.get_properties(instance)
            properties = {**properties, **types}
        return pd.Series(properties)

    def get_all_categories(self) -> dict:
        categories = {}
        self._get_recursive_child(categories, 1, "_RC")
        return categories

    def get_all_families(self) -> dict:
        families = {}
        self._get_recursive_child(families, 1, "_RFN")
        return families

    def get_all_families_types(self) -> dict:
        families_types = {}
        self._get_recursive_child(families_types, 1, "_RFT")
        return families_types

    def get_data_by_category(self, category) -> pd.DataFrame:
        categories = self.get_all_categories()
        # if category starts with Revit, remove it
        if category.startswith("Revit"):
            category = category[5:].strip()
        category_id = [key for key, value in categories.items() if value == category]
        dataframe = self._get_recursive_ids(category_id)
        return dataframe

    def get_data_by_categories(self, categories: List[str]) -> pd.DataFrame:
        dataframe = pd.DataFrame()
        for category in categories:
            dataframe = pd.concat([dataframe, self.get_data_by_category(category)], ignore_index=True)
        return dataframe

    def get_data_by_categories_and_params(self, categories: List[str], params: List[str]) -> pd.DataFrame:
        dataframe = pd.DataFrame()
        all_categories = self.get_all_categories()
        category_ids = [key for key, value in all_categories.items() if value in categories]
        for category_id in category_ids:
            dataframe = pd.concat([dataframe, self._get_recursive_ids_prams([category_id], params)], ignore_index=True)
        # remove all row have all values is null, ignore dbId and external_id columns
        dataframe = dataframe.dropna(how='all',
                                     subset=[col for col in dataframe.columns if col not in ['dbId', 'external_id']])
        return dataframe

    def get_data_by_family(self, family_name) -> pd.DataFrame:
        families = self.get_all_families()
        category_id = [key for key, value in families.items() if value == family_name]
        dataframe = self._get_recursive_ids(category_id)
        return dataframe

    def get_data_by_family_type(self, family_type) -> pd.DataFrame:
        family_types = self.get_all_families_types()
        type_id = [key for key, value in family_types.items() if value == family_type]
        dataframe = self._get_recursive_ids(type_id)
        return dataframe

    def _get_recursive_ids(self, db_ids: List[int]) -> pd.DataFrame:
        dataframe = pd.DataFrame()
        if len(db_ids) == 0:
            return dataframe
        for id in db_ids:
            props = self.enumerate_properties(id)
            properties = {}
            rg = re.compile(r'^__\w+__$')
            # if props contain _RC, _RFN, _RFT, it's not a leaf node, continue to get children
            if len([prop for prop in props if prop.name in ["_RC", "_RFN", "_RFT"]]) > 0:
                ids = self.get_children(id)
                dataframe = pd.concat([dataframe, self._get_recursive_ids(ids)], ignore_index=True)
                continue
            for prop in props:
                if prop.category and not rg.match(prop.category):
                    properties[prop.name] = prop.value
            db_id = id
            external_id = self.ids[id]
            properties['dbId'] = db_id
            properties['external_id'] = external_id
            ins = self.get_instance(id)
            if len(ins) > 0:
                for instance in ins:
                    types = self.get_properties(instance)
                    properties = {**properties, **types}
            singleDF = pd.DataFrame(properties, index=[0])
            dataframe = pd.concat([dataframe, singleDF], ignore_index=True)
            ids = self.get_children(id)
            dataframe = pd.concat([dataframe, self._get_recursive_ids(ids)], ignore_index=True)
        dataframe = dataframe[['dbId', 'external_id'] + [col for col in dataframe.columns if col not in ['dbId', 'external_id']]]
        return dataframe

    def _get_recursive_ids_prams(self, childs: List[int], params: List[str]) -> pd.DataFrame:
        dataframe = pd.DataFrame()
        if len(childs) == 0:
            return dataframe
        for id in childs:
            props = self.enumerate_properties(id)
            # if props contain _RC, _RFN, _RFT, it's not a leaf node, continue to get children
            if len([prop for prop in props if prop.name in ["_RC", "_RFN", "_RFT"]]) > 0:
                ids = self.get_children(id)
                dataframe = pd.concat([dataframe, self._get_recursive_ids(ids)], ignore_index=True)
                continue
            properties = {}
            for prop in props:
                properties[prop.name] = prop.value
            db_id = id
            external_id = self.ids[id]
            # filter just get properties name in params list
            properties = {k: v for k, v in properties.items() if k in params}
            properties['dbId'] = db_id
            properties['external_id'] = external_id
            # get instances
            ins = self.get_instance(id)
            if len(ins) > 0:
                for instance in ins:
                    types = self.get_all_properties(instance)
                    types = {k: v for k, v in types.items() if k in params}
                    properties = {**properties, **types}
            singleDF = pd.DataFrame(properties, index=[0])
            dataframe = pd.concat([dataframe, singleDF], ignore_index=True)
            ids = self.get_children(id)
            dataframe = pd.concat([dataframe, self._get_recursive_ids_prams(ids,params)], ignore_index=True)
        dataframe = dataframe[['dbId', 'external_id'] + [col for col in dataframe.columns if col not in ['dbId', 'external_id']]]
        return dataframe

    def get_data_by_external_id(self, external_id) -> pd.DataFrame:
        db_id = None
        for idx in range(1, len(self.ids) + 1):
            if self.ids[idx] == external_id:
                db_id = idx
                break
        if db_id is None:
            return pd.DataFrame()
        dataframe = self._get_recursive_ids([db_id])
        # transpose to dataframe with two columns: property and value
        df = dataframe.T
        df.reset_index(inplace=True)
        df.columns = ['property', 'value']
        # keep dataframe because maybe we need expand for unit in future
        return df
