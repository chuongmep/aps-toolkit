from typing import List
import re
import pandas as pd
import json
import requests
from .PropReader import PropReader


class PropDbReaderRevit(PropReader):
    def __int__(self, urn, token, region="US"):
        super().__init__(urn, token, region)

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
        properties = self.get_all_properties(1)
        return pd.Series(properties)

    def _get_aec_model_data(self):
        URL = f"{self.host}/modelderivative/v2/designdata/{self.urn}/manifest"
        access_token = self.token.access_token
        headers = {
            "Authorization": f"Bearer {access_token}",
            "region": self.region
        }
        # request
        response = requests.get(URL, headers=headers)
        json_response = response.json()
        children = json_response['derivatives'][0]["children"]
        urn_json = ""
        for child in children:
            if child["type"] == "resource" and child["role"] == "Autodesk.AEC.ModelData":
                urn_json = child["urn"]
        URL = f"{self.host}/modelderivative/v2/designdata/{self.urn}/manifest/{urn_json}"
        response = requests.get(URL, headers=headers)
        json_response = response.json()
        return json_response

    def get_phases(self) -> List[str]:
        phases = []
        json_response = self._get_aec_model_data()
        for phase in json_response["phases"]:
            phases.append(phase["name"])
        return phases

    def get_document_id(self) -> str:
        json_response = self._get_aec_model_data()
        return json_response["documentId"]

    def get_levels(self) -> pd.DataFrame:
        json_response = self._get_aec_model_data()
        levels = json_response["levels"]
        df = pd.DataFrame(levels)
        return df

    def get_grids(self) -> pd.DataFrame:
        json_response = self._get_aec_model_data()
        grids = json_response["grids"]
        df = pd.DataFrame(grids)
        return df

    def get_linked_documents(self) -> list:
        json_response = self._get_aec_model_data()
        linked_documents = json_response["linkedDocuments"]
        return linked_documents

    def get_ref_point_transformation(self) -> list:
        json_response = self._get_aec_model_data()
        return json_response["refPointTransformation"]

    def get_all_categories(self) -> dict:
        categories = {}
        self._get_recursive_child(categories, 1, "_RC")
        return categories

    def get_all_data(self, is_get_sub_family=False) -> pd.DataFrame:
        categories_dict = self.get_all_categories()
        dbids = list(categories_dict.keys())
        dataframe = pd.DataFrame()
        for dbid in dbids:
            df = self._get_recursive_ids([dbid], is_get_sub_family)
            dataframe = pd.concat([dataframe, df], ignore_index=True)
        return dataframe

    def get_all_families(self) -> dict:
        families = {}
        self._get_recursive_child(families, 1, "_RFN")
        return families

    def get_all_families_types(self) -> dict:
        families_types = {}
        self._get_recursive_child(families_types, 1, "_RFT")
        return families_types

    def get_data_by_category(self, category, is_get_sub_family=False) -> pd.DataFrame:
        categories = self.get_all_categories()
        # if category starts with Revit, remove it
        if category.startswith("Revit"):
            category = category[5:].strip()
        category_id = [key for key, value in categories.items() if value == category]
        dataframe = self._get_recursive_ids(category_id, is_get_sub_family)
        return dataframe

    def get_data_by_categories(self, categories: List[str], is_get_sub_family=False) -> pd.DataFrame:
        dataframe = pd.DataFrame()
        for category in categories:
            dataframe = pd.concat([dataframe, self.get_data_by_category(category, is_get_sub_family)],
                                  ignore_index=True)
        return dataframe

    def get_data_by_categories_and_params(self, categories: List[str], params: List[str],
                                          is_get_sub_family=False) -> pd.DataFrame:
        dataframe = pd.DataFrame()
        all_categories = self.get_all_categories()
        category_ids = [key for key, value in all_categories.items() if value in categories]
        for category_id in category_ids:
            dataframe = pd.concat([dataframe, self._get_recursive_ids_prams([category_id], params, is_get_sub_family)],
                                  ignore_index=True)
        # remove all row have all values is null, ignore dbId and external_id columns
        dataframe = dataframe.dropna(how='all',
                                     subset=[col for col in dataframe.columns if col not in ['dbId', 'external_id']])
        return dataframe

    def get_data_by_family(self, family_name, is_get_sub_family=False) -> pd.DataFrame:
        families = self.get_all_families()
        category_id = [key for key, value in families.items() if value == family_name]
        dataframe = self._get_recursive_ids(category_id, is_get_sub_family)
        return dataframe

    def get_data_by_family_type(self, family_type, is_get_sub_family=False) -> pd.DataFrame:
        family_types = self.get_all_families_types()
        type_id = [key for key, value in family_types.items() if value == family_type]
        dataframe = self._get_recursive_ids(type_id, is_get_sub_family)
        return dataframe

    def _get_recursive_ids(self, db_ids: List[int], get_sub_family) -> pd.DataFrame:
        dataframe = pd.DataFrame()
        props_ignore = ['parent', 'instanceof_objid', 'child', "viewable_in"]
        if len(db_ids) == 0:
            return dataframe
        for id in db_ids:
            props = self.enumerate_properties(id)
            flag_sub_families = False
            properties = {}
            # if props contain _RC, _RFN, _RFT, it's not a leaf node, continue to get children
            if len([prop for prop in props if prop.name in ["_RC", "_RFN", "_RFT"]]) > 0:
                ids = self.get_children(id)
                dataframe = pd.concat([dataframe, self._get_recursive_ids(ids, get_sub_family)], ignore_index=True)
                continue
            for prop in props:
                if prop.category == "__internalref__" and prop.name == "Sub Family":
                    flag_sub_families = True
                if prop.name not in props_ignore:
                    if prop.name == "name":
                        properties["Name"] = prop.value
                    else:
                        properties[prop.name] = prop.value
            db_id = id
            external_id = self.ids[id]
            properties['dbId'] = db_id
            properties['external_id'] = external_id
            if flag_sub_families and not get_sub_family:
                ids = self.get_children(id)
                dataframe = pd.concat([dataframe, self._get_recursive_ids(ids, get_sub_family)], ignore_index=True)
                continue
            ins = self.get_instance(id)
            if len(ins) > 0:
                for instance in ins:
                    types = self.get_properties(instance)
                    properties = {**properties, **types}
            singleDF = pd.DataFrame(properties, index=[0])
            dataframe = pd.concat([dataframe, singleDF], ignore_index=True)
            ids = self.get_children(id)
            dataframe = pd.concat([dataframe, self._get_recursive_ids(ids, get_sub_family)], ignore_index=True)
        if 'dbId' in dataframe.columns and 'external_id' in dataframe.columns:
            dataframe = dataframe[
                ['dbId', 'external_id'] + [col for col in dataframe.columns if col not in ['dbId', 'external_id']]]
        return dataframe

    def _get_recursive_ids_prams(self, childs: List[int], params: List[str], get_sub_family) -> pd.DataFrame:
        dataframe = pd.DataFrame()
        props_ignore = ['parent', 'instanceof_objid', 'child', "viewable_in"]
        if len(childs) == 0:
            return dataframe
        for id in childs:
            flag_sub_families = False
            props = self.enumerate_properties(id)
            # if props contain _RC, _RFN, _RFT, it's not a leaf node, continue to get children
            if len([prop for prop in props if prop.name in ["_RC", "_RFN", "_RFT"]]) > 0:
                ids = self.get_children(id)
                dataframe = pd.concat([dataframe, self._get_recursive_ids_prams(ids, params, get_sub_family)],
                                      ignore_index=True)
                continue
            properties = {}
            for prop in props:
                if prop.category == "__internalref__" and prop.name == "Sub Family":
                    flag_sub_families = True
                if prop.name not in props_ignore:
                    if prop.name == "name":
                        properties["Name"] = prop.value
                    else:
                        properties[prop.name] = prop.value
            db_id = id
            external_id = self.ids[id]
            # filter just get properties name in params list
            properties = {k: v for k, v in properties.items() if k in params}
            properties['dbId'] = db_id
            properties['external_id'] = external_id
            if flag_sub_families and not get_sub_family:
                ids = self.get_children(id)
                dataframe = pd.concat([dataframe, self._get_recursive_ids_prams(ids, params, get_sub_family)],
                                      ignore_index=True)
                continue
            # get instances
            ins = self.get_instance(id)
            if len(ins) > 0:
                for instance in ins:
                    types = self.get_all_properties(instance)
                    types = {k: v for k, v in types.items() if k in params}
                    properties = {**properties, **types}
            single_df = pd.DataFrame(properties, index=[0])
            dataframe = pd.concat([dataframe, single_df], ignore_index=True)
            ids = self.get_children(id)
            dataframe = pd.concat([dataframe, self._get_recursive_ids_prams(ids, params, get_sub_family)],
                                  ignore_index=True)
        # set dbid and external_id to first and second column if it exists
        if 'dbId' in dataframe.columns and 'external_id' in dataframe.columns:
            dataframe = dataframe[
                ['dbId', 'external_id'] + [col for col in dataframe.columns if col not in ['dbId', 'external_id']]]
        return dataframe

    def get_data_by_external_id(self, external_id, is_get_sub_family=False) -> pd.DataFrame:
        db_id = None
        for idx in range(0, len(self.ids)):
            if self.ids[idx] == external_id:
                db_id = idx
                break
        if db_id is None:
            return pd.DataFrame()
        dataframe = self._get_recursive_ids([db_id], is_get_sub_family)
        return dataframe

    def get_data_by_element_id(self, element_id) -> dict:
        rg = re.compile(r'^__\w+__$')
        properties = {}
        for i in range(0, len(self.ids)):
            props = self.enumerate_properties(i)
            for prop in props:
                if prop.name == "ElementId" and prop.value == str(element_id):
                    for prop in props:
                        if not rg.match(prop.category):
                            properties[prop.name] = prop.value
                    # get instance
                    instances = self.get_instance(i)
                    for instance in instances:
                        types = self.get_properties(instance)
                        properties = {**properties, **types}
                    break
        properties = dict(sorted(properties.items()))
        return properties

    def get_all_parameters(self) -> List:
        parameters = []
        for id in range(0, len(self.ids)):
            props_dict = self.get_properties(id)
            for key, value in props_dict.items():
                if key not in parameters:
                    parameters.append(key)
        parameters = list(set(parameters))
        parameters.sort()
        return parameters
