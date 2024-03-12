import requests
import gzip
from io import BytesIO
import zipfile
from json import loads as json_loads
from typing import List
class Derivative:
    def __init__(self,urn,token,region="US"):
        self.urn = urn
        self.token = token
        self.region = region
        self.host = "https://developer.api.autodesk.com"

    def ReadSvf(self):
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
        svf_urns = []
        for child in children:
            if child["type"] == "geometry":
                for c in child["children"]:
                    # check if contains 'mime' and 'application/autodesk-svf
                    if "mime" in c and c["mime"] == "application/autodesk-svf":
                        urn_json = c["urn"]
                        svf_urns.append(urn_json)
                        json_content = self._unzip_svf(urn_json)
                        files = self._get_assets(json_content)
                        # TODO : Get Base Path and connect to return exact path url

        return svf_urns
    def _unzip_svf(self,urn):
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
        files = []
        # Iterate over each "asset" in the manifest
        for asset in manifest.get("assets", []):
            uri = asset.get("URI", "")
            print(uri)  # You can remove this line; it's equivalent to Debug.WriteLine in C#
            if "embed:/" in uri:
                continue
            files.append(uri)

        return files


