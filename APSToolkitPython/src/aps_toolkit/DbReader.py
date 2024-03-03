import os
import requests
from .Auth import Auth
import pandas as pd
import sqlite3


class DbReader:
    def __init__(self, urn, token=None):
        self.urn = urn
        if token is None:
            auth = Auth()
            self.token = auth.auth2leg()
        else:
            self.token = token
        self.host = "https://developer.api.autodesk.com"
        url = f"{self.host}/modelderivative/v2/designdata/{self.urn}/manifest"
        headers = {
            "Authorization": f"Bearer {self.token.access_token}"
        }
        response = requests.get(url, headers=headers)
        json_response = response.json()
        childrens = json_response['derivatives'][0]["children"]
        self.path = ""
        for child in childrens:
            if child["type"] == "resource" and child["mime"] == "application/autodesk-db":
                self.path = child["urn"]
                break
        url = f"{self.host}/modelderivative/v2/designdata/{self.urn}/manifest/{self.path}"
        response = requests.get(url, headers=headers)
        temp_path = os.path.join(os.path.dirname(__file__), "database")
        extension = self.path.split(".")[-1]
        temp_path = os.path.join(temp_path, self.urn + "." + extension)
        self.db_path = temp_path
        if not os.path.exists(temp_path):
            os.makedirs(os.path.dirname(temp_path), exist_ok=True)
        with open(temp_path, "wb") as file:
            file.write(response.content)
            file.close()

    def execute_query(self, query):
        conn = sqlite3.connect(self.db_path)
        return pd.read_sql_query(query, conn)
