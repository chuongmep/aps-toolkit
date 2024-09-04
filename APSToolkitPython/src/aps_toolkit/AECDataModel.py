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
