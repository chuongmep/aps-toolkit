import requests
import gzip
from io import BytesIO
import zipfile
from json import loads as json_loads
from typing import List
from urllib.parse import unquote, quote, urljoin
import re
from os.path import join, normpath
import os


class Derivative:
    def __init__(self, urn, token, region="US"):
        self.urn = urn
        self.token = token
        self.region = region
        self.host = "https://developer.api.autodesk.com"

    def read_svf_manifest_items(self):
        """
        Reads SVF manifest items associated with the URN.

        Returns:
        List[ManifestItem]: A list of SVF manifest items.
        """
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
        manifest_items = []
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
                        manifest_items.append(ManifestItem(guid, mime, path_info))
        return manifest_items

    def read_svf_resource_item(self, manifest_item):
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

    def read_svf_resource(self):
        """
        Reads SVF resources from the SVF manifest items.

        Returns:
        List[Resource]: A list of resources extracted from the SVF manifest.
        """
        manifest_items = self.read_svf_manifest_items()
        resources = []
        for manifest_item in manifest_items:
            source_items = self.read_svf_resource_item(manifest_item)
            resources.extend(source_items)
        return resources

    def _unzip_svf(self, urn):
        """
        Retrieves and unzips the manifest associated with the given URN.

        Parameters:
        urn (str): The URN of the manifest to retrieve and unzip.

        Returns:
        dict: The contents of the unzipped manifest in JSON format.
        """
        URL = f"{self.host}/modelderivative/v2/designdata/{self.urn}/manifest/{urn}"
        access_token = self.token.access_token
        headers = {
            "Authorization": f"Bearer {access_token}",
            "region": self.region
        }
        response = requests.get(URL, headers=headers)
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

    def _decompose_urn(self, encodedUrn):
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

    def download_resource(self, resource, local_path) -> str:
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


class ManifestItem:
    def __init__(self, guid, mime, path_info):
        self.guid = guid
        self.mime = mime
        self.path_info = path_info


class PathInfo:
    def __init__(self, root_file_name=None, base_path=None, local_path=None, urn=None):
        self.root_file_name = root_file_name
        self.local_path = local_path
        self.base_path = base_path
        self.urn = urn
        self.files = []


class Resource:
    def __init__(self, file_name, remote_path, local_path):
        self.host = "https://developer.api.autodesk.com"
        self.file_name = file_name
        self.remote_path = self._resolve_path_slashes(remote_path)
        self.url = self._resolve_url(remote_path)
        self.local_path = self._resolve_path_slashes(local_path)

    def _resolve_path_slashes(self, path):
        url_with_forward_slashes = path.replace('\\', '/')
        return url_with_forward_slashes

    def _resolve_url(self, remote_path):
        url_with_forward_slashes = remote_path.replace('\\', '/')
        return urljoin(self.host, quote(url_with_forward_slashes, safe=':/'))
