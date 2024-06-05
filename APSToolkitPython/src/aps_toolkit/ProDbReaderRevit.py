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
import requests
from .PropReader import PropReader
from .ManifestItem import ManifestItem


class PropDbReaderRevit(PropReader):
    """
    Class PropDbReaderRevit to read properties from Revit model
    """

    def __int__(self, urn, token, region="US", manifest_item: [ManifestItem] = None):
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
        """
        Get unique id of element in model from database id
        :param id:  The database id of element in model
        :return:  :class:`str` : Unique id of element in model
        """
        return self.ids[id]

    def get_db_id(self, external_id) -> int:
        """
        Get database id of element in model from external id
        :param external_id:  The unique id of element in model
        :return:  :class:`int` : Database id of element in model
        """
        for idx in range(0, len(self.ids)):
            if self.ids[idx] == external_id:
                return idx
        return -1

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
        """
        Get all phases in model
        :return: :class:`list` : List contains all phases
        """
        phases = []
        json_response = self._get_aec_model_data()
        for phase in json_response["phases"]:
            phases.append(phase["name"])
        return phases

    def get_document_id(self) -> str:
        """
        Get unique id of document in model
        :return: :class:`str` : Document id
        """
        json_response = self._get_aec_model_data()
        return json_response["documentId"]

    def get_levels(self) -> pd.DataFrame:
        """
        Get levels in model
        :return: :class:`pandas.DataFrame` : Dataframe contains levels
        """
        json_response = self._get_aec_model_data()
        levels = json_response["levels"]
        df = pd.DataFrame(levels)
        return df

    def get_grids(self) -> pd.DataFrame:
        """
        Get grids in model
        :return:  :class:`pandas.DataFrame` : Dataframe contains grids
        """
        json_response = self._get_aec_model_data()
        grids = json_response["grids"]
        df = pd.DataFrame(grids)
        return df

    def get_linked_documents(self) -> list:
        """
        Get linked documents in model
        :return:  :class:`list` : List contains linked documents
        """
        json_response = self._get_aec_model_data()
        linked_documents = json_response["linkedDocuments"]
        return linked_documents

    def get_ref_point_transformation(self) -> list:
        """
        Get ref point transformation in model
        :return:  :class:`list` : List contains ref point transformation
        """
        json_response = self._get_aec_model_data()
        return json_response["refPointTransformation"]

    def get_all_categories(self) -> dict:
        """
        Get all categories in model
        e.g: {1: "Walls", 2: "Doors", 3: "Windows", 4: "Furniture", 5: "Plumbing Fixtures", 6: "Electrical Fixtures"}
        :return:  :class:`dict` : Dictionary contains all categories, key is dbId, value is category name
        """
        categories = {}
        self._get_recursive_child(categories, 1, "_RC")
        return categories

    def get_all_data(self, is_get_sub_family: bool = False, display_unit: bool = False) -> pd.DataFrame:
        """
        Get all data from model, include all categories
        :param is_get_sub_family: the flag to get sub family or not, default is False
        :param display_unit: the flag to display unit or not in value, default is False
        :return: :class:`pandas.DataFrame` : Dataframe contains all data
        """
        categories_dict = self.get_all_categories()
        dbids = list(categories_dict.keys())
        dataframe = pd.DataFrame()
        for dbid in dbids:
            df = self._get_recursive_ids([dbid], is_get_sub_family, display_unit)
            dataframe = pd.concat([dataframe, df], ignore_index=True)
        return dataframe

    def get_all_families(self) -> dict:
        """
        Get all families in model
        :return:  :class:`dict` : Dictionary contains all families, key is dbId, value is family name
        """
        families = {}
        self._get_recursive_child(families, 1, "_RFN")
        return families

    def get_all_families_types(self) -> dict:
        """
        Get all families types in model
        :return:  :class:`dict` : Dictionary contains all families types, key is dbId, value is family type name
        """
        families_types = {}
        self._get_recursive_child(families_types, 1, "_RFT")
        return families_types

    def get_data_by_category(self, category: str, is_get_sub_family: bool = False,
                             display_unit: bool = False) -> pd.DataFrame:
        """
        Get data by category in model
        :param category: the category name need get data, e.g: Walls, Doors, Windows, etc
        :param is_get_sub_family: the flag to get sub family or not, default is False
        :param display_unit: the flag to display unit or not in value, default is False
        :return: :class:`pandas.DataFrame` : Dataframe contains data by category
        """
        categories = self.get_all_categories()
        # if category starts with Revit, remove it
        if category.startswith("Revit"):
            category = category[5:].strip()
        category_id = [key for key, value in categories.items() if value == category]
        dataframe = self._get_recursive_ids(category_id, is_get_sub_family, display_unit)
        return dataframe

    def get_data_by_categories(self, categories: List[str], is_get_sub_family: bool = False,
                               display_unit: bool = False) -> pd.DataFrame:
        """
        Get data by list of categories in model
        :param categories: the list of categories need get data, e.g: ["Walls", "Doors", "Windows"]
        :param is_get_sub_family: the flag to get sub family or not, default is False
        :param display_unit: the flag to display unit or not in value, default is False
        :return: :class:`pandas.DataFrame` : Dataframe contains data by categories
        """
        dataframe = pd.DataFrame()
        for category in categories:
            dataframe = pd.concat([dataframe, self.get_data_by_category(category, is_get_sub_family, display_unit)],
                                  ignore_index=True)
        return dataframe

    def get_data_by_categories_and_params(self, categories: List[str], params: List[str],
                                          is_get_sub_family: bool = False, display_unit: bool = False) -> pd.DataFrame:
        """
        Get data by list of categories and list of parameters in model
        :param categories: the list of categories need get data, e.g: ["Walls", "Doors", "Windows"]
        :param params: the list of parameters need get data, e.g: ["Name", "Area", "Volume", "Height"]
        :param is_get_sub_family: the flag to get sub family or not, default is False
        :param display_unit: the flag to display unit or not in value, default is False
        :return: :class:`pandas.DataFrame` : Dataframe contains data by categories and parameters
        """
        dataframe = pd.DataFrame()
        all_categories = self.get_all_categories()
        category_ids = [key for key, value in all_categories.items() if value in categories]
        for category_id in category_ids:
            dataframe = pd.concat(
                [dataframe, self._get_recursive_ids_prams([category_id], params, is_get_sub_family, display_unit)],
                ignore_index=True)
        # remove all row have all values is null, ignore dbId and external_id columns
        dataframe = dataframe.dropna(how='all',
                                     subset=[col for col in dataframe.columns if col not in ['dbId', 'external_id']])
        return dataframe

    def get_data_by_family(self, family_name: str, is_get_sub_family: bool = False,
                           display_unit: bool = False) -> pd.DataFrame:
        """
        Get data by family name in model
        :param family_name: the family name need get data, e.g: "Seating-LAMMHULTS-PENNE-Chair"
        :param is_get_sub_family: the flag to get sub family or not, default is False
        :param display_unit: the flag to display unit or not in value, default is False
        :return: :class:`pandas.DataFrame` : Dataframe contains data by family name
        """
        families = self.get_all_families()
        category_id = [key for key, value in families.items() if value == family_name]
        dataframe = self._get_recursive_ids(category_id, is_get_sub_family, display_unit)
        return dataframe

    def get_data_by_family_type(self, family_type: str, is_get_sub_family: bool = False,
                                display_unit: bool = False) -> pd.DataFrame:
        """
        Get data by family type in model
        :param family_type:  the family type name need get data, e.g: "Plastic-Seat"
        :param is_get_sub_family:  the flag to get sub family or not, default is False
        :param display_unit:  the flag to display unit or not in value, default is False
        :return: :class:`pandas.DataFrame` : Dataframe contains data by family type
        """
        family_types = self.get_all_families_types()
        type_id = [key for key, value in family_types.items() if value == family_type]
        dataframe = self._get_recursive_ids(type_id, is_get_sub_family, display_unit)
        return dataframe

    def _get_recursive_ids(self, db_ids: List[int], get_sub_family: bool, display_unit: bool = False) -> pd.DataFrame:
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
                dataframe = pd.concat([dataframe, self._get_recursive_ids(ids, get_sub_family, display_unit)],
                                      ignore_index=True)
                continue
            for prop in props:
                if prop.category == "__internalref__" and prop.name == "Sub Family":
                    flag_sub_families = True
                if prop.name not in props_ignore:
                    if prop.name == "name":
                        properties["Name"] = prop.value
                    else:
                        if display_unit:
                            if prop.data_type_context not in ["", None]:
                                properties[prop.name] = str(prop.value) + " " + str(
                                    self.units.parse_symbol(prop.data_type_context))
                            else:
                                properties[prop.name] = prop.value
                        else:
                            properties[prop.name] = prop.value
            db_id = id
            external_id = self.ids[id]
            properties['dbId'] = db_id
            properties['external_id'] = external_id
            if flag_sub_families and not get_sub_family:
                ids = self.get_children(id)
                dataframe = pd.concat([dataframe, self._get_recursive_ids(ids, get_sub_family, display_unit)],
                                      ignore_index=True)
                continue
            ins = self.get_instance(id)
            if len(ins) > 0:
                for instance in ins:
                    if display_unit:
                        types = self.get_all_properties_display_unit(instance)
                    else:
                        types = self.get_properties(instance)
                    properties = {**properties, **types}
            singleDF = pd.DataFrame(properties, index=[0])
            dataframe = pd.concat([dataframe, singleDF], ignore_index=True)
            ids = self.get_children(id)
            dataframe = pd.concat([dataframe, self._get_recursive_ids(ids, get_sub_family, display_unit)],
                                  ignore_index=True)
        if 'dbId' in dataframe.columns and 'external_id' in dataframe.columns:
            dataframe = dataframe[
                ['dbId', 'external_id'] + [col for col in dataframe.columns if col not in ['dbId', 'external_id']]]
        return dataframe

    def _get_recursive_ids_prams(self, childs: List[int], params: List[str], get_sub_family: bool,
                                 display_unit: bool = False) -> pd.DataFrame:
        """
        Get recursive ids by list of parameters
        :param childs:  List of child ids, ids is database id
        :param params: List of parameters need get data
        :param get_sub_family: the flag to get sub family or not
        :param display_unit: the flag to display unit or not in value
        :return:
        """
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
                dataframe = pd.concat(
                    [dataframe, self._get_recursive_ids_prams(ids, params, get_sub_family, display_unit)],
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
                        if display_unit:
                            if prop.data_type_context not in ["", None]:
                                properties[prop.name] = str(prop.value) + " " + str(
                                    self.units.parse_symbol(prop.data_type_context))
                            else:
                                properties[prop.name] = prop.value
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
                dataframe = pd.concat(
                    [dataframe, self._get_recursive_ids_prams(ids, params, get_sub_family, display_unit)],
                    ignore_index=True)
                continue
            # get instances
            ins = self.get_instance(id)
            if len(ins) > 0:
                for instance in ins:
                    if display_unit:
                        types = self.get_all_properties_display_unit(instance)
                    else:
                        types = self.get_all_properties(instance)
                    types = {k: v for k, v in types.items() if k in params}
                    # add unit to value
                    if display_unit:
                        for key, value in types.items():
                            if key in params:
                                if self.units is not None:
                                    types[key] = str(value) + " " + str(self.units.parse_symbol(key))
                    properties = {**properties, **types}
            single_df = pd.DataFrame(properties, index=[0])
            dataframe = pd.concat([dataframe, single_df], ignore_index=True)
            ids = self.get_children(id)
            dataframe = pd.concat([dataframe, self._get_recursive_ids_prams(ids, params, get_sub_family, display_unit)],
                                  ignore_index=True)
        # set dbid and external_id to first and second column if it exists
        if 'dbId' in dataframe.columns and 'external_id' in dataframe.columns:
            dataframe = dataframe[
                ['dbId', 'external_id'] + [col for col in dataframe.columns if col not in ['dbId', 'external_id']]]
        return dataframe

    def get_data_by_external_id(self, external_id: str, is_get_sub_family: bool = False,
                                display_unit: bool = False) -> pd.DataFrame:
        """
        Get data by external id(UniqueId Element) in model
        :param external_id:  The unique id of element in model
        :param is_get_sub_family:  the flag to get sub family or not, default is False
        :param display_unit:  the flag to display unit or not in value, default is False
        :return: :class:`pandas.DataFrame` : Dataframe contains data by external id
        """
        db_id = None
        for idx in range(0, len(self.ids)):
            if self.ids[idx] == external_id:
                db_id = idx
                break
        if db_id is None:
            return pd.DataFrame()
        dataframe = self._get_recursive_ids([db_id], is_get_sub_family, display_unit)
        return dataframe

    def get_data_by_element_id(self, element_id: str) -> dict:
        """
        Get data by element id in model
        :param element_id:  the element id of element in model. e.g: 9895625
        :return: :class:`dict` : Dictionary contains data by element id
        """
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
        """
        Get all parameters in model.
        e.g: ["Area", "Volume", "Height", "Width", "Name", "Category", "ElementId", "IfcGUID"]
        :return:  :class:`list` : List contains all parameters.
        """
        parameters = []
        for id in range(0, len(self.ids)):
            props_dict = self.get_properties(id)
            for key, value in props_dict.items():
                if key not in parameters:
                    parameters.append(key)
        parameters = list(set(parameters))
        parameters.sort()
        return parameters
