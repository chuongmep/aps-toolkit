import requests
import pandas as pd
from .Auth import Auth

class BIM360:
    def __init__(self, token=None):
        self.token = token
        if self.token is None:
            auth = Auth()
            self.token = auth.auth2leg()
        self.host = "https://developer.api.autodesk.com"

    def get_hubs(self) :
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/project/v1/hubs"
        response = requests.get(url, headers=headers)
        return response.json()

    def get_projects(self, hub_id):
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/project/v1/hubs/{hub_id}/projects"
        response = requests.get(url, headers=headers)
        return response.json()

    def get_top_folders(self, hub_id, project_id):
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/project/v1/hubs/{hub_id}/projects/{project_id}/topFolders"
        response = requests.get(url, headers=headers)
        return response.json()

    def get_folder_contents(self, project_id, folder_id):
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/folders/{folder_id}/contents"
        response = requests.get(url, headers=headers)
        return response.json()

    def get_item_versions(self, project_id, item_id):
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}/versions"
        response = requests.get(url, headers=headers)
        return response.json()

    def get_item_info(self, project_id, item_id):
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}"
        response = requests.get(url, headers=headers)
        return response.json()
    def batch_report_projects(self,hub_id) -> pd.DataFrame:
        df = pd.DataFrame(columns=['project_id', 'project_name',"project_type", 'top_folder_id'])
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/project/v1/hubs/{hub_id}/projects"
        response = requests.get(url, headers=headers)
        projects = response.json()
        for project in projects['data']:
            project_id = project['id']
            project_name = project['attributes']['name']
            project_type = project['attributes']['extension']["data"]["projectType"]
            top_folder = self.get_top_folders(hub_id, project_id)
            top_folder_id = top_folder["data"][0]["id"]
            df = pd.concat([df, pd.DataFrame({'project_id': project_id, 'project_name': project_name,'project_type':project_type, 'top_folder_id': top_folder_id}, index=[0])], ignore_index=True)
        return df

    def batch_report_item_versions(self, project_id, item_id) -> pd.DataFrame:
        # create a dataframe to save data report with columns : item_id,version,derivative_urn,last_modified_time
        df = pd.DataFrame(columns=['item_id', 'version', 'derivative_urn', 'last_modified_time'])
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}/versions"
        response = requests.get(url, headers=headers)
        item_versions = response.json()
        for item_version in item_versions['data']:
            version = item_version['attributes']['versionNumber']
            # check if item_version have derivatives
            if 'derivatives' not in item_version['relationships']:
                continue
            derivative_urn = item_version['relationships']['derivatives']['data']['id']
            last_modified_time = item_version['attributes']['lastModifiedTime']
            df = pd.concat([df, pd.DataFrame({'item_id': item_id, 'version': version, 'derivative_urn': derivative_urn,
                                              'last_modified_time': last_modified_time}, index=[0])], ignore_index=True)
        return df

    def get_item_display_name(self, project_id, item_id):
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}"
        response = requests.get(url, headers=headers)
        item = response.json()
        return item['data']['attributes']['displayName']

    def batch_report_items(self, project_id, folder_id, extension=".rvt", is_sub_folder=False) -> pd.DataFrame:
        df = pd.DataFrame(columns=['project_id', 'folder_id', 'item_name', 'item_id', 'last_version', 'derivative_urn',
                                   'last_modified_time'])
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/folders/{folder_id}/contents"
        response = requests.get(url, headers=headers)
        folder_contents = response.json()
        for folder_content in folder_contents['data']:
            if folder_content['type'] == "items":
                item_id = folder_content['id']
                item_name = folder_content['attributes']['displayName']
                item_versions = self._get_latest_version(project_id, item_id)
                # getfrom dict with keys : version
                last_version = item_versions["version"]
                derivative_urn = item_versions["derivative_urn"]
                last_modified_time = folder_content['attributes']['lastModifiedTime']
                df = pd.concat([df, pd.DataFrame(
                    {'project_id': project_id, 'folder_id': folder_id, 'item_name': item_name, 'item_id': item_id,
                     'last_version': last_version, 'derivative_urn': derivative_urn,
                     'last_modified_time': last_modified_time}, index=[0])], ignore_index=True)
            elif folder_content['type'] == "folders" and is_sub_folder:
                if is_sub_folder:
                    df = pd.concat(
                        [df, self.batch_report_items(project_id, folder_content['id'], extension, is_sub_folder)],
                        ignore_index=True)
        return df

    def _get_number_latest_item_version(self, project_id, item_id) -> int:
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}/versions"
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        response = requests.get(url, headers=headers)
        item_versions = response.json()
        return len(item_versions['data'])

    def _get_latest_version(self, project_id, item_id) -> dict:
        # dict with keys : version,derivative_urn,last_modified_time
        latest_version = {}
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}/versions"
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        response = requests.get(url, headers=headers)
        item_versions = response.json()
        # add to latest_version
        latest_version['version'] = item_versions['data'][0]['attributes']['versionNumber']
        latest_version['last_modified_time'] = item_versions['data'][0]['attributes']['lastModifiedTime']
        latest_version['derivative_urn'] = item_versions['data'][0]['relationships']['derivatives']['data']['id']
        return latest_version
    def get_urn_item_version(self, project_id, item_id, version):
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}/versions"
        response = requests.get(url, headers=headers)
        item_versions = response.json()
        for item_version in item_versions['data']:
            if item_version['attributes']['versionNumber'] == version:
                return item_version['relationships']['derivatives']['data']['id']
        return None
