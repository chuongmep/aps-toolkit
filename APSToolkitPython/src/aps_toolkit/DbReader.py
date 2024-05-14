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
import os
import requests
from .Auth import Auth
import pandas as pd
import sqlite3
from .Token import Token


class DbReader:
    def __init__(self, urn: str, token: Token = None):
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
        if response.status_code != 200:
            raise Exception(response.content)
        json_response = response.json()
        if json_response["status"] != "success":
            raise Exception(json_response)
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

    def execute_query(self, query: str):
        conn = sqlite3.connect(self.db_path)
        return pd.read_sql_query(query, conn)
