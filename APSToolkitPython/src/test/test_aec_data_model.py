from unittest import TestCase
import os
from .context import AECDataModel
from .context import Auth


class TestAECDataModel(TestCase):
    def setUp(self):
        self.token = Auth().auth3leg()
        self.hub_id = "urn:adsk.ace:prod.scope:207eaa13-b1e2-4e99-9f20-dc94fc599272"
        self.project_id = "urn:adsk.workspace:prod.project:f4d95147-4eef-4664-89e0-a4f01f8a7b71"
        self.group_id = "YWVjZH45enZvNHRHazl1RTI4VVc0NUsySkgzX0wyQ35JZE10SWtTU1I2eXBaTjFfLTJVd0RR"
        self.aec_data_model = AECDataModel(self.token)

    def test_get_hubs(self):
        data = {
            "query": """
                query GetHubs {
                    hubs {
                        results {
                            id
                            name
                        }
                    }
                }
            """
        }
        result = self.aec_data_model.execute_query(data)
        self.assertIsNotNone(result)

    def test_get_projects(self):
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
                                        }
                                }
                }
                    """,
            "variables": {
                "hubId": f"{self.hub_id}"
            }
        }
        result = self.aec_data_model.execute_query(data)
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

    def test_get_element_by_category(self):
        query = """
            query GetElementsFromCategory($elementGroupId: ID!, $propertyFilter: String!) {
                elementsByElementGroup(elementGroupId: $elementGroupId, filter: {query:$propertyFilter}) {
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
                                definition {
                                    name
                                    units {
                                        id
                                        name
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
            "propertyFilter": "property.name.category==Walls"
            # Replace with your property filter property.name.category==Walls
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