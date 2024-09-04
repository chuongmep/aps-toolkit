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
import base64
from enum import Enum
import time
import datetime

class RevokeType(Enum):
    TOKEN_PUBLIC = 1
    TOKEN_PRIVATE = 2
    REFRESH_TOKEN_PUBLIC = 3
    REFRESH_TOKEN_PRIVATE = 4


class ClientType(Enum):
    PUBLIC = 1
    PRIVATE = 2


class Token():
    def __init__(self, access_token: str = None, token_type: str = None, expires_in: float = None,
                 refresh_token: str = None,
                 SetEnv: bool = False):
        """
        Initialize Token
        :param access_token: the access token used to authenticate
        :param token_type:  the type of token
        :param expires_in:  the time in seconds that the token will expire
        :param refresh_token:  the refresh token used to get a new access token
        :param SetEnv:  set the access token and refresh token to the environment variables
        """
        if SetEnv:
            os.environ["APS_ACCESS_TOKEN"] = access_token
            os.environ["APS_REFRESH_TOKEN"] = refresh_token
        self.access_token = access_token
        self.token_type = token_type
        self.expires_in = expires_in
        self.refresh_token = refresh_token

    def set_env(self):
        """
        Set the access token and refresh token to the environment variables
        :return:
        """
        os.environ["APS_ACCESS_TOKEN"] = self.access_token if self.access_token is not None else ""
        os.environ["APS_REFRESH_TOKEN"] = self.refresh_token if self.refresh_token is not None else ""

    def is_expired(self, buffer_minutes=0) -> bool:
        """
        Check if the token is expired
        :return: True if the token is expired, False otherwise
        """
        time_stamp_now = time.time()
        if self.expires_in is None:
            return False
        if time_stamp_now + buffer_minutes * 60 >= self.expires_in:
            return True
        return False

    def refresh(self, client_id: str = None, client_secret: str = None, refresh_token: str = None):
        """
        Refresh the access token
        :param client_id: the client id of the application
        :param client_secret: the client secret of the application
        :param refresh_token: the refresh token used to get a new access token
        """
        host = "https://developer.api.autodesk.com"
        url = "/authentication/v2/token"
        if client_id is None:
            client_id = os.getenv("APS_CLIENT_ID")
        if client_secret is None:
            client_secret = os.getenv("APS_CLIENT_SECRET")
        if refresh_token is None:
            refresh_token = self.refresh_token
            if refresh_token is None:
                raise Exception("refresh_token is not provided, please provide the refresh_token or use Auth3leg")
        # body
        body = {
            "client_id": client_id,
            "client_secret": client_secret,
            "grant_type": "refresh_token",
            "refresh_token": self.refresh_token
        }
        response = requests.post(host + url, data=body)
        if response.status_code != 200:
            raise Exception(response.reason)
        content = response.json()
        self.access_token = content['access_token']
        second = content['expires_in']
        now = datetime.datetime.now()
        expires = now + datetime.timedelta(seconds=second)
        self.expires_in = expires.timestamp()
        self.token_type = content['token_type']
        self.refresh_token = content.get('refresh_token')

    def introspect(self, client_type: ClientType) -> dict:
        """
        Examines an access token including the reference token and returns the status information of the tokens.
        If the token is active, additional information is returned.
        :param :class:`ClientType` : the type of client \n
        PUBLIC : use public APS_CLIENT_ID, APS_ACCESS_TOKEN \n
        PRIVATE : use private APS_CLIENT_ID , APS_CLIENT_SECRET and APS_ACCESS_TOKEN \n
        :return:
        """
        if self.access_token is None:
            self.access_token = os.getenv("APS_ACCESS_TOKEN")
            if self.access_token is None:
                raise Exception("access_token is not provided.")
        url = "https://developer.api.autodesk.com/authentication/v2/introspect"
        if client_type == ClientType.PUBLIC:
            token = self.access_token
            client_id = os.getenv("APS_CLIENT_ID")
            if client_id is None:
                raise Exception("APS_CLIENT_ID is not provided.")
            headers = {"Content-Type": "application/x-www-form-urlencoded"}
            data = {
                "token": token,
                "client_id": client_id
            }
            result = requests.post(url, headers=headers, data=data)
            if result.status_code != 200:
                raise Exception(result.content)
            return result.json()
        elif client_type == ClientType.PRIVATE:
            token = self.access_token
            client_id = os.getenv("APS_CLIENT_ID")
            if client_id is None:
                raise Exception("APS_CLIENT_ID is not provided.")
            client_secret = os.getenv("APS_CLIENT_SECRET")
            if client_secret is None:
                raise Exception("APS_CLIENT_SECRET is not provided.")
            auth = f"Basic {base64.b64encode(f'{client_id}:{client_secret}'.encode()).decode()}"
            headers = {
                "Content-Type": "application/x-www-form-urlencoded",
                "Authorization": auth
            }
            data = {
                "token": token
            }
            result = requests.post(url, headers=headers, data=data)
            if result.status_code != 200:
                raise Exception(result.content)
            return result.json()

    @staticmethod
    def revoke(RevokeType: RevokeType = RevokeType.REFRESH_TOKEN_PRIVATE):
        """
        Revoke the token
        :param :class:`RevokeType` the type of token to revoke \n
        TOKEN_PUBLIC : use public APS_CLIENT_ID, APS_ACCESS_TOKEN \n
        TOKEN_PRIVATE : use private APS_CLIENT_ID and APS_CLIENT_SECRET, APS_ACCESS_TOKEN \n
        REFRESH_TOKEN_PUBLIC : use public APS_CLIENT_ID, APS_REFRESH_TOKEN \n
        REFRESH_TOKEN_PRIVATE : use private APS_CLIENT_ID and APS_CLIENT_SECRET, APS_REFRESH_TOKEN \n
        :return:  the response content
        """
        url = "https://developer.api.autodesk.com/authentication/v2/revoke"
        if RevokeType == RevokeType.TOKEN_PUBLIC:
            token = os.getenv("APS_ACCESS_TOKEN")
            if token is None:
                raise Exception("access_token is not provided.")
            headers = {
                "Content-Type": "application/x-www-form-urlencoded"
            }
            client_id = os.getenv("APS_CLIENT_ID")
            if client_id is None:
                raise Exception("APS_CLIENT_ID is not provided.")
            data = {
                "token": token,
                "token_type_hint": "access_token",
                "client_id": client_id

            }
            result = requests.post(url, headers=headers, data=data)
            if result.status_code != 200:
                raise Exception(result.content)
            return result.content
        elif RevokeType == RevokeType.TOKEN_PRIVATE:
            client_id = os.getenv("APS_CLIENT_ID")
            if client_id is None:
                raise Exception("APS_CLIENT_ID is not provided.")
            client_secret = os.getenv("APS_CLIENT_SECRET")
            if client_secret is None:
                raise Exception("APS_ACCESS_TOKEN is not provided.")
            auth = f"Basic {base64.b64encode(f'{client_id}:{client_secret}'.encode()).decode()}"
            headers = {
                "Content-Type": "application/x-www-form-urlencoded",
                "Authorization": auth
            }
            token = os.getenv("APS_ACCESS_TOKEN")
            if token is None:
                raise Exception("APS_ACCESS_TOKEN is not provided.")
            data = {
                "token": token,
                "token_type_hint": "access_token"
            }
            result = requests.post(url, headers=headers, data=data)
            if result.status_code != 200:
                raise Exception(result.content)
            return result.content
        elif RevokeType == RevokeType.REFRESH_TOKEN_PUBLIC:
            refresh_token = os.getenv("APS_REFRESH_TOKEN")
            if refresh_token is None:
                raise Exception("APS_REFRESH_TOKEN is not provided.")
            headers = {
                "Content-Type": "application/x-www-form-urlencoded"
            }
            data = {
                "token": refresh_token,
                "token_type_hint": "refresh_token"
            }
            client_id = os.getenv("APS_CLIENT_ID")
            if client_id is None:
                raise Exception("APS_CLIENT_ID is not provided.")
            result = requests.post(url, headers=headers, data=data)
            if result.status_code != 200:
                raise Exception(result.content)
            return result.content
        elif RevokeType == RevokeType.REFRESH_TOKEN_PRIVATE:
            client_id = os.getenv("APS_CLIENT_ID")
            if client_id is None:
                raise Exception("APS_CLIENT_ID is not provided.")
            client_secret = os.getenv("APS_CLIENT_SECRET")
            if client_secret is None:
                raise Exception("APS_ACCESS_TOKEN is not provided.")
            auth = f"Basic {base64.b64encode(f'{client_id}:{client_secret}'.encode()).decode()}"
            headers = {
                "Content-Type": "application/x-www-form-urlencoded",
                "Authorization": auth
            }
            refresh_token = os.getenv("APS_REFRESH_TOKEN")
            if refresh_token is None:
                raise Exception("APS_REFRESH_TOKEN is not provided.")
            data = {
                "token": refresh_token,
                "token_type_hint": "refresh_token"
            }
            result = requests.post(url, headers=headers, data=data)
            if result.status_code != 200:
                raise Exception(result.content)
            return result.content
