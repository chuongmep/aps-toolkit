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
import requests
import gzip
from io import BytesIO
import zipfile
from json import loads as json_loads
from typing import List
from urllib.parse import unquote
import re
from os.path import join, normpath
import os
from .PathInfo import PathInfo
from .Resource import Resource
from .ManifestItem import ManifestItem
from .Token import Token
import json
import pandas as pd
import time

class Derivative:
    def __init__(self, urn: str, token: Token, region: str = "US"):
        self.urn = urn
        self.token = token
        self.region = region
        self.host = "https://developer.api.autodesk.com"

    def translate_job(self, root_file_name: str, type: str = "svf", generate_master_views: bool = False):
        url = "https://developer.api.autodesk.com/modelderivative/v2/designdata/job"
        access_token = self.token.access_token
        if not access_token:
            raise Exception("Have no access token to translate job.")
        payload = json.dumps({
            "input": {
                "urn": self.urn,
                "rootFilename": root_file_name
            },
            "output": {
                "destination": {
                    "region": self.region.lower()
                },
                "formats": [
                    {
                        "type": type,
                        "views": [
                            "2d",
                            "3d"
                        ],
                        "advanced": {
                            "generateMasterViews": generate_master_views
                        }
                    }
                ]
            }
        })

        headers = {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + access_token,
            'x-ads-force': 'true'
        }
        response = requests.post(url, headers=headers, data=payload)
        return response.text

    def download_stream_ifc(self,ifc_setting_name="IFC 2x3 GSA Concept Design BIM 2010",is_translate=False):
        """
        Download IFC file from the model
        :param ifc_setting_name: the setting name of IFC format, see more at : https://help.autodesk.com/view/RVT/2024/ENU/?guid=GUID-E029E3AD-1639-4446-A935-C9796BC34C95
        :param is_translate: True if you want post a translation job for convert to IFC format (Cost Will be charged)
        :return:
        """
        response = self.check_job_status()
        process = response["progress"]
        while process != "complete" and response['status'] == 'inprogress':
            time.sleep(5)
            response = self.check_job_status()
            process = response["progress"]
        derivatives = response["derivatives"]
        if is_translate:
            # start call translate job
            response = self.translate_to_ifc(ifc_setting_name)
            if response.status_code != 200:
                raise Exception("Translate job failed.Return: " + response.text)
            # check job status
            response = self.check_job_status()
            process = response["progress"]
            while process != "complete" and response['status']=='inprogress':
                time.sleep(5)
                response = self.check_job_status()
                process = response["progress"]
            # get all derivatives
            derivatives = response["derivatives"]
        # find in list derivatives any item has outputType is "ifc"
        for derivative in derivatives:
            if derivative["outputType"] == "ifc":
                # get the manifest of the derivative
                children = derivative["children"][0] if "children" in derivative else []
                if not children:
                    raise Exception("The derivative has no Urn IFC.")
                guid = children["guid"]
                mime = children["mime"]
                path_info = self._decompose_urn(children["urn"])
                urn_json = children["urn"]
                manifest_item = ManifestItem(guid, mime, path_info, urn_json)
                local_path = path_info.local_path
                myUri = "file://" + path_info.base_path + path_info.root_file_name
                remote_path = join("derivativeservice/v2/derivatives/", unquote(myUri)[len("file://"):])
                resource = Resource(manifest_item.path_info.root_file_name, remote_path, local_path)
                # download the IFC file
                stream = self.download_stream_resource(resource)
                return {"file_name": manifest_item.path_info.root_file_name, "stream": stream}
        return None
    def download_ifc(self, file_path=None,ifc_setting_name="IFC2x3 Coordination View 2.0"):
        """
        Download IFC file from the model and save it to the file path
        :param file_path: the path to save the IFC file e.g: "C:/Users/<your name>/Downloads/IFC.ifc"
        :param ifc_setting_name: the setting name of IFC format, see more at : https://help.autodesk.com/view/RVT/2024/ENU/?guid=GUID-E029E3AD-1639-4446-A935-C9796BC34C95
        :return:
        """
        result_dict = self.download_stream_ifc(ifc_setting_name)
        if result_dict is None:
            raise Exception("Can not download IFC file.Please check stream download process.")
        if file_path is None:
            file_path = result_dict["file_name"]
        # in case file exits, remove it
        if os.path.exists(file_path):
            try:
                os.remove(file_path)
            except Exception as e:
                raise Exception(f"Can not remove file {file_path}. Reason: {e}")
        bytes_io = result_dict["stream"]
        with open(file_path, "wb") as f:
            f.write(bytes_io.read())
        return file_path

    def translate_to_ifc(self, ifc_setting_name="IFC 2x3 GSA Concept Design BIM 2010")-> requests.Response:
        """
        Run a Job to translate the model to IFC format:
        https://aps.autodesk.com/blog/export-ifc-rvt-using-model-derivative-api
        More about ifc setting name : https://help.autodesk.com/view/RVT/2024/ENU/?guid=GUID-E029E3AD-1639-4446-A935-C9796BC34C95
        :return:
        """
        url = 'https://developer.api.autodesk.com/modelderivative/v2/designdata/job'
        headers = {
            'Authorization': f'Bearer {self.token.access_token}',
            'Content-Type': 'application/json',
            'x-ads-force': 'true'
        }
        data = {
            "input": {
                "urn": self.urn
            },
            "output": {
                "formats": [
                    {
                        "type": "ifc",
                        "advanced": {
                            "exportSettingName": f"{ifc_setting_name}"
                        }
                    }
                ]
            }
        }
        # Make the POST request
        response = requests.post(url, headers=headers, json=data)
        return response

    def check_job_status(self):
        url = f"{self.host}/modelderivative/v2/designdata/{self.urn}/manifest"
        access_token = self.token.access_token
        if not access_token:
            raise Exception("Have no access token to check job status.")
        headers = {
            "Authorization": f"Bearer {access_token}",
            "region": self.region
        }
        response = requests.get(url, headers=headers)
        if response.status_code == 404:
            raise Exception(response.content)
        json_response = response.json()
        return json_response

    def read_svf_manifest_items(self) -> List[ManifestItem]:
        """
        Reads SVF manifest items associated with the URN.

        Returns:
        List[ManifestItem]: A list of SVF manifest items.
        """
        URL = f"{self.host}/modelderivative/v2/designdata/{self.urn}/manifest"
        access_token = self.token.access_token
        if not access_token:
            raise Exception("Have no access token to get manifest items.")
        headers = {
            "Authorization": f"Bearer {access_token}",
            "region": self.region
        }
        # request
        response = requests.get(URL, headers=headers)
        if response.status_code == 404:
            raise Exception(response.reason)
        json_response = response.json()
        if "derivatives" not in json_response:
            raise Exception(f"the manifest not found, recheck authentication or translate job.\nReason: {response.reason}")
        children = json_response['derivatives'][0]["children"]
        manifest_items = []
        image_items = []
        for child in children:
            if child["type"] == "geometry":
                for c in child["children"]:
                    # check if contains 'mime' and 'application/autodesk-svf
                    if "mime" in c and c["mime"] == "application/autodesk-svf":
                        urn_json = c["urn"]
                        json_content = self._unzip_svf(urn_json)
                        path_info = self._decompose_urn(urn_json)
                        path_info.files = self._get_assets(json_content)
                        guid = c["guid"]
                        mime = c["mime"]
                        # add svf files
                        path_info.files.append(path_info.root_file_name)
                        manifest_items.append(ManifestItem(guid, mime, path_info, urn_json))
                    # case mapping image with svf
                    if "type" in c and c["role"] == "thumbnail":
                        guid = c["guid"]
                        mime = c["mime"]
                        urn = c["urn"]
                        path_info = self._decompose_urn(urn)
                        image_items.append(ManifestItem(guid, mime, path_info, urn))
        # add files images to manifest items by mapp with local_path
        for image in image_items:
            for manifest_item in manifest_items:
                if image.path_info.local_path == manifest_item.path_info.local_path:
                    manifest_item.path_info.files.append(image.path_info.root_file_name)
                    continue
        return manifest_items

    def read_svf_resource_item(self, manifest_item: ManifestItem) -> List[Resource]:
        """
        Reads SVF resource items from the manifest item.

        Parameters:
        manifest_item (ManifestItem): The manifest item containing information about SVF resources.

        Returns:
        List[Resource]: A list of SVF resource items extracted from the manifest item.
        """
        resources = []
        derivative_path = "derivativeservice/v2/derivatives/"
        for file in manifest_item.path_info.files:
            file_name = file[file.rfind("/") + 1:]
            uri_local_path = "file://" + manifest_item.path_info.local_path + file
            local_path = unquote(uri_local_path)[len("file://"):]
            # Normalize the path to remove /../../
            local_path = normpath(local_path)
            myUri = "file://" + manifest_item.path_info.base_path + file
            remote_path = join(derivative_path, unquote(myUri)[len("file://"):])
            remote_path = normpath(remote_path)
            resources.append(Resource(file_name, remote_path, local_path))
        return resources

    def read_svf_resource(self) -> dict[str, List[Resource]]:
        """
        Reads SVF resources from the SVF manifest items.

        Returns:
        List[Resource]: A list of resources extracted from the SVF manifest.
        """
        manifest_items = self.read_svf_manifest_items()
        resources = {}
        for manifest_item in manifest_items:
            source_items = self.read_svf_resource_item(manifest_item)
            resources[manifest_item.guid] = source_items
        return resources

    def _unzip_svf(self, svf_urn: str):
        """
        Retrieves and unzips the manifest associated with the given URN.

        Parameters:
        urn (str): The URN of the manifest to retrieve and unzip.

        Returns:
        dict: The contents of the unzipped manifest in JSON format.
        """
        URL = f"{self.host}/modelderivative/v2/designdata/{self.urn}/manifest/{svf_urn}"
        access_token = self.token.access_token
        headers = {
            "Authorization": f"Bearer {access_token}",
            "region": self.region
        }
        response = requests.get(URL, headers=headers)
        if response.status_code == 404:
            raise Exception(response.reason)
        manifest_json = None
        # unzip it
        if ".gz" in response.headers.get("Content-Type", ""):
            with gzip.GzipFile(fileobj=BytesIO(response.content), mode="rb") as gzip_file:
                manifest_json = json_loads(gzip_file.read().decode("utf-8"))
        else:
            with zipfile.ZipFile(BytesIO(response.content)) as zip_file:
                with zip_file.open("manifest.json") as manifest_data:
                    manifest_json = json_loads(manifest_data.read().decode("utf-8"))

        return manifest_json

    def read_svf_metadata(self, svf_urn: str):
        """
        Retrieves and unzips the manifest associated with the given URN.

        Parameters:
        urn (str): The URN of the manifest to retrieve and unzip.

        Returns:
        dict: The contents of the unzipped manifest in JSON format.
        """
        URL = f"{self.host}/modelderivative/v2/designdata/{self.urn}/manifest/{svf_urn}"
        access_token = self.token.access_token
        headers = {
            "Authorization": f"Bearer {access_token}",
            "region": self.region
        }
        response = requests.get(URL, headers=headers)
        if response.status_code == 404:
            raise Exception(response.reason)
        meta_data_json = None
        # unzip it
        if ".gz" in response.headers.get("Content-Type", ""):
            with gzip.GzipFile(fileobj=BytesIO(response.content), mode="rb") as gzip_file:
                meta_data_json = json_loads(gzip_file.read().decode("utf-8"))
        else:
            with zipfile.ZipFile(BytesIO(response.content)) as zip_file:
                with zip_file.open("metadata.json") as manifest_data:
                    meta_data_json = json_loads(manifest_data.read().decode("utf-8"))

        return meta_data_json

    def _get_assets(self, manifest) -> List[str]:
        """
        Extracts asset URIs from the given manifest.

        Parameters:
        manifest (dict): The manifest containing information about assets.

        Returns:
        List[str]: A list of asset URIs extracted from the manifest.
        """
        files = []
        # Iterate over each "asset" in the manifest
        for asset in manifest.get("assets", []):
            uri = asset.get("URI", "")
            if "embed:/" in uri:
                continue
            files.append(uri)

        return files

    def _decompose_urn(self, encodedUrn: str):
        """
            Decomposes the given encoded URN into its constituent parts.

            Parameters:
            encodedUrn (str): The encoded URN to be decomposed.

            Returns:
            PathInfo: An object containing the decomposed parts of the URN,
                      including root filename, base path, local path, and the original URN.
        """
        urn = unquote(encodedUrn)

        rootFileName = urn[urn.rfind('/') + 1:]
        basePath = urn[:urn.rfind('/') + 1]
        localPath = basePath[basePath.find('/') + 1:]
        localPath = re.sub(r"[/]?output/", "", localPath)

        return PathInfo(rootFileName, basePath, localPath, urn)

    def download_stream_resource(self, resource: Resource) -> BytesIO:
        """
        Downloads a resource from a URL and returns it as a stream.

        Parameters:
        resource (Resource): The resource object containing the URL to download.

        Returns:
        BytesIO: A stream containing the downloaded resource.
        """
        url = resource.url
        access_token = self.token.access_token
        if not access_token:
            raise Exception("Have no access token to download resource.")
        headers = {
            "Authorization": f"Bearer {access_token}",
            "region": self.region
        }
        response = requests.get(url, headers=headers)
        return BytesIO(response.content)

    def download_resource(self, resource: Resource, local_path: str) -> str:
        """
        Downloads a resource from a URL and saves it to a local path.

        Parameters:
        resource (Resource): The resource object containing the URL to download.
        local_path (str): The local path where the resource will be saved.

        Returns:
        str: The local path where the resource has been saved.
        """
        url = resource.url
        access_token = self.token.access_token
        if not access_token:
            raise Exception("Have no access token to download resource.")
        headers = {
            "Authorization": f"Bearer {access_token}",
            "region": self.region
        }
        response = requests.get(url, headers=headers)
        # if dir not exist, create it
        if not os.path.exists(os.path.dirname(local_path)):
            os.makedirs(os.path.dirname(local_path))
        with open(local_path, "wb") as f:
            f.write(response.content)
        return local_path

    def get_metadata(self) -> pd.DataFrame:
        """
        Get metadata of the model
        :return: a dictionary of metadata
        """
        URL = f"{self.host}/modelderivative/v2/designdata/{self.urn}/metadata"
        access_token = self.token.access_token
        headers = {
            "Authorization": f"Bearer {access_token}",
            "region": self.region
        }
        response = requests.get(URL, headers=headers)
        if response.status_code == 404:
            raise Exception(response.reason)
        result = response.json()
        df = pd.DataFrame()
        for item in result["data"]["metadata"]:
            if isinstance(item, dict):
                properties_values = {}
                for i, v in item.items():
                    properties_values[i] = v
                df = pd.concat([df, pd.DataFrame(properties_values, index=[0])])
        return df

    def get_all_properties(self, model_guid) -> dict:
        """
        Get all properties of the model
        :param model_guid: the guid of the model
        :return: a dictionary of properties
        """
        URL = f"{self.host}/modelderivative/v2/designdata/{self.urn}/metadata/{model_guid}/properties"
        access_token = self.token.access_token
        headers = {
            "Authorization": f"Bearer {access_token}",
            "region": self.region,
            "x-ads-force": "true",
            'Accept-Encoding': 'gzip',
            "x-ads-derivative-format": "latest",

        }
        response = requests.get(URL, headers=headers)
        if response.status_code == 404:
            raise Exception(response.reason)
        if response.status_code == 202:
            while True:
                response = requests.get(URL, headers=headers)
                if response.status_code == 200:
                    break
        result = response.json()
        return result
