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
import urllib.parse
import webbrowser
import requests
import os
from http.server import HTTPServer, BaseHTTPRequestHandler
import http.server
from typing import Optional
import hashlib
import base64
import random
import string
import datetime


class Auth:
    def __init__(self, client_id: Optional[str] = None, client_secret: Optional[str] = None):
        if client_id and client_secret:
            self.client_id = client_id
            self.client_secret = client_secret
        else:
            self.client_id = os.environ.get('APS_CLIENT_ID')
            self.client_secret = os.environ.get('APS_CLIENT_SECRET')
        # Initialize token variables
        self.access_token = None
        self.token_type = None
        self.expires_in = None
        self.refresh_token = None

    def auth2leg(self) -> Token:
        """
        This method is used to authenticate an application using the 2-legged OAuth flow.
        https://aps.autodesk.com/en/docs/oauth/v2/tutorials/get-2-legged-token/
       :return: :class:`Token`: An instance of the Token class containing the access token, token type, and expiration time.
        """
        Host = "https://developer.api.autodesk.com"
        url = "/authentication/v2/token"

        # body
        body = {
            "client_id": self.client_id,
            "client_secret": self.client_secret,
            "grant_type": "client_credentials",
            "scope": "data:read data:write data:search data:create bucket:read bucket:create user:read bucket:update bucket:delete code:all"
        }
        response = requests.post(Host + url, data=body)
        if response.status_code != 200:
            raise Exception(response.content)
        content = response.json()
        self.access_token = content['access_token']
        second = content['expires_in']
        now = datetime.datetime.now()
        expires = now + datetime.timedelta(seconds=second)
        self.expires_in = expires.timestamp()
        self.token_type = content['token_type']
        result = Token(self.access_token, self.token_type, self.expires_in)
        return result

    def auth3leg(self, callback_url: str = None, scopes: str = None) -> Token:
        """
        This method is used to authenticate a user using the 3-legged OAuth flow.
        https://aps.autodesk.com/en/docs/oauth/v2/tutorials/get-3-legged-token/
        :param callback_url: This is the URL-encoded callback URL you want the user redirected to after they grant consent. In this example, that URL is http://localhost:8080/oauth/callback/. Replace the value here with the appropriate URL for your web app. Note that it must match the pattern specified for the callback URL in your app’s registration in the APS developer portal.
        :param scopes: This requests the data:read scope. You can leave this value as it is for the purpose of this example, but in your own app, you should request one or more scopes you actually need. If you need to include multiple scopes, you can include them all as space-delimited items. For example: scope=data:create%20data:read%20data:write includes data:read, data:write, and data:create scopes.
        :return: :class:`Token`:  An instance of the Token class containing the access token, token type, expiration time, and refresh token (if available).
        """
        if not scopes:
            scopes = 'data:read data:write data:create data:search bucket:create bucket:read bucket:update bucket:delete code:all'
        if not callback_url:
            # Default callback url
            callback_url = "http://localhost:8080/api/auth/callback"

        class CallbackHandler(BaseHTTPRequestHandler):
            def do_GET(self):
                nonlocal auth_instance
                query = urllib.parse.urlparse(self.path).query
                params = urllib.parse.parse_qs(query)
                code = params.get('code', [''])[0]
                if code:
                    self.send_response(200)
                    self.end_headers()
                    self.wfile.write(b"Authentication successful. You can close this window now.")
                    result_token = handle_callback(callback_url, code)
                    auth_instance.access_token = result_token['access_token']
                    auth_instance.token_type = result_token['token_type']
                    now = datetime.datetime.now()
                    second = result_token['expires_in']
                    expires = now + datetime.timedelta(seconds=second)
                    auth_instance.expires_in = expires.timestamp()
                    auth_instance.refresh_token = result_token.get('refresh_token')

                else:
                    self.send_response(400)
                    self.end_headers()
                    self.wfile.write(b"Bad Request")

        def handle_callback(callback_url, code):
            tokenUrl = "https://developer.api.autodesk.com/authentication/v2/token"
            payload = {
                "grant_type": "authorization_code",
                "code": code,
                "client_id": self.client_id,
                "client_secret": self.client_secret,
                "redirect_uri": callback_url
            }
            resp = requests.post(tokenUrl, data=payload)
            if resp.status_code != 200:
                raise Exception(resp.content)
            respJson = resp.json()
            return respJson

        def start_callback_server(callback_url):
            parsed_url = urllib.parse.urlparse(callback_url)
            server_address = ('', parsed_url.port if parsed_url.port else 8080)
            httpd = HTTPServer(server_address, CallbackHandler)
            httpd.handle_request()

        auth_instance = self  # Reference to the Auth instance

        auth_url = f"https://developer.api.autodesk.com/authentication/v2/authorize?response_type=code&client_id={self.client_id}&redirect_uri={callback_url}&scope={scopes}"
        webbrowser.open(auth_url)
        start_callback_server(callback_url)

        # Return the token object using the global variables
        return Token(self.access_token, self.token_type, self.expires_in, self.refresh_token)

    def auth3legPkce(self, clientId: Optional[str] = None, callback_url: Optional[str] = None,
                     scopes: Optional[str] = None) -> Token:
        """
        This method is used to authenticate a user using the 3-legged OAuth PKCE flow.
        https://aps.autodesk.com/blog/new-application-types
        Parameters:
        :param clientId: The client ID of the application. If not provided, it will use the client ID from the environment variables.
        :param callback_url: The callback URL where the user will be redirected after authentication. If not provided, it defaults to "http://localhost:8080/api/auth/callback".
        :param scopes: The scopes for which the application is requesting access. If not provided, it defaults to 'data:read data:write data:create data:search bucket:create bucket:read bucket:update bucket:delete code:all'.
        :Returns: :class:`Token`: An instance of the Token class containing the access token, token type, expiration time, and refresh token (if available).
        """
        if clientId is None:
            self.client_id = os.environ.get('APS_CLIENT_PKCE_ID')
        else:
            self.client_id = clientId
        if not scopes:
            scopes = 'data:read data:write data:create data:search bucket:create bucket:read bucket:update bucket:delete code:all'
        if not callback_url:
            # Default callback url
            callback_url = "http://localhost:8080/api/auth/callback"

        code_verifier = self.random_string(64)
        code_challenge = self.generate_code_challenge(code_verifier)

        class CallbackHandler(BaseHTTPRequestHandler):
            def do_GET(self):
                nonlocal auth_instance
                query = urllib.parse.urlparse(self.path).query
                params = urllib.parse.parse_qs(query)
                code = params.get('code', [''])[0]
                if code:
                    self.send_response(200)
                    self.end_headers()
                    self.wfile.write(b"Authentication successful. You can close this window now.")
                    result_token = handle_callback(callback_url, code, auth_instance.client_id, code_verifier)
                    auth_instance.access_token = result_token['access_token']
                    auth_instance.token_type = result_token['token_type']
                    now = datetime.datetime.now()
                    second = result_token['expires_in']
                    expires = now + datetime.timedelta(seconds=second)
                    auth_instance.expires_in = expires.timestamp()
                    auth_instance.refresh_token = result_token.get('refresh_token')

                else:
                    self.send_response(400)
                    self.end_headers()
                    self.wfile.write(b"Bad Request")

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

        def start_callback_server(callback_url):
            parsed_url = urllib.parse.urlparse(callback_url)
            server_address = ('', parsed_url.port if parsed_url.port else 8080)
            httpd = HTTPServer(server_address, CallbackHandler)
            httpd.handle_request()

        auth_instance = self  # Reference to the Auth instance

        auth_url = f"https://developer.api.autodesk.com/authentication/v2/authorize?response_type=code&client_id={self.client_id}&redirect_uri={callback_url}&scope={scopes}&code_challenge={code_challenge}&code_challenge_method=S256"
        webbrowser.open(auth_url)
        start_callback_server(callback_url)

        # Return the token object using the global variables
        return Token(self.access_token, self.token_type, self.expires_in, self.refresh_token)

    @staticmethod
    def random_string(length):
        chars = string.ascii_letters + string.digits + '-._~'
        return ''.join(random.choice(chars) for _ in range(length))

    @staticmethod
    def generate_code_challenge(code_verifier):
        sha256 = hashlib.sha256(code_verifier.encode()).digest()
        code_challenge = base64.urlsafe_b64encode(sha256).rstrip(b'=')
        return code_challenge.decode()

    def refresh_new_token(self, old_refresh_token: str) -> Token:
        Host = "https://developer.api.autodesk.com"
        url = "/authentication/v2/token"

        # body
        body = {
            "client_id": self.client_id,
            "client_secret": self.client_secret,
            "grant_type": "refresh_token",
            "refresh_token": old_refresh_token
        }
        response = requests.post(Host + url, data=body)
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
        result = Token(self.access_token, self.token_type, self.expires_in, self.refresh_token)
        return result

    @staticmethod
    def refresh_token_from_env(refresh_token: str = None) -> Token:
        """
        Refresh to new token from the environment variables (auth 3legend).
        :param refresh_token: The refresh token. If not provided, it will use the refresh token from the environment variables.
        :return: :class:`Token`: An instance of the Token class containing the access token, token type, expiration time, and refresh token (if available).
        """
        if refresh_token is None:
            refresh_token = os.getenv('APS_REFRESH_TOKEN')
        else:
            refresh_token = refresh_token
        token = Token(refresh_token=refresh_token)
        if refresh_token is not None:
            try:
                client_id = os.getenv('APS_CLIENT_ID')
                client_secret = os.getenv('APS_CLIENT_SECRET')
                token.refresh(client_id, client_secret)
                token.set_env()
                print('Token refreshed')
            except Exception as e:
                print('Token refresh failed, try to re login', e)
                token = Auth().auth3leg()
                token.set_env()
                print('Token refreshed')
        return token

    def get_user_info(self) -> dict:
        """
        This method is used to get user information.
        It requires OAuth 2.0 authentication with 3-legged flow.
        https://developer.api.autodesk.com/userprofile/v1/userinfo
        :return:  A dictionary containing user information.
        """
        if not self.access_token:
            raise Exception("Access token is required, please authenticate first. Use auth2leg or auth3leg method.")
        url = "https://api.userprofile.autodesk.com/userinfo"
        headers = {
            "Authorization": f"{self.token_type} {self.access_token}"
        }
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        return response.json()
