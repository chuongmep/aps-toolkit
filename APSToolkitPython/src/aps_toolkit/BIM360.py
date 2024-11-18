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
import os
import re
from urllib.parse import urlparse, parse_qs
from io import BytesIO
import urllib.parse

class BIM360:
    def __init__(self, token: Token = None):
        self.token = token
        if self.token is None:
            auth = Auth()
            self.token = auth.auth2leg()
        self.host = "https://developer.api.autodesk.com"

    @staticmethod
    def parse_url(url: str) -> pd.Series:
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
            item_id =version_id.split("?")[0]
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

    def publish_model(self, project_id: str, item_id: str):
        """
        Publish a model in Autodesk Construction Cloud (ACC)
        :param project_id: the unique identifier of a project
        :param item_id: the unique identifier of an item
        :return: Response from the API
        """
        headers = {
            'Authorization': f'Bearer {self.token.access_token}',
            'Content-Type': 'application/vnd.api+json'
        }
        url = f"{self.host}/data/v1/projects/{project_id}/commands"

        payload = {
            "jsonapi": {
                "version": "1.0"
            },
            "data": {
                "type": "commands",
                "attributes": {
                    "extension": {
                        "type": "commands:autodesk.bim360:C4RModelPublish",
                        "version": "1.0.0"
                    }
                },
                "relationships": {
                    "resources": {
                        "data": [
                            {
                                "type": "items",
                                "id": item_id
                            }
                        ]
                    }
                }
            }
        }
        response = requests.post(url, headers=headers, json=payload)
        return response.json() if response.status_code == 200 else response.text

    def publish_model_without_links(self, project_id: str, item_id: str):
        """
        Publish a model in Autodesk Construction Cloud (ACC)
        :param project_id: the unique identifier of a project
        :param item_id: the unique identifier of an item
        :return: Response from the API
        """
        headers = {
            'Authorization': f'Bearer {self.token.access_token}',
            'Content-Type': 'application/vnd.api+json'
        }
        url = f"{self.host}/data/v1/projects/{project_id}/commands"

        payload = {
            "jsonapi": {
                "version": "1.0"
            },
            "data": {
                "type": "commands",
                "attributes": {
                    "extension": {
                        "type": "commands:autodesk.bim360:C4RPublishWithoutLinks",
                        "version": "1.0.0"
                    }
                },
                "relationships": {
                    "resources": {
                        "data": [
                            {
                                "type": "items",
                                "id": item_id
                            }
                        ]
                    }
                }
            }
        }
        response = requests.post(url, headers=headers, json=payload)
        return response.json() if response.status_code == 200 else response.text

    def batch_publish_revit_model(self, project_id: str, folder_id: str,is_sub_folder: bool = False):
        """
        Patch publish Revit model in Autodesk Construction Cloud (ACC)
        :param project_id: the unique identifier of a project
        :param folder_id:  the unique identifier of a folder
        :param is_sub_folder:  the flag to check if the folder is sub folder
        :return:  :class:`pandas.DataFrame` the dataframe of all items that able to publish
        """
        df_items = self.batch_report_items(project_id, folder_id,[".rvt"], is_sub_folder)
        df_report = pd.DataFrame()
        for index,item in df_items.iterrows():
            item_id = item["relationships.item.data.id"]
            result = self.get_publish_model_job(project_id, item_id)
            if result.status_code != 200:
                raise Exception(result.reason)
            if "data" not in result.json():
                continue
            content = result.json()["data"]
            if content is not None:
                continue
            self.publish_model(project_id, item_id)
            df_report = pd.concat([df_report, item], ignore_index=True)
        return df_report

    def get_publish_model_job(self, project_id: str, item_id: str)-> requests.Response:
        """
        Get the publish job for a model in Autodesk Construction Cloud (ACC)
        :param project_id: the unique identifier of a project
        :param item_id: the unique identifier of an item
        :return: Response from the API
        """
        headers = {
            'Authorization': f'Bearer {self.token.access_token}',
            'Content-Type': 'application/vnd.api+json'
        }
        url = f"{self.host}/data/v1/projects/{project_id}/commands"

        payload = {
            "jsonapi": {
                "version": "1.0"
            },
            "data": {
                "type": "commands",
                "attributes": {
                    "extension": {
                        "type": "commands:autodesk.bim360:C4RModelGetPublishJob",
                        "version": "1.0.0"
                    }
                },
                "relationships": {
                    "resources": {
                        "data": [
                            {
                                "type": "items",
                                "id": item_id  # Using the item_id passed into the function
                            }
                        ]
                    }
                }
            }
        }

        response = requests.post(url, headers=headers, json=payload)
        return response

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
            raise Exception(response.reason)

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
            raise Exception(response.reason)
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
            raise Exception(response.reason)
        return response.json()

    def get_top_folder_project_files(self, hub_id: str, project_id: str):
        data = self.get_top_folders(hub_id, project_id)
        for item in data['data']:
            if item['attributes']['name'] == "Project Files":
                return item
        return None

    def create_folder(self, project_id, parent_folder_id, folder_name):
        """
        Creates a folder in a project. Folders are used to organize items in a project.
        :param project_id:  :class:`str` The unique identifier of a project.
        :param parent_folder_id:  :class:`str` The unique identifier of a parent folder.
        :param folder_name:  :class:`str` The name of the folder.
        :return:  :class:`dict` all information of new folder
        """
        url = f"{self.host}/data/v1/projects/{project_id}/folders"
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        data = {
            "jsonapi": {
                "version": "1.0"
            },
            "data": {
                "type": "folders",
                "attributes": {
                    "name": folder_name,
                    "extension": {
                        "type": "folders:autodesk.bim360:Folder",
                        "version": "1.0"
                    }
                },
                "relationships": {
                    "parent": {
                        "data": {
                            "type": "folders",
                            "id": parent_folder_id
                        }
                    }
                }
            }
        }
        response = requests.post(url, headers=headers, json=data)
        if response.status_code != 201:
            raise Exception(response.reason)
        return response.json()

    def rename_folder(self, project_id: str, folder_id: str, folder_name: str):
        """
        rename a folder in a project. Folders are used to organize items in a project.
        :param project_id:  :class:`str` The unique identifier of a project.
        :param folder_id:  :class:`str` The unique identifier of a folder.
        :return:  :class:`bytes` response content
        """
        url = f"{self.host}/data/v1/projects/{project_id}/folders/{folder_id}"
        headers = {'Authorization': 'Bearer ' + self.token.access_token, 'Content-Type': 'application/vnd.api+json'}
        data = {
            "jsonapi": {
                "version": "1.0"
            },
            "data": {
                "type": "folders",
                "id": folder_id,
                "attributes": {
                    "name": folder_name
                }
            }

        }
        response = requests.patch(url, headers=headers, json=data)
        if response.status_code != 200:
            raise Exception(response.reason)
        return response.content

    def get_folder_contents(self, project_id: str, folder_id: str) -> dict:
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
            raise Exception(response.reason)
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
            raise Exception(response.reason)
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
            raise Exception(response.reason)
        return response.json()

    def get_version_info(self, project_id: str, version_id: str):
        """
        Returns the version with the given version_id.
        https://aps.autodesk.com/en/docs/data/v2/reference/http/projects-project_id-versions-version_id-GET/
        :param project_id: :class:`str` The unique identifier of a project.
        :param version_id: :class:`str` The unique identifier of a version.
        :return: :class:`dict` all information of version
        """
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        # URL-encode the version_id
        version_id = urllib.parse.quote(version_id)
        url = f"{self.host}/data/v1/projects/{project_id}/versions/{version_id}"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        return response.json()

    def get_item_from_version(self, project_id: str, version_id: str):
        """
        Returns the item the given version is associated with.
        https://aps.autodesk.com/en/docs/data/v2/reference/http/projects-project_id-versions-version_id-item-GET/
        :param project_id: :class:`str` The unique identifier of a project.
        :param version_id: :class:`str` The unique identifier of a version.
        :return: :class:`dict` all information of version
        """
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        # encoder version_id
        version_id = urllib.parse.quote(version_id)
        url = f"{self.host}/data/v1/projects/{project_id}/versions/{version_id}/item"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        return response.json()

    def get_latest_derivative_urn(self, project_id: str, item_id: str):
        """
        Get the latest derivative urn of an item by project_id and item_id
        :param project_id: :class:`str` the unique identifier of a project
        :param item_id: :class:`str` the unique identifier of an item
        :return: :class:`str` the latest derivative urn of an item
        """
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}/versions"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        item_versions = response.json()
        return item_versions['data'][0]['relationships']['derivatives']['data']['id']


    def batch_report_hubs(self) -> pd.DataFrame:
        """
        Get batch all hubs information
        :return: :class:`pandas.DataFrame` all hubs information : id, name, type,...
        """
        df = pd.DataFrame(columns=['id', 'name', 'type'])
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/project/v1/hubs"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        hubs = response.json()
        df = pd.json_normalize(hubs['data'])
        return df

    def batch_report_projects(self, hub_id: str) -> pd.DataFrame:
        """
        Get batch all projects with general information by hub_id
        :param hub_id:  :class:`str` the unique identifier of a hub
        :return:  :class:`pandas.DataFrame` all projects with general information : id, name, type
        """
        df = pd.DataFrame(columns=['id', 'name', 'type'])
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/project/v1/hubs/{hub_id}/projects"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        projects = response.json()
        for project in projects['data']:
            project_id = project['id']
            project_name = project['attributes']['name']
            type = project['attributes']['extension']["data"]["projectType"]
            df = pd.concat([df, pd.DataFrame({'id': project_id, 'name': project_name, 'type': type}, index=[0])],
                           ignore_index=True)
        df.sort_values(by='name', inplace=True)
        return df

    def batch_report_top_folders(self, hub_id: str, project_id: str) -> pd.DataFrame:
        """
        Get batch all top folders with general information by hub_id and project_id
        :param hub_id:  :class:`str` the unique identifier of a hub
        :param project_id:  :class:`str` the unique identifier of a project
        :return:  :class:`pandas.DataFrame` all top folders with general information : id, name
        """
        df = pd.DataFrame(columns=['id'])
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/project/v1/hubs/{hub_id}/projects/{project_id}/topFolders"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        top_folders = response.json()
        for top_folder in top_folders['data']:
            top_folder_id = top_folder['id']
            atts = top_folder['attributes']
            properties_values = {}
            for key in atts:
                if isinstance(atts[key], dict):
                    continue
                properties_values[key] = atts[key]
            properties_values['id'] = top_folder_id
            df = pd.concat([df, pd.DataFrame(properties_values, index=[0])], ignore_index=True)
        # drop all columns have null value
        df.dropna(axis=1, how='all', inplace=True)
        return df

    def batch_report_folder_contents(self, project_id: str, folder_id: str, deep: int = 1) -> pd.DataFrame:
        """
        Get batch all folder contents information
        :param project_id:  :class:`str` the unique identifier of a project
        :param folder_id:  :class:`str` the unique identifier of a folder
        :param deep:  :class:`int` the deep of recursive folder contents, default is 1
        :return:
        """
        result = self.get_folder_contents(project_id, folder_id)
        df = pd.json_normalize(result['data'])
        if deep > 1:
            return self._recursive_folder_contents(project_id, folder_id, deep)
        return df

    def _recursive_folder_contents(self, project_id: str, folder_id: str, deep: int = 1):
        result = self.get_folder_contents(project_id, folder_id)
        df = pd.json_normalize(result['data'])
        if deep > 1:
            for item in result['data']:
                if item['type'] == "folders":
                    df = pd.concat([df, self._recursive_folder_contents(project_id, item['id'], deep - 1)],
                                   ignore_index=True)
        return df

    def batch_report_item_versions(self, project_id: str, item_id: str) -> pd.DataFrame:
        """
        Get batch all item versions with general information by project_id and item_id
        :param project_id:  :class:`str` the unique identifier of a project
        :param item_id:  :class:`str` the unique identifier of an item
        :return:  :class:`pandas.DataFrame` all item versions with general information containing :
        item_id,version,derivative_urn,last_modified_time
        """
        # create a dataframe to save data report with columns : item_id,version,derivative_urn,last_modified_time
        df = pd.DataFrame(columns=['item_id', 'version', 'derivative_urn', 'last_modified_time'])
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}/versions"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
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
            raise Exception(response.reason)
        item = response.json()
        return item['data']['attributes']['displayName']

    def batch_report_items(self, project_id: str, folder_id: str, extensions: list[str] = None,
                           is_sub_folder: bool = False) -> pd.DataFrame:
        """
        Get batch all items with general information by project_id and folder_id
        :param project_id:  :class:`str` the unique identifier of a project
        :param folder_id:  :class:`str` the unique identifier of a folder
        :param extensions:  :class:`list[str]` the extension of file. e.g: [.rvt,.dwg,.pdf]
        :param is_sub_folder:  :class:`bool` if True, get all items in sub folders by recursive
        :return: :class:`pandas.DataFrame` all items with general information containing :
        project_id, folder_id, item_name, item_id, last_version, derivative_urn, last_modified_time
        """
        if extensions is None:
            extensions = [""]
        df = pd.DataFrame()
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{project_id}/folders/{folder_id}/contents"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        folder_contents = response.json()
        if is_sub_folder:
            for folder_content in folder_contents['data']:
                if folder_content['type'] == "folders":
                    df = pd.concat(
                        [df, self.batch_report_items(project_id, folder_content['id'], extensions, is_sub_folder)],
                        ignore_index=True)
        # if included not include or null,pass
        if 'included' in folder_contents:
            for include_content in folder_contents['included']:
                item_name = include_content['attributes']['displayName']
                extension_format = item_name.split('.')[-1]
                # remove all dot in extensions
                extensions = [extension.replace(".", "") for extension in extensions]
                if extension_format not in extensions:
                    if extensions != [""]:
                        continue
                single_df = pd.json_normalize(include_content)
                single_df.insert(0, 'project_id', project_id)
                single_df.insert(1, 'folder_id', folder_id)
                df = pd.concat([df, single_df], ignore_index=True)
        # df.columns = ['project_id','folder_id','item_name', 'item_id', 'last_version', 'derivative_urn', 'last_modified_time']

        return df

    def _get_number_latest_item_version(self, project_id: str, item_id: str) -> int:
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}/versions"
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        item_versions = response.json()
        return len(item_versions['data'])

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
            raise Exception(response.reason)
        item_versions = response.json()
        for item_version in item_versions['data']:
            if str(item_version['attributes']['versionNumber']) == str(version):
                if 'derivatives' in item_version['relationships']:
                    return item_version['relationships']['derivatives']['data']['id']
        return None

    def upload_file_item(self, project_id: str, folder_id: str, file_path: str):
        """
        Upload a file to BIM 360 Docs or Autodesk Construction Cloud
        :param project_id:  :class:`str` the unique identifier of a project
        :param folder_id:  :class:`str` the unique identifier of a folder
        :param file_path:  :class:`str` the path of file need to upload
        :return: :class:`dict` all information of file version
        """
        object_name = os.path.basename(file_path)
        result = self._create_object_storage(project_id, folder_id, object_name)
        id = result['data']['id']
        sign = self._signeds_3_upload(id)
        upload_key = sign['uploadKey']
        url = sign['urls'][0]
        self._upload_file_to_signed_url(url, file_path)
        self._complete_upload(upload_key, id)
        try:
            file_version = self._create_first_version_file(project_id, folder_id, object_name, id)
            return file_version
        except Exception as e:
            error = "Another object with the same name already exists in this container"
            if error in str(e) or "Conflict" in str(e):
                print("File already exists")
                item_id = self._get_item_id(project_id, folder_id, object_name)
                file_version = self._create_new_file_version(project_id, item_id, object_name, id)
                return file_version
            else:
                raise e

    def upload_file_item_stream(self, project_id: str, folder_id: str, file_name: str, file_stream: BytesIO):
        """
        Upload a file to BIM 360 Docs or Autodesk Construction Cloud
        :param project_id:  :class:`str` the unique identifier of a project
        :param folder_id:  :class:`str` the unique identifier of a folder
        :param file_name:  :class:`str` the name of file need to upload
        :param file_stream:  :class:`BytesIO` the stream of file need to upload
        :return: :class:`dict` all information of file version
        """
        result = self._create_object_storage(project_id, folder_id, file_name)
        id = result['data']['id']
        sign = self._signeds_3_upload(id)
        upload_key = sign['uploadKey']
        url = sign['urls'][0]
        self._upload_file_to_signed_url_stream(url, file_stream)
        self._complete_upload(upload_key, id)
        try:
            file_version = self._create_first_version_file(project_id, folder_id, file_name, id)
            return file_version
        except Exception as e:
            error = "Another object with the same name already exists in this container"
            if error in str(e) or "Conflict" in str(e):
                print("File already exists")
                item_id = self._get_item_id(project_id, folder_id, file_name)
                file_version = self._create_new_file_version(project_id, item_id, file_name, id)
                return file_version
            else:
                raise e

    def copy_folder_contents(self, source_project_id, source_folder_id, target_project_id, target_folder_id) -> None:
        """
        Copy all contents of a folder to another folder
        :param source_project_id:  :class:`str` the unique identifier of a source project
        :param source_folder_id:  :class:`str` the unique identifier of a source folder
        :param target_project_id:  :class:`str` the unique identifier of a target project
        :param target_folder_id:  :class:`str` the unique identifier of a target folder
        :return: :class:`dict` all information of file version
        """
        folder_contents = self.get_folder_contents(source_project_id, source_folder_id)
        for item in folder_contents['data']:
            if item['type'] == "folders":
                folder_name = item['attributes']['name']
                new_folder = self.create_folder(target_project_id, target_folder_id, folder_name)
                self.copy_folder_contents(source_project_id, item['id'], target_project_id, new_folder['data']['id'])
            else:
                item_id = item['id']
                self.copy_file_item(item_id, source_project_id, target_project_id, target_folder_id)

    def copy_file_item(self, item_id, source_project_id, target_project_id, target_folder_id) -> dict:
        """
        Copy a file between hubs, projects, folders
        :param item_id:  :class:`str` the unique identifier of an item
        :param source_project_id:  :class:`str` the unique identifier of a source project
        :param target_project_id:  :class:`str` the unique identifier of a target project
        :param target_folder_id:  :class:`str` the unique identifier of a target folder
        :return: :class:`dict` all information of file version
        """
        # find the object_id of item
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        url = f"{self.host}/data/v1/projects/{source_project_id}/items/{item_id}"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        item = response.json()
        object_id = item['included'][0]['relationships']['storage']['data']['id']
        # bucket_key should wip.dm.prod
        bucket_key = object_id.split("/").pop(0).split(":").pop()
        object_key = object_id.split("/").pop()
        item_name = item['data']['attributes']['displayName']
        # Create storage in target folder and target project
        result = self._create_object_storage(target_project_id, target_folder_id, item_name)
        new_object_id = result['data']['id']
        new_object_key = new_object_id.split("/").pop()

        #  // Transfer data from source to target storage
        url = f"https://developer.api.autodesk.com/oss/v2/buckets/{bucket_key}/objects/{object_key}/copyto/{new_object_key}"
        response = requests.put(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        new_object_id = response.json()['objectId']
        try:
            # create first version
            file_version = self._create_first_version_file(target_project_id, target_folder_id, item_name,
                                                           new_object_id)
            return file_version
        except Exception as e:
            error = "Another object with the same name already exists in this container"
            if error in str(e):
                print("File already exists, create new version")
                object_name = item_name
                item_id = self._get_item_id(target_project_id, target_folder_id, object_name)
                file_version = self._create_new_file_version(target_project_id, item_id, object_name, new_object_id)
                return file_version
            else:
                raise e

    def _create_object_storage(self, project_id: str, folder_id: str, file_name: str):
        """
        Create object storage in BIM 360 Docs
        :param project_id: :class:`str` the unique identifier of a project
        :param folder_id: :class:`str` the unique identifier of a folder
        :param file_name: :class:`str` the name of object storage
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
                    "name": file_name
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
            raise Exception(response.reason)
        return response.json()

    def _signeds_3_upload(self, object_id):
        bucket_key = object_id.split("/").pop(0).split(":").pop()
        object_key = object_id.split("/").pop()
        url = f"{self.host}/oss/v2/buckets/{bucket_key}/objects/{object_key}/signeds3upload"
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        return response.json()

    def _upload_file_to_signed_url(self, signed_upload_url, file_path):
        with open(file_path, 'rb') as file:
            response = requests.put(signed_upload_url, data=file)
            return response

    def _upload_file_to_signed_url_stream(self, signed_upload_url, file_stream):
        response = requests.put(signed_upload_url, data=file_stream)
        return response

    def _complete_upload(self, upload_key, object_id):
        bucket_key = object_id.split("/").pop(0).split(":").pop()
        object_key = object_id.split("/").pop()
        url = f"{self.host}/oss/v2/buckets/{bucket_key}/objects/{object_key}/signeds3upload"
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        data = {
            "uploadKey": upload_key
        }
        response = requests.post(url, headers=headers, json=data)
        if response.status_code != 200:
            raise Exception(response.reason)
        return response.json()

    def _create_first_version_file(self, project_id: str, folder_id: str, object_name: str, object_id: str) -> dict:
        url = f"{self.host}/data/v1/projects/{project_id}/items"
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        data = {
            "jsonapi": {
                "version": "1.0"
            },
            "data": {
                "type": "items",
                "attributes": {
                    "displayName": object_name,
                    "extension": {
                        "type": "items:autodesk.bim360:File",
                        "version": "1.0"
                    }
                },
                "relationships": {
                    "tip": {
                        "data": {
                            "type": "versions",
                            "id": "1"
                        }
                    },
                    "parent": {
                        "data": {
                            "type": "folders",
                            "id": folder_id
                        }
                    }
                }
            },
            "included": [
                {
                    "type": "versions",
                    "id": "1",
                    "attributes": {
                        "name": object_name,
                        "extension": {
                            "type": "versions:autodesk.bim360:File",
                            "version": "1.0"
                        }
                    },
                    "relationships": {
                        "storage": {
                            "data": {
                                "type": "objects",
                                "id": object_id
                            }
                        }
                    }
                }
            ]
        }
        response = requests.post(url, headers=headers, json=data)
        if response.status_code != 201:
            raise Exception(response.reason)
        return response.json()

    def _get_item_id(self, project_id: str, folder_id: str, object_name: str):
        url = f"{self.host}/data/v1/projects/{project_id}/folders/{folder_id}/contents"
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        folder_contents = response.json()
        for folder_content in folder_contents['data']:
            if folder_content['type'] == "items":
                item_name = folder_content['attributes']['displayName']
                if item_name == object_name:
                    return folder_content['id']
        return None

    def _create_new_file_version(self, project_id: str, item_id: str, object_name: str, object_id: str):
        url = f"{self.host}/data/v1/projects/{project_id}/versions"
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        data = {
            "jsonapi": {
                "version": "1.0"
            },
            "data": {
                "type": "versions",
                "attributes": {
                    "name": object_name,
                    "extension": {
                        "type": "versions:autodesk.bim360:File",
                        "version": "1.0"
                    }
                },
                "relationships": {
                    "item": {
                        "data": {
                            "type": "items",
                            "id": item_id
                        }
                    },
                    "storage": {
                        "data": {
                            "type": "objects",
                            "id": object_id
                        }
                    }
                }
            }
        }
        response = requests.post(url, headers=headers, json=data)
        if response.status_code != 201:
            raise Exception(response.reason)
        return response.json()


    def rename_file_item(self, projectId:str, item_id, newName:str):
        """
        Rename a file in BIM 360 Docs or Autodesk Construction Cloud
        :param projectId:  :class:`str` the unique identifier of a project
        :param item_id:  :class:`str` the unique identifier of an item
        :param newName:  :class:`str` the new name of item e.g: new_name.rvt
        :return: :class:`bytes` response content
        # API : https://aps.autodesk.com/en/docs/data/v2/reference/http/projects-project_id-versions-POST/
        """
        item_info = self.get_item_info(projectId, item_id)
        version_id = item_info['data']['relationships']['tip']['data']['id']
        url = f"{self.host}/data/v1/projects/{projectId}/versions?copyFrom={version_id}"
        headers = {
            'Authorization': 'Bearer ' + self.token.access_token,
            'Content-Type': 'application/vnd.api+json'
        }
        data = {
            "jsonapi": {
                "version": "1.0"
            },
            "data": {
                "type": "versions",
                "attributes": {
                    "name": newName
                }
            }
        }
        response = requests.post(url, headers=headers, json=data)
        if response.status_code != 201:
            raise Exception(response.reason)
        return response.json()

    def delete_file_item(self, project_id: str, folder_id: str, file_name: str):
        """
        Delete a file in BIM 360 Docs or Autodesk Construction Cloud
        :param project_id:  :class:`str` the unique identifier of a project
        :param folder_id:  :class:`str` the unique identifier of a folder
        :param file_name:  :class:`str` the name of file need to delete
        :return: :class: `bytes` response content
        """
        item_id = self._get_item_id(project_id, folder_id, file_name)
        url = f"{self.host}/data/v1/projects/{project_id}/versions"
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        data = {
            "jsonapi": {
                "version": "1.0"
            },
            "data": {
                "type": "versions",
                "attributes": {
                    "extension": {
                        "type": "versions:autodesk.core:Deleted",
                        "version": "1.0"
                    }
                },
                "relationships": {
                    "item": {
                        "data": {
                            "type": "items",
                            "id": item_id
                        }
                    }
                }
            }
        }
        response = requests.post(url, headers=headers, json=data)
        if response.status_code != 201:
            raise Exception(response.reason)
        return response.content

    def download_file_item(self, file_path: str, project_id: str, folder_id: str, file_name: str, version: int = -1):
        """
        Download a file in BIM 360 Docs or Autodesk Construction Cloud
        :param file_path:  :class:`str` the path of file need to download
        :param project_id:  :class:`str` the unique identifier of a project
        :param folder_id:  :class:`str` the unique identifier of a folder
        :param file_name:  :class:`str` the name of file at the folder need to download
        :param version:  :class:`int` the version of file need to download
        :return:  :class:`str` the path of file after download
        """
        item_id = self._get_item_id(project_id, folder_id, file_name)
        if item_id is None:
            raise Exception("File not found")
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}/versions"
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        item_versions = response.json()
        if version == -1:
            url = item_versions['data'][0]['relationships']['storage']['data']['id']
        else:
            version = version - 1
            if version < 0 or version >= len(item_versions['data']):
                raise Exception("Version not found")
            url = item_versions['data'][version]['relationships']['storage']['data']['id']
        bucket_key = url.split("/").pop(0).split(":").pop()
        object_key = url.split("/").pop()
        s3_url = f"{self.host}/oss/v2/buckets/{bucket_key}/objects/{object_key}/signeds3download"
        response = requests.get(s3_url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        download_url = response.json()['url']
        response = requests.get(download_url)
        with open(file_path, 'wb') as file:
            file.write(response.content)
        return file_path

    def download_file_stream_item(self, project_id: str, folder_id: str, file_name: str, version: int = -1):
        """
        Download a file stream in BIM 360 Docs or Autodesk Construction Cloud
        :param project_id:  :class:`str` the unique identifier of a project
        :param folder_id:  :class:`str` the unique identifier of a folder
        :param file_name:  :class:`str` the name of file at the folder need to download
        :param version:  :class:`int` the version of file need to download
        :return:  :class:`str` the path of file after download
        """
        item_id = self._get_item_id(project_id, folder_id, file_name)
        if item_id is None:
            raise Exception("File not found")
        url = f"{self.host}/data/v1/projects/{project_id}/items/{item_id}/versions"
        headers = {'Authorization': 'Bearer ' + self.token.access_token}
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        item_versions = response.json()
        if version == -1:
            url = item_versions['data'][0]['relationships']['storage']['data']['id']
        else:
            version = version - 1
            if version < 0 or version >= len(item_versions['data']):
                raise Exception("Version not found")
            url = item_versions['data'][version]['relationships']['storage']['data']['id']
        bucket_key = url.split("/").pop(0).split(":").pop()
        object_key = url.split("/").pop()
        s3_url = f"{self.host}/oss/v2/buckets/{bucket_key}/objects/{object_key}/signeds3download"
        response = requests.get(s3_url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        download_url = response.json()['url']
        response = requests.get(download_url)
        bytes_io = BytesIO(response.content)
        return bytes_io

    def restore_file_item(self, project_id, item_id, version=1):
        """
        Restore a file in BIM 360 Docs or Autodesk Construction Cloud
        https://aps.autodesk.com/en/docs/data/v2/tutorials/delete-and-restore-file/
        :param project_id:  :class:`str` the unique identifier of a project
        :param item_id:  :class:`str` the unique identifier of an item
        :param version:  :class:`int` the version of file need to restore
        :return: :class:`dict` response content
        """
        url = f"https://developer.api.autodesk.com/data/v1/projects/{project_id}/versions?copyFrom={item_id}%3Fversion={version}"
        headers = {
            "Authorization": f"Bearer {self.token.access_token}",
            "content-type": "application/vnd.api+json"
        }
        data = {
            "data": {
                "type": "versions"
            }
        }
        response = requests.post(url, headers=headers, json=data)

        if response.status_code == 201:
            print("File restoration successful.")
        else:
            print(f"Failed to restore file. Status code: {response.status_code}")
            print(response.text)
        return response.content
