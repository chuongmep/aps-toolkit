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
from .Auth import Auth
from typing import Optional
import requests
from .Token import Token


class AuthGoogleColab(Auth):
    def __init__(self, client_id: Optional[str] = None, client_secret: Optional[str] = None):
        super().__init__(client_id, client_secret)

    def auth3leg(self, callback_url: str = None, scopes: str = None) -> Token:
        if not scopes:
            scopes = 'data:read data:write data:create data:search bucket:create bucket:read bucket:update bucket:delete code:all'
        if not callback_url:
            # Default callback url
            callback_url = "http://localhost:8080/api/auth/callback"
        auth_url = f"https://developer.api.autodesk.com/authentication/v2/authorize?response_type=code&client_id={self.client_id}&redirect_uri={callback_url}&scope={scopes}"
        auth_url = auth_url.replace(" ", "%20")
        print(f"Click the following link to authenticate:\n{auth_url}")
        auth_code = input("Enter the authorization code: ")
        # Exchange the authorization code for an access token
        token_url = "https://developer.api.autodesk.com/authentication/v2/token"
        payload = {
            "grant_type": "authorization_code",
            "code": auth_code,
            "client_id": self.client_id,
            "client_secret": self.client_secret,
            "redirect_uri": callback_url
        }
        response = requests.post(token_url, data=payload)
        if response.status_code != 200:
            raise Exception(response.content)
        response_json = response.json()

        # Return the token object
        return Token(
            access_token=response_json['access_token'],
            token_type=response_json['token_type'],
            expires_in=response_json['expires_in'],
            refresh_token=response_json.get('refresh_token')
        )

    def auth3legPkce(self, clientId: str = None, callback_url: str = None, scopes: str = None) -> Token:
        """
        This method is used to authenticate a user using the 3-legged OAuth PKCE flow.
        https://aps.autodesk.com/blog/new-application-types
        Parameters:
        clientId (str, optional): The client ID of the application. If not provided, it will use the client ID from the environment variables.
        callback_url (str, optional): The callback URL where the user will be redirected after authentication. If not provided, it defaults to "http://localhost:8080/api/auth/callback".
        scopes (str, optional): The scopes for which the application is requesting access. If not provided, it defaults to 'data:read data:write data:create data:search bucket:create bucket:read bucket:update bucket:delete code:all'.

        Returns:
        Token: An instance of the Token class containing the access token, token type, expiration time, and refresh token (if available).
        """

        if clientId:
            self.client_id = clientId
        if not scopes:
            scopes = 'data:read data:write data:create data:search bucket:create bucket:read bucket:update bucket:delete code:all'
        if not callback_url:
            # Default callback url
            callback_url = "http://localhost:8080/api/auth/callback"

        code_verifier = self.random_string(64)
        code_challenge = self.generate_code_challenge(code_verifier)

        def handle_callback(callback_url, code, client_id, code_verifier):
            token_url = "https://developer.api.autodesk.com/authentication/v2/token"
            payload = {
                "grant_type": "authorization_code",
                "code": code,
                "client_id": client_id,
                "code_verifier": code_verifier,
                "redirect_uri": callback_url
            }
            resp = requests.post(token_url, data=payload)
            if resp.status_code != 200:
                raise Exception(resp.content)
            resp_json = resp.json()
            return resp_json

        auth_url = f"https://developer.api.autodesk.com/authentication/v2/authorize?response_type=code&client_id={self.client_id}&redirect_uri={callback_url}&scope={scopes}&code_challenge={code_challenge}&code_challenge_method=S256"
        # encode the url with spaces
        auth_url = auth_url.replace(" ", "%20")
        print(f"Click the following link to authenticate:\n{auth_url}")
        auth_code = input("Enter the authorization code: ")
        response_json = handle_callback(callback_url, auth_code, self.client_id, code_verifier)
        self.access_token = response_json['access_token']
        self.token_type = response_json['token_type']
        self.expires_in = response_json['expires_in']
        self.refresh_token = response_json.get('refresh_token')
        # Return the token object using the global variables
        return Token(self.access_token, self.token_type, self.expires_in, self.refresh_token)
