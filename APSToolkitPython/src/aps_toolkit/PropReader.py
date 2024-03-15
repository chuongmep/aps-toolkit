import gzip
import json
import re
import codecs
import requests


class PropReader:

    # def __init__(self, arg1, arg2=None, arg3=None, arg4=None, arg5=None):
    #     if isinstance(arg1, bytes):
    #         # If the first argument is bytes, assume buffer initialization
    #         self.ids = json.loads(codecs.decode(gzip.decompress(arg1), 'utf-8'))
    #         self.offsets = json.loads(codecs.decode(gzip.decompress(arg2), 'utf-8'))
    #         self.avs = json.loads(codecs.decode(gzip.decompress(arg3), 'utf-8'))
    #         self.attrs = json.loads(codecs.decode(gzip.decompress(arg4), 'utf-8'))
    #         self.vals = json.loads(codecs.decode(gzip.decompress(arg5), 'utf-8'))
    #     else:
    #         # Otherwise, assume file path initialization
    #         ids_path, offsets_path, avs_path, attrs_path, vals_path = arg1, arg2, arg3, arg4, arg5
    #         with gzip.open(ids_path, 'rb') as ids_file, gzip.open(offsets_path, 'rb') as offsets_file, \
    #                 gzip.open(avs_path, 'rb') as avs_file, gzip.open(attrs_path, 'rb') as attrs_file, \
    #                 gzip.open(vals_path, 'rb') as vals_file:
    #             self.ids = json.load(ids_file)
    #             self.offsets = json.load(offsets_file)
    #             self.avs = json.load(avs_file)
    #             self.attrs = json.load(attrs_file)
    #             self.vals = json.load(vals_file)
    def __init__(self, urn, token, region="US"):
        items = [
            "objects_attrs.json.gz",
            "objects_vals.json.gz",
            "objects_ids.json.gz",
            "objects_offs.json.gz",
            "objects_avs.json.gz",
            "objects_ids.json.gz"
        ]
        # get manifest
        self.host = "https://developer.api.autodesk.com"
        self.urn = urn
        self.token = token
        self.region = region
        URL = f"{self.host}/modelderivative/v2/designdata/{self.urn}/manifest"
        access_token = token.access_token
        # add headers authorization
        headers = {
            "Authorization": f"Bearer {access_token}",
            "region": region
        }
        # request
        response = requests.get(URL, headers=headers)
        if response.status_code != 200:
            print(response.reason)
            return
        json_response = response.json()
        children = json_response['derivatives'][0]["children"]
        path = ""
        for child in children:
            if child["type"] == "resource" and child["mime"] == "application/autodesk-db":
                path = child["urn"]
                break
        downloaded_files = {}
        for item in items:
            path = f"urn:adsk.viewing:fs.file:{self.urn}/output/Resource/{item}"
            url = f"{self.host}/modelderivative/v2/designdata/{self.urn}/manifest/{path}"
            # add headers authorization
            headers = {
                "Authorization": f"Bearer {access_token}",
                "region": region
            }
            response = requests.get(url, headers=headers)
            if response.status_code == 200:
                file_bytes = response.content
                downloaded_files[item] = file_bytes
            else:
                print(response.reason)
                return
        self.ids = json.loads(codecs.decode(gzip.decompress(downloaded_files["objects_ids.json.gz"]), 'utf-8'))
        self.offsets = json.loads(codecs.decode(gzip.decompress(downloaded_files["objects_offs.json.gz"]), 'utf-8'))
        self.avs = json.loads(codecs.decode(gzip.decompress(downloaded_files["objects_avs.json.gz"]), 'utf-8'))
        self.attrs = json.loads(codecs.decode(gzip.decompress(downloaded_files["objects_attrs.json.gz"]), 'utf-8'))
        self.vals = json.loads(codecs.decode(gzip.decompress(downloaded_files["objects_vals.json.gz"]), 'utf-8'))

    def enumerate_properties(self, id) -> list:
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
        props = {}
        rg = re.compile(r'^__\w+__$')
        for prop in self.enumerate_properties(id):
            if prop.category and not rg.match(prop.category):
                props[prop.name] = prop.value
        return props

    """
    Get all properties include internal properties
    """

    def get_all_properties(self, id) -> dict:
        props = {}
        for prop in self.enumerate_properties(id):
            props[prop.name] = prop.value
        return props

    def get_properties_group_by_category(self, id) -> dict:
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
        children = []
        for prop in self.enumerate_properties(id):
            if prop.category == "__child__":
                children.append(int(prop.value))
        return children

    def get_parent(self, id) -> list:
        parent = []
        for prop in self.enumerate_properties(id):
            if prop.category == "__parent__":
                parent.append(int(prop.value))
        return parent

    def get_instance(self, id) -> list:
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
