from unittest import TestCase
import os
from .context import AECDataModel
from .context import Auth


class TestAECDataModel(TestCase):
    def setUp(self):
        # read refresh token from json
        if not os.path.exists('refresh_token.json'):
            with open('refresh_token.json', 'w') as f:
                f.write('')
        with open('refresh_token.json', 'r') as f:
            self.refresh_token = f.read()
        self.token = Auth.refresh_token_from_env(self.refresh_token)
        self.refresh_token = self.token.refresh_token
        # save to json
        with open('refresh_token.json', 'w') as f:
            f.write(self.refresh_token)
        self.hub_id = "urn:adsk.ace:prod.scope:207eaa13-b1e2-4e99-9f20-dc94fc599272"
        self.project_id = "urn:adsk.workspace:prod.project:f4d95147-4eef-4664-89e0-a4f01f8a7b71"
        self.group_id = "YWVjZH45enZvNHRHazl1RTI4VVc0NUsySkgzX0wyQ35JZE10SWtTU1I2eXBaTjFfLTJVd0RR"
        self.aec_data_model = AECDataModel(self.token)

    def test_get_hubs(self):
        df_result = self.aec_data_model.get_hubs()
        self.assertIsNotNone(df_result)

    def test_get_projects(self):
        result = self.aec_data_model.get_projects(self.hub_id)
        self.assertIsNotNone(result)

    def test_get_folders(self):
        result = self.aec_data_model.get_folders(self.project_id)
        self.assertIsNotNone(result)

    def test_get_element_group_by_project(self):
        result = self.aec_data_model.get_element_group_by_project(self.project_id)
        self.assertIsNotNone(result)

    def test_get_element_by_category(self):
        result = self.aec_data_model.get_element_by_category(self.group_id, "Doors")
        self.assertIsNotNone(result)

    # def test_get_element_projects_by_parameters(self):
    #     result = self.aec_data_model.get_element_projects_by_parameters(self.project_id,
    #                                                                     ["Name", "Revit Element ID", "Category",
    #                                                                      "Width", "Height", "Element Context",
    #                                                                      "Family Name", "Type Name", "Comments"])
    #     self.assertIsNotNone(result)

    def test_get_elements_by_projects(self):
        cursor = "YWRjdXJzfjB-NTB-NTA"
        # cursor = ""
        result = self.aec_data_model.get_elements_by_projects(self.project_id, cursor)
        self.assertIsNotNone(result)

    def test_version_group_by_project(self):
        query = """
            query GetElementGroupsByProject($projectId: ID!) {
                elementGroupsByProject(projectId: $projectId) {
                    results {
                        name
                        id
                        alternativeIdentifiers {
                            fileUrn
                            fileVersionUrn
                        }
                    }
                }
            }
        """
        variables = {
            "projectId": f"{self.project_id}"  # Replace with your actual project ID
        }
        result = self.aec_data_model.execute_query_variables(query, variables)
        self.assertIsNotNone(result)

    def test_get_elements_from_type(self):
        query = """
                    query ($elementGroupId: ID!, $propertyFilter: String!) {
                elementsByElementGroup(
                    elementGroupId: $elementGroupId
                    filter: { query: $propertyFilter }
                    pagination: {limit: 5}
                ) {
                    pagination {
                        cursor
                    }
                    results {
                        id
                        name
                        properties {
                            results {
                            name
                            value
                            }
                        }
                        referencedBy(name: "Type") {
                            pagination {
                                cursor
                            }
                            results {
                                id
                                name
                                alternativeIdentifiers {
                                    externalElementId
                                }
                                properties {
                                    results {
                                        name
                                        value
                                    }
                                }
                            }
                        }
                    }
                }
            }
            """
        variables = {
            "elementGroupId": f"{self.group_id}",  # Replace with your actual element group ID
            "propertyFilter": "'property.name.category'=contains=Walls and 'property.name.Element Context'==Type'"
        }
        result = self.aec_data_model.execute_query_variables(query, variables)
        self.assertIsNotNone(result)

    def test_get_data_instance_type_category(self):
        query = """
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
        """

        # Set the variables (replace with your actual values)
        variables = {
            "elementGroupId": "YWVjZH45enZvNHRHazl1RTI4VVc0NUsySkgzX0wyQ35yTHd5MTNSSlFhMml5cmlCZ1NMd3ZB",
            "propertyFilter": "'property.name.category'==Walls and 'property.name.Element Context'==Instance"
        }
        result = self.aec_data_model.execute_query_variables(query, variables)
        self.assertIsNotNone(result)
