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
import pandas as pd
from .Auth import Auth
from .Token import Token


class BIM360:
    def __init__(self, token: Token = None):
        self.token = token
        if self.token is None:
            auth = Auth()
            self.token = auth.auth2leg()
        self.host = "https://developer.api.autodesk.com"

    def get_hubs(self) -> dict:
        """
        Returns a collection of accessible hubs for this member.
        Hubs represent BIM 360 Team hubs, Fusion Team hubs (formerly known as A360 Team hubs), A360 Personal hubs, or BIM 360 Docs accounts. Team hubs include BIM 360 Team hubs and Fusion Team hubs (formerly known as A360 Team hubs). Personal hubs include A360 Personal hubs. Only active hubs are listed.
        Note that for BIM 360 Docs, a hub ID corresponds to an account ID in the BIM 360 API. To convert an account ID into a hub ID you need to add a "b." prefix. For example, an account ID of c8b0c73d-3ae9 translates to a hub ID of b.c8b0c73d-3ae9.
        https://aps.autodesk.com/en/docs/data/v2/reference/http/hubs-GET
        :return: :class:`dict` hubs collection
        """
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/project/v1/hubs"
        response = requests.get(url, headers=headers)
        if response.status_code == 200:
            return response.json()
        else:
            raise Exception(response.content)

    def get_projects(self, hub_id: str) -> dict:
        """
        Returns a collection of projects for a given hub_id. A project represents a BIM 360 Team project, a Fusion Team project, a BIM 360 Docs project, or an A360 Personal project. Multiple projects can be created within a single hub. Only active projects are listed.
        Note that for BIM 360 Docs, a hub ID corresponds to an account ID in the BIM 360 API. To convert an account ID into a hub ID you need to add a "b." prefix. For example, an account ID of c8b0c73d-3ae9 translates to a hub ID of b.c8b0c73d-3ae9.
        Similarly, for BIM 360 Docs, the project ID in the Data Management API corresponds to the project ID in the BIM 360 API. To convert a project ID in the BIM 360 API into a project ID in the Data Management API you need to add a "b." prefix. For example, a project ID of c8b0c73d-3ae9 translates to a project ID of b.c8b0c73d-3ae9.
        https://aps.autodesk.com/en/docs/data/v2/reference/http/hubs-hub_id-projects-GET/
        :param hub_id: :class:`str` hub id
        :return: :class:`dict` all information of projects
        """
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/project/v1/hubs/{hub_id}/projects"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        return response.json()

    def get_top_folders(self, hub_id: str, project_id: str):
        """
        Returns the details of the highest level folders the user has access to for a given project. The user must have at least read access to the folders.
        If the user is a Project Admin, it returns all top level folders in the project. Otherwise, it returns all the highest level folders in the folder hierarchy the user has access to.
        Note that when users have access to a folder, access is automatically granted to its subfolders.
        https://aps.autodesk.com/en/docs/data/v2/reference/http/hubs-hub_id-projects-project_id-topFolders-GET
        :param hub_id: :class:`str` hub id
        :param project_id: :class:`str` project id
        :return: :class:`dict` all information of top folders
        """
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/project/v1/hubs/{hub_id}/projects/{project_id}/topFolders"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        return response.json()

    def get_top_folder_project_files(self, hub_id: str, project_id: str):
        data = self.get_top_folders(hub_id, project_id)
        for item in data['data']:
            if item['attributes']['name'] == "Project Files":
                return item
        return None

    def get_folder_contents(self, project_id: str, folder_id: str):
        """
        Returns a collection of items and folders within a folder. Items represent word documents, fusion design files, drawings, spreadsheets, etc.
        https://aps.autodesk.com/en/docs/data/v2/reference/http/projects-project_id-folders-folder_id-contents-GET/
        :param project_id: :class:`str` The unique identifier of a project.
        :param folder_id: :class:`str` The unique identifier of a folder.
        :return:
        """
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/folders/{folder_id}/contents"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        return response.json()

    def get_item_versions(self, project_id: str, item_id: str):
        """
        Returns versions for the given item. Multiple versions of a resource item can be uploaded in a project.
        https://aps.autodesk.com/en/docs/data/v2/reference/http/projects-project_id-items-item_id-versions-GET/
        :param project_id: :class:`str` The unique identifier of a project.
        :param item_id: :class:`str` The unique identifier of an item.
        :return: :class:`dict` all information of item versions
        """
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}/versions"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        return response.json()

    def get_item_info(self, project_id: str, item_id: str):
        """
        Retrieves metadata for a specified item. Items represent word documents, fusion design files, drawings, spreadsheets, etc.
        https://aps.autodesk.com/en/docs/data/v2/reference/http/projects-project_id-items-item_id-GET/
        :param project_id: :class:`str` The unique identifier of a project.
        :param item_id: :class:`str` The unique identifier of an item.
        :return: :class:`dict` all information of item
        """
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        return response.json()

    def batch_report_projects(self, hub_id: str) -> pd.DataFrame:
        """
        Get batch all projects with general information by hub_id
        :param hub_id:  :class:`str` the unique identifier of a hub
        :return:  :class:`pandas.DataFrame` all projects with general information
        """
        df = pd.DataFrame(columns=['project_id', 'project_name', "project_type", 'top_folder_id'])
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/project/v1/hubs/{hub_id}/projects"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        projects = response.json()
        for project in projects['data']:
            project_id = project['id']
            project_name = project['attributes']['name']
            project_type = project['attributes']['extension']["data"]["projectType"]
            top_folder = self.get_top_folders(hub_id, project_id)
            top_folder_id = top_folder["data"][0]["id"]
            df = pd.concat([df, pd.DataFrame(
                {'project_id': project_id, 'project_name': project_name, 'project_type': project_type,
                 'top_folder_id': top_folder_id}, index=[0])], ignore_index=True)
        return df

    def batch_report_item_versions(self, project_id: str, item_id: str) -> pd.DataFrame:
        """
        Get batch all item versions with general information by project_id and item_id
        :param project_id:  :class:`str` the unique identifier of a project
        :param item_id:  :class:`str` the unique identifier of an item
        :return:  :class:`pandas.DataFrame` all item versions with general information
        """
        # create a dataframe to save data report with columns : item_id,version,derivative_urn,last_modified_time
        df = pd.DataFrame(columns=['item_id', 'version', 'derivative_urn', 'last_modified_time'])
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}/versions"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
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

    def get_item_display_name(self, project_id: str, item_id: str):
        """
        Retrieves the display name of a specified item. Items represent word documents, fusion design files, drawings, spreadsheets, etc.
        :param project_id:  :class:`str` the unique identifier of a project
        :param item_id:  :class:`str` the unique identifier of an item
        :return:  :class:`str` the display name of a specified item
        """
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        item = response.json()
        return item['data']['attributes']['displayName']

    def batch_report_items(self, project_id: str, folder_id: str, extension: str = ".rvt",
                           is_sub_folder: bool = False) -> pd.DataFrame:
        """
        Get batch all items with general information by project_id and folder_id
        :param project_id:  :class:`str` the unique identifier of a project
        :param folder_id:  :class:`str` the unique identifier of a folder
        :param extension:  :class:`str` the extension of file. e.g: .rvt, .dwg, .pdf
        :param is_sub_folder:  :class:`bool` if True, get all items in sub folders by recursive
        :return:
        """
        df = pd.DataFrame(columns=['project_id', 'folder_id', 'item_name', 'item_id', 'last_version', 'derivative_urn',
                                   'last_modified_time'])
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/folders/{folder_id}/contents"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        folder_contents = response.json()
        for folder_content in folder_contents['data']:
            if folder_content['type'] == "items":
                item_id = folder_content['id']
                item_name = folder_content['attributes']['displayName']
                if not item_name.endswith(extension):
                    continue
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

    def _get_number_latest_item_version(self, project_id: str, item_id: str) -> int:
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}/versions"
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        item_versions = response.json()
        return len(item_versions['data'])

    def _get_latest_version(self, project_id: str, item_id: str) -> dict:
        # dict with keys : version,derivative_urn,last_modified_time
        latest_version = {}
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}/versions"
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        item_versions = response.json()
        # add to latest_version
        latest_version['version'] = item_versions['data'][0]['attributes']['versionNumber']
        latest_version['last_modified_time'] = item_versions['data'][0]['attributes']['lastModifiedTime']
        latest_version['derivative_urn'] = item_versions['data'][0]['relationships']['derivatives']['data']['id']
        return latest_version

    def get_urn_item_version(self, project_id: str, item_id: str, version: str):
        """
        Get derivative urn of item version by project_id, item_id and version
        :param project_id: :class:`str` the unique identifier of a project
        :param item_id: :class:`str` the unique identifier of an item
        :param version: :class:`str` the version of an item
        :return: :class:`str` derivative urn of item version
        """
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}/versions"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        item_versions = response.json()
        for item_version in item_versions['data']:
            if item_version['attributes']['versionNumber'] == version:
                return item_version['relationships']['derivatives']['data']['id']
        return None

    def create_object_storage(self, project_id: str, folder_id: str, object_name: str):
        """
        Create object storage in BIM 360 Docs
        :param project_id: :class:`str` the unique identifier of a project
        :param folder_id: :class:`str` the unique identifier of a folder
        :param object_name: :class:`str` the name of object storage
        :return: :class:`dict` all information of object storage
        """
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/storage"
        data = {
            "jsonapi": {
                "version": "1.0"
            },
            "data": {
                "type": "objects",
                "attributes": {
                    "name": object_name
                },
                "relationships": {
                    "target": {
                        "data": {
                            "type": "folders",
                            "id": folder_id
                        }
                    }
                }
            }
        }
        response = requests.post(url, headers=headers, json=data)
        if response.status_code != 201:
            raise Exception(response.content)
        return response.json()

