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
from .Token import Token


class Auth:
    def __init__(self, client_id=None, client_secret=None):
        if client_id and client_secret:
            self.client_id = client_id
            self.client_secret = client_secret
        else:
            self.client_id = os.environ.get('APS_CLIENT_ID')
            self.client_secret = os.environ.get('APS_CLIENT_SECRET')

    def auth2leg(self) -> Token:
        Host = "https://developer.api.autodesk.com"
        url = "/authentication/v2/token"

        # body
        body = {
            "client_id": self.client_id,
            "client_secret": self.client_secret,
            "grant_type": "client_credentials",
            "scope": "data:read data:write data:search data:create bucket:read bucket:create user:read"
        }
        response = requests.post(Host + url, data=body)
        if response.status_code != 200:
            raise Exception(response.content)
        content = response.json()
        access_token = content['access_token']
        expires_in = content['expires_in']
        token_type = content['token_type']
        result = Token(access_token, token_type, expires_in)
        return result
