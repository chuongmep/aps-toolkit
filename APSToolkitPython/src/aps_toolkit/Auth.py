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
        access_token = content['access_token']
        expires_in = content['expires_in']
        token_type = content['token_type']
        result = Token(access_token, token_type, expires_in)
        return result

    def auth3leg(self, callback_url=None, scopes=None) -> Token:
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
                    auth_instance.expires_in = result_token['expires_in']
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

        def start_callback_server():
            server_address = ('', 8080)
            httpd = HTTPServer(server_address, CallbackHandler)
            httpd.handle_request()

        auth_instance = self  # Reference to the Auth instance

        auth_url = f"https://developer.api.autodesk.com/authentication/v2/authorize?response_type=code&client_id={self.client_id}&redirect_uri={callback_url}&scope={scopes}"
        webbrowser.open(auth_url)
        start_callback_server()

        # Return the token object using the global variables
        return Token(self.access_token, self.token_type, self.expires_in, self.refresh_token)
