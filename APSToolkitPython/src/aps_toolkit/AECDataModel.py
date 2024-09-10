import pandas as pd

from .Token import Token
import requests


class AECDataModel:
    def __init__(self, token: Token):
        self.url = "https://developer.api.autodesk.com/aec/graphql"
        self.token = token

    def execute_query(self, query):
        headers = {
            'Authorization': f'Bearer {self.token.access_token}',  # Replace with your actual token
            'Content-Type': 'application/json'
        }
        response = requests.post(self.url, headers=headers, json=query)
        if response.status_code != 200:
            raise Exception(f"Error: {response.content}")
        return response.json()

    def execute_query_variables(self, query, variables):
        headers = {
            'Authorization': f'Bearer {self.token.access_token}',  # Replace with your actual token
            'Content-Type': 'application/json'
        }
        response = requests.post(self.url, headers=headers, json={'query': query, 'variables': variables})
        if response.status_code != 200:
            raise Exception(f"Error: {response.content}")
        return response.json()

    def get_hubs(self) -> pd.DataFrame:
        data = {
            "query": """
                query GetHubs {
                    hubs {
                        results {
                            id
                            name
                            alternativeIdentifiers{
                            dataManagementAPIHubId
                            }
                        }
                    }
                }
            """
        }
        result = self.execute_query(data)
        hubs = result['data']['hubs']['results']
        return pd.json_normalize(hubs)

    def get_projects(self, hub_id: str) -> pd.DataFrame:
        data = {
            "query": """
                query GetProjects($hubId: ID!) {
                    projects(hubId: $hubId) {
                        results {
                            id
                            name
                            hub {
                                id
                                name
                            }
                            alternativeIdentifiers{
                             dataManagementAPIProjectId
                            }
                        }
                    }
                }
            """,
            "variables": {
                "hubId": hub_id
            }
        }
        result = self.execute_query(data)
        projects = result['data']['projects']['results']
        return pd.json_normalize(projects)

    def get_folders(self, project_id: str) -> pd.DataFrame:
        data = {
            "query": """
                query GetFolders($projectId: ID!) {
                  foldersByProject(projectId: $projectId) {
                    results {
                      id
                      name
                      objectCount
                    }
                  }
                }
            """,
            "variables": {
                "projectId": project_id
            }
        }
        result = self.execute_query_variables(data['query'], data['variables'])
        folders = result['data']['foldersByProject']['results']
        return pd.json_normalize(folders)

    def get_element_group_by_project(self, projectId: str) -> pd.DataFrame:
        """
        Get element groups by project, return source file urn and version urn in alternativeIdentifiers
        :param projectId:
        :return:
        """
        data = {
            "query": """
                query GetElementGroupsByProject($projectId: ID!) {
                elementGroupsByProject(projectId: $projectId) {
                  pagination {
                    cursor
                  }
                  results{
                    name
                    id
                    version{
                    versionNumber
                    createdOn
                    createdBy{
                        id
                        userName
                        firstName
                        lastName
                        email
                        lastModifiedOn
                        createdOn
                    }
                    }
                    createdOn
                    lastModifiedBy{
                        id
                        userName
                        firstName
                        lastName
                        email
                        lastModifiedOn
                    }
                    alternativeIdentifiers{
                      fileUrn
                      fileVersionUrn
                    }
                    parentFolder{
                        id
                        name
                        objectCount
                    }
                  }
                }
            }
            """,
            "variables": {
                "projectId": projectId
            }
        }
        result = self.execute_query_variables(data['query'], data['variables'])
        item_versions = result['data']['elementGroupsByProject']['results']
        return pd.json_normalize(item_versions)
