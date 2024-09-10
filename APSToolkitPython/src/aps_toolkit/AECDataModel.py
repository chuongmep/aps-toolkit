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

    def get_element_by_category(self, elementGroupId: str, category: str) -> pd.DataFrame:
        data = {
            "query": """
               query GetElementsFromCategory($elementGroupId: ID!, $propertyFilter: String!) {
                    elementsByElementGroup(elementGroupId: $elementGroupId, filter: {query:$propertyFilter}) {
                      pagination {
                        cursor
                      }
                      results {
                        id
                        name
                        properties(includeReferencesProperties: "Type") {
                          results {
                            name
                            value
                            definition {
                              units{
                                name
                              }
                            }
                          }
                        }
                    }
                      }
                }
            """,
            "variables": {
                "elementGroupId": elementGroupId,
                "propertyFilter": f"'property.name.category'=={category} and 'property.name.Element Context'==Instance"
            }
        }
        result = self.execute_query_variables(data['query'], data['variables'])
        elements = result['data']['elementsByElementGroup']['results']
        df = pd.json_normalize(elements)
        data_df = pd.DataFrame()
        for i, row in df.iterrows():
            props_dict = {}
            single_df = pd.json_normalize(row['properties.results'])
            for i in range(len(single_df)):
                props_dict[single_df['name'][i]] = single_df['value'][i]
            single_df = pd.DataFrame(props_dict, index=[0], dtype="object")
            data_df = pd.concat([data_df, single_df], axis=0)
        return data_df

    # def get_element_projects_by_parameters(self, projectId: str, parameters: list[str]) -> pd.DataFrame:
    #     parameters_str = '","'.join(parameters)
    #     query = f"""
    #         query GetElementsInProject($projectId: ID!, $propertyFilter: String!) {{
    #             elementsByProject(projectId: $projectId, filter: {{query: $propertyFilter}}) {{
    #                 pagination {{
    #                     cursor
    #                 }}
    #                 results {{
    #                     id
    #                     name
    #                     properties(
    #                         includeReferencesProperties: "Type"
    #                         filter: {{names: ["{parameters_str}"]}}  # Dynamically insert parameters here
    #                     ) {{
    #                         results {{
    #                             name
    #                             value
    #                             displayValue
    #                             definition {{
    #                                 units {{
    #                                     name
    #                                 }}
    #                             }}
    #                         }}
    #                     }}
    #                 }}
    #             }}
    #         }}
    #     """
    #
    #     data = {
    #         "query": query,
    #         "variables": {
    #             "projectId": projectId,
    #             "propertyFilter": "'property.name.Element Context'==Instance"
    #         }
    #     }
    #
    #     # Execute the query
    #     result = self.execute_query_variables(data['query'], data['variables'])
    #
    #     # Normalize the data into a pandas DataFrame
    #     elements = result['data']['elementsByProject']['results']
    #     df = pd.json_normalize(elements)
    #
    #     data_df = pd.DataFrame()
    #     for i, row in df.iterrows():
    #         props_dict = {}
    #         single_df = pd.json_normalize(row['properties.results'])
    #         for i in range(len(single_df)):
    #             props_dict[single_df['name'][i]] = single_df['value'][i]
    #         single_df = pd.DataFrame(props_dict, index=[0], dtype="object")
    #         data_df = pd.concat([data_df, single_df], axis=0)
    #
    #     return data_df

    def get_elements_by_projects(self, projectId: str, cursor: str = None, is_recursive: bool = False) -> pd.DataFrame:
        # Define the GraphQL query with the cursor as a dynamic variable
        data = {
            "query": """
                query GetElementsInProject($projectId: ID!, $propertyFilter: String!, $cursor: String) {
                    elementsByProject(projectId: $projectId, filter: {query: $propertyFilter}, pagination: {cursor: $cursor}) {
                        pagination {
                            cursor
                        }
                        results {
                            id
                            name
                            properties(
                                includeReferencesProperties: "Type"
                                
                            ) {
                                results {
                                    name
                                    value
                                    displayValue
                                    definition {
                                        units {
                                            name
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            """,
            "variables": {
                "projectId": projectId,
                "propertyFilter": "'property.name.Element Context'==Instance",
                "cursor": cursor
            }
        }

        # Execute the query
        result = self.execute_query_variables(data['query'], data['variables'])

        # Process the results into a pandas DataFrame
        elements = result['data']['elementsByProject']['results']
        df = pd.json_normalize(elements)

        data_df = pd.DataFrame()
        for i, row in df.iterrows():
            props_dict = {}
            id = row['id']
            single_df = pd.json_normalize(row['properties.results'])
            for i in range(len(single_df)):
                props_dict[single_df['name'][i]] = single_df['value'][i]
            props_dict['id'] = id
            single_df = pd.DataFrame(props_dict, index=[0], dtype="object")
            data_df = pd.concat([data_df, single_df], axis=0)
        # recursive call to get all data if cursor is not None
        cursor = result['data']['elementsByProject']['pagination']['cursor']
        if cursor and is_recursive:
            # merge by id
            data_df = pd.concat([data_df, self.get_elements_by_projects(projectId, cursor)], axis=0)
        return data_df
