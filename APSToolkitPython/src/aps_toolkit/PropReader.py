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
import gzip
import json
import re
import codecs
import requests
from .Derivative import Derivative
from .ManifestItem import ManifestItem
import pandas as pd
from typing import List
from .units.DisplayUnits import DisplayUnits
from .Token import Token
import concurrent.futures
import os


class PropReader:

    def __init__(self, urn: str = None, token: Token = None, region: str = "US", manifest_item: [ManifestItem] = None):
        # get manifest
        self.host = "https://developer.api.autodesk.com"
        self.urn = urn
        self.token = token
        self.region = region
        if manifest_item:
            derivative = Derivative(self.urn, self.token, self.region)
            self._read_metadata_item(derivative, manifest_item)
        else:
            self._read_metadata()
        self.units = DisplayUnits()

    @classmethod
    def read_from_resource(cls, path):
        """
        Initialize PropReader from svf extracted
        :param path: path of resource extracted by svf, e.g. "path/to/3D.svf" or "path/../Resource"
        :remark :see tutorial at https://chuongmep.com/posts/2024-09-25-revit-extractor.html
        :return: Instance
        """
        parrent_dir = path
        if path.endswith(".svf"):
            parrent_dir = os.path.abspath(os.path.join(path, os.pardir))
            parrent_dir = os.path.abspath(os.path.join(parrent_dir, os.pardir))
            parrent_dir = os.path.abspath(os.path.join(parrent_dir, os.pardir))
        if path.endswith("output"):
            # find sub folder have "Resource" name and stop
            for root, dirs, files in os.walk(path):
                if "Resource" in dirs:
                    parrent_dir = os.path.join(root, "Resource")
                    break
        if not os.path.exists(parrent_dir):
            raise Exception(f"Directory {parrent_dir} not found")
        if not parrent_dir.endswith("Resource"):
            raise Exception(f"Directory {parrent_dir} is not Resource")
        ids_path = os.path.join(parrent_dir, "objects_ids.json.gz")
        if not os.path.exists(ids_path):
            raise Exception(f"File {ids_path} not found")
        offsets_path = os.path.join(parrent_dir, "objects_offs.json.gz")
        if not os.path.exists(offsets_path):
            raise Exception(f"File {offsets_path} not found")
        avs_path = os.path.join(parrent_dir, "objects_avs.json.gz")
        if not os.path.exists(avs_path):
            raise Exception(f"File {avs_path} not found")
        attrs_path = os.path.join(parrent_dir, "objects_attrs.json.gz")
        if not os.path.exists(attrs_path):
            raise Exception(f"File {attrs_path} not found")
        vals_path = os.path.join(parrent_dir, "objects_vals.json.gz")
        if not os.path.exists(vals_path):
            raise Exception(f"File {vals_path} not found")
        return cls.read_from_json_gzip_files(ids_path, offsets_path, avs_path, attrs_path, vals_path)
    @classmethod
    def read_from_json_gzip_files(cls, ids_path: str, offsets_path: str, avs_path: str, attrs_path: str,
                                  vals_path: str):
        # unzip and load json files
        with open(ids_path, 'rb') as f:
            ids = json.loads(codecs.decode(gzip.decompress(f.read()), 'utf-8'))
        with open(offsets_path, 'rb') as f:
            offsets = json.loads(codecs.decode(gzip.decompress(f.read()), 'utf-8'))
        with open(avs_path, 'rb') as f:
            avs = json.loads(codecs.decode(gzip.decompress(f.read()), 'utf-8'))
        with open(attrs_path, 'rb') as f:
            attrs = json.loads(codecs.decode(gzip.decompress(f.read()), 'utf-8'))
        with open(vals_path, 'rb') as f:
            vals = json.loads(codecs.decode(gzip.decompress(f.read()), 'utf-8'))
        instance = cls.__new__(cls)
        instance.ids = ids
        instance.offsets = offsets
        instance.avs = avs
        instance.attrs = attrs
        instance.vals = vals
        instance.units = DisplayUnits()
        return instance

    def _read_metadata(self):
        derivative = Derivative(self.urn, self.token, self.region)
        manifest_items = derivative.read_svf_manifest_items()
        if len(manifest_items) > 0:
            self._read_metadata_item(derivative, manifest_items[0])
        else:
            raise Exception("No manifest item found")

    def _read_metadata_item(self, derivative, manifest_item):
        items = [
            "objects_attrs.json.gz",
            "objects_vals.json.gz",
            "objects_ids.json.gz",
            "objects_offs.json.gz",
            "objects_avs.json.gz",
            "objects_ids.json.gz"
        ]
        resources = derivative.read_svf_resource_item(manifest_item)
        downloaded_files = {}
        access_token = self.token.access_token
        if not access_token:
            raise Exception("No access token found")
        headers = {
            "Authorization": f"Bearer {access_token}",
            "region": self.region
        }

        def download_file(source):
            if source.file_name in items:
                try:
                    response = requests.get(source.url, headers=headers)
                    if response.status_code == 200:
                        return source.file_name, response.content
                    else:
                        print(f"Failed to download {source.file_name}: {response.status_code}")
                except Exception as e:
                    print(f"Error downloading {source.file_name}: {e}")
            return None, None

        with concurrent.futures.ThreadPoolExecutor() as executor:
            future_to_source = {executor.submit(download_file, source): source for source in resources}
            for future in concurrent.futures.as_completed(future_to_source):
                source = future_to_source[future]
                try:
                    file_name, file_bytes = future.result()
                    if file_name and file_bytes:
                        downloaded_files[file_name] = file_bytes
                except Exception as e:
                    print(f"Error processing {source.file_name}: {e}")

        required_files = ["objects_ids.json.gz", "objects_offs.json.gz", "objects_avs.json.gz", "objects_attrs.json.gz",
                          "objects_vals.json.gz"]

        # Ensure all required files are downloaded
        missing_files = [file for file in required_files if file not in downloaded_files]
        if missing_files:
            raise Exception(f"Missing required files: {missing_files}")

        self.ids = json.loads(codecs.decode(gzip.decompress(downloaded_files["objects_ids.json.gz"]), 'utf-8'))
        self.offsets = json.loads(codecs.decode(gzip.decompress(downloaded_files["objects_offs.json.gz"]), 'utf-8'))
        self.avs = json.loads(codecs.decode(gzip.decompress(downloaded_files["objects_avs.json.gz"]), 'utf-8'))
        self.attrs = json.loads(codecs.decode(gzip.decompress(downloaded_files["objects_attrs.json.gz"]), 'utf-8'))
        self.vals = json.loads(codecs.decode(gzip.decompress(downloaded_files["objects_vals.json.gz"]), 'utf-8'))

    def enumerate_properties(self, id) -> list:
        """
        Get all properties of an object
        :param id: database id storage in the manifest file
        :return: list of properties
        """
        properties = []
        if 0 < id < len(self.offsets):
            av_start = 2 * self.offsets[id]
            av_end = len(self.avs) if id == len(self.offsets) - 1 else 2 * self.offsets[id + 1]
            for i in range(av_start, av_end, 2):
                attr_offset = self.avs[i]
                val_offset = self.avs[i + 1]
                attr_obj = self.attrs[attr_offset]

                # Check if attr_obj is a list and has at least two elements
                if isinstance(attr_obj, list) and len(attr_obj) >= 2:
                    property_id = self.ids[id]
                    name = attr_obj[0]
                    category = attr_obj[1]
                    data_type = self.attrs[2]
                    data_type_context = attr_obj[3]
                    description = attr_obj[4]
                    display_name = attr_obj[5]
                    flags = attr_obj[6]
                    display_precision = attr_obj[7]
                    forge_parameter_id = attr_obj[8]
                value = self.vals[val_offset]
                properties.append(
                    Property(property_id, name, category, data_type, data_type_context, description, display_name,
                             flags,
                             display_precision, forge_parameter_id, value))
        return properties

    """
    Get all properties exclude internal properties
    """

    def get_properties(self, id) -> dict:
        """
        Get all properties exclude internal properties
        :param id:
        :return:
        """
        props = {}
        rg = re.compile(r'^__\w+__$')
        for prop in self.enumerate_properties(id):
            if prop.category and not rg.match(prop.category):
                props[prop.name] = prop.value
        return props

    def get_all_properties(self, id) -> dict:
        """
            Get all properties include internal properties
        """
        props = {}
        for prop in self.enumerate_properties(id):
            props[prop.name] = prop.value
        return props

    def get_entities_table(self) -> pd.DataFrame:
        data = self.ids
        df = pd.DataFrame(data, columns=["dbId"])
        return df

    def get_values_table(self) -> pd.DataFrame:
        data = self.vals
        df = pd.DataFrame(data, columns=["value"])
        return df

    def get_attributes_table(self) -> pd.DataFrame:
        data = self.attrs
        data = data[1:]
        df = pd.DataFrame(data, columns=['name', 'category', 'data_type', 'data_type_context', 'description',
                                         'display_name', 'flags', 'display_precision', 'forge_parameter_id'])
        return df

    def get_avs_table(self) -> pd.DataFrame:
        data = self.avs
        df = pd.DataFrame(data, columns=['entId'])
        return df

    def get_offsets_table(self) -> pd.DataFrame:
        data = self.offsets
        df = pd.DataFrame(data, columns=['offset'])
        return df

    def get_all_properties_display_unit(self, id) -> dict:
        """
        Get all properties include internal properties with display unit value
        :param id: database id storage in the manifest file
        :return: :class:`dict` of properties, key is property name, value is property value with display unit
        """
        props = {}
        for prop in self.enumerate_properties(id):
            value = prop.value
            unit_type = prop.data_type_context
            if unit_type not in ["", None]:
                unit = self.units.parse_symbol(unit_type)
                props[prop.name] = f"{value} {unit}"
            else:
                props[prop.name] = value
        return props

    def get_property_values_by_names(self, names: List[str]) -> dict:
        result = {}
        for i in range(len(self.offsets)):
            av_start = 2 * self.offsets[i]
            av_end = len(self.avs) if i == len(self.offsets) - 1 else 2 * self.offsets[i + 1]
            for j in range(av_start, av_end, 2):
                attr_offset = self.avs[j]
                attr_obj = self.attrs[attr_offset]
                if isinstance(attr_obj, list) and len(attr_obj) >= 2:
                    name = attr_obj[0]
                    if name in names:
                        value = self.vals[self.avs[j + 1]]
                        values = result.get(name, [])
                        if value not in values:
                            values.append(value)
                            result[name] = values
        return result

    def get_property_values_by_display_names(self, display_names: List[str]) -> dict:
        """
        Get property values by display names. e.g. : ["Name", "Category,"Comments"]
        :param display_names: list of display names to get values
        :return:
        """
        result = {}
        for i in range(len(self.offsets)):
            av_start = 2 * self.offsets[i]
            av_end = len(self.avs) if i == len(self.offsets) - 1 else 2 * self.offsets[i + 1]
            for j in range(av_start, av_end, 2):
                attr_offset = self.avs[j]
                attr_obj = self.attrs[attr_offset]
                if isinstance(attr_obj, list) and len(attr_obj) >= 2:
                    display_name = attr_obj[5]
                    if display_name in display_names:
                        value = self.vals[self.avs[j + 1]]
                        values = result.get(display_name, [])
                        if value not in values:
                            values.append(value)
                            result[display_name] = values

        return result

    def get_properties_group_by_category(self, id) -> dict:
        """
        Get all properties group by category Property.
        :param id: database id storage in the manifest file
        :return: :class:`dict` of properties, key is category name, value is list of properties
        """
        properties = {}
        rg = re.compile(r'^__\w+__$')
        categories = []

        props = self.enumerate_properties(id)
        for prop in props:
            if prop.category:
                if not rg.match(prop.category):
                    if not any(category in prop.category for category in categories):
                        categories.append(prop.category)

        for category in categories:
            prop_result = [prop for prop in props if prop.category == category]
            prop_dictionary = []
            for prop in prop_result:
                prop_key = prop.name
                prop_dictionary.append((prop_key, prop.value))
            properties[category] = prop_dictionary

        return properties

    def get_children(self, id) -> list:
        """
        Get all children of an object
        :param id: database id storage in the manifest file
        :return: list of children (database id)
        """
        children = []
        for prop in self.enumerate_properties(id):
            if prop.category == "__child__":
                children.append(int(prop.value))
        return children

    def get_parent(self, id) -> list:
        """
        Get all parent of an object
        :param id: database id storage in the manifest file
        :return: list of parent (database id)
        """
        parent = []
        for prop in self.enumerate_properties(id):
            if prop.category == "__parent__":
                parent.append(int(prop.value))
        return parent

    def get_instance(self, id) -> list:
        """
        Get all instance of an object
        :param id:  database id storage in the manifest file
        :return:  list of instance (database id)
        """
        instance_of = []
        for prop in self.enumerate_properties(id):
            if prop.category == "__instanceof__":
                instance_of.append(int(prop.value))
        return instance_of

    def get_internal_ref(self, id) -> list:
        reference = []
        for prop in self.enumerate_properties(id):
            if prop.category == "__internalref__":
                reference.append(int(prop.value))

    # TODO : It too slow, need find another way to get all data with less time and cover all format : dwg, rvt, nwd, ifc, ...
    # def get_all_data(self) -> pd.DataFrame:
    #     db_index_ids = [i for i in range(len(self.offsets))]
    #     return self.get_recursive_ids(db_index_ids)

    def get_recursive_ids(self, db_ids: List[int]) -> pd.DataFrame:
        """
        Get all properties of all objects
        :param db_ids:  list of database id storage in the manifest file
        :return:  :class:`pandas.DataFrame` of properties
        """
        dataframe = pd.DataFrame()
        props_ignore = ['parent', 'instanceof_objid', 'child', "viewable_in"]
        if len(db_ids) == 0:
            return dataframe
        for id in db_ids:
            props = self.enumerate_properties(id)
            properties = {}
            for prop in props:
                if prop.name == 'name':
                    continue
                if prop.name not in props_ignore:
                    properties[prop.name] = prop.value
            db_id = id
            properties['dbId'] = db_id
            ins = self.get_instance(id)
            if len(ins) > 0:
                for instance in ins:
                    types = self.get_properties(instance)
                    properties = {**properties, **types}
            singleDF = pd.DataFrame(properties, index=[0])
            dataframe = pd.concat([dataframe, singleDF], ignore_index=True)
            ids = self.get_children(id)
            dataframe = pd.concat([dataframe, self.get_recursive_ids(ids)], ignore_index=True)
        if 'dbId' in dataframe.columns:
            dataframe = dataframe[
                ['dbId'] + [col for col in dataframe.columns if col not in ['dbId']]]
        return dataframe

    def get_recursive_ids_by_parameters(self, db_ids: List[int], params: List[str]) -> pd.DataFrame:
        """
        Get all properties of all objects by parameters
        :param db_ids:  list of database id storage in the manifest file
        :param params:  list of parameters to get
        :return:  :class:`pandas.DataFrame` of properties
        """
        dataframe = pd.DataFrame()
        props_ignore = ['parent', 'instanceof_objid', 'child', "viewable_in"]
        if len(db_ids) == 0:
            return dataframe
        for id in db_ids:
            props = self.enumerate_properties(id)
            properties = {}
            for prop in props:
                if prop.name == 'name':
                    continue
                if prop.name not in props_ignore:
                    properties[prop.name] = prop.value
            db_id = id
            properties = {k: v for k, v in properties.items() if k in params}
            properties['dbId'] = db_id
            ins = self.get_instance(id)
            if len(ins) > 0:
                for instance in ins:
                    types = self.get_properties(instance)
                    types = {k: v for k, v in types.items() if k in params}
                    properties = {**properties, **types}
            singleDF = pd.DataFrame(properties, index=[0])
            dataframe = pd.concat([dataframe, singleDF], ignore_index=True)
            ids = self.get_children(id)
            dataframe = pd.concat([dataframe, self.get_recursive_ids_by_parameters(ids, params)], ignore_index=True)
        if 'dbId' in dataframe.columns:
            dataframe = dataframe[['dbId'] + [col for col in dataframe.columns if col not in ['dbId']]]
        return dataframe

    def get_all_properties_names(self) -> List[str]:
        """
        Get all properties names of all objects
        :return: :class:`list` of properties names
        """
        props_names = []
        for i in range(len(self.offsets)):
            av_start = 2 * self.offsets[i]
            av_end = len(self.avs) if i == len(self.offsets) - 1 else 2 * self.offsets[i + 1]
            for j in range(av_start, av_end, 2):
                attr_offset = self.avs[j]
                attr_obj = self.attrs[attr_offset]
                if isinstance(attr_obj, list) and len(attr_obj) >= 2:
                    name = attr_obj[0]
                    props_names.append(name)
        props_names = list(set(props_names))
        props_names.sort()
        return props_names


class Property():
    def __init__(self, id=None, name=None, category=None, data_type=None, data_type_context=None, description=None,
                 display_name=None, flags=None,
                 display_precision=None, forge_parameter_id=None, value=None):
        self.id = id,
        self.name = name
        self.category = category
        self.data_type = data_type
        self.data_type_context = data_type_context
        self.description = description
        self.display_name = display_name
        self.flags = flags
        self.display_precision = display_precision
        self.forge_parameter_id = forge_parameter_id
        self.value = value
