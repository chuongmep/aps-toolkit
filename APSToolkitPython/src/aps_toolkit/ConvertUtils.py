import base64
import re
import urllib.parse
from urllib.parse import urlparse, parse_qs
import pandas as pd

class ConvertUtils:
    def __init__(self):
        pass
    @staticmethod
    def urn_to_item_version(urn):
        """
        Convert URN to item_version
        e.g. dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0zNg to urn:adsk.wipprod:fs.file:vf.Od8txDbKSSelToVg1oc1VA?version=36
        :param urn:
        :return:
        """
        if not urn:
            return None
        if urn.find("_") == -1:
            raise Exception("Invalid URN")
        data = urn.split("_")
        item_id = data[0]
        versionid = data[1]
        item_id = base64.b64decode(item_id + "=").decode("utf-8")
        # Manually add padding to ensure the length is a multiple of 4
        padding = "=" * (4 - len(versionid) % 4) if len(versionid) % 4 != 0 else ""
        urn_padded = versionid + padding
        versionid = base64.b64decode(urn_padded).decode("utf-8")
        item_version = item_id + "?" + versionid
        return item_version

    @staticmethod
    def item_version_to_urn(item_version):
        """
        Convert item_version to URN
        e.g. urn:adsk.wipprod:fs.file:vf.Od8txDbKSSelToVg1oc1VA?version=36 to dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0zNg
        :param item_version:
        :return:
        """
        data = item_version.split("?")
        item_id = data[0]
        versionid = data[1]
        item_id = base64.b64encode(item_id.encode("utf-8")).decode("utf-8")
        versionid = base64.b64encode(versionid.encode("utf-8")).decode("utf-8")
        urn = item_id + "_" + versionid
        urn = urn.replace("=", "")
        return urn

    @staticmethod
    def parse_acc_url(url: str) -> pd.Series:
        """
        Parse url to get project_id, folder_urn, version_id, viewable_guid
        :param url: the url from bim360 or autodesk construction cloud (ACC)
        :return: :class:`dict` project_id, folder_urn, version_id, viewable_guid
        """
        if url is None:
            raise Exception("url is required")
        if not str.__contains__(url, "autodesk.com"):
            raise Exception("url is not valid")
        project_id_match = re.search(r'projects/([^\/?#]+)', url)
        project_id = 'b.' + project_id_match.group(1) if project_id_match else ''
        query_params = parse_qs(urlparse(url).query)
        folder_urn = query_params.get('folderUrn', [''])[0]
        version_id = query_params.get('entityId', [''])[0]
        version_encoder = urllib.parse.quote(version_id)
        item_id = None
        if version_id is not None or version_id != '':
            item_id = version_id.split("?")[0]
        viewable_guid = query_params.get('viewable_guid', [''])[0]

        data = {
            'project_id': project_id,
            'folder_urn': folder_urn,
            'item_id': item_id,
            'version_id': version_id,
            'version_encoder': version_encoder,
            'viewable_guid': viewable_guid,
        }
        series = pd.Series(data)
        return series