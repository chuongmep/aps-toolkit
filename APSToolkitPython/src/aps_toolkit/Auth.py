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
        content = response.json()
        access_token = content['access_token']
        expires_in = content['expires_in']
        token_type = content['token_type']
        result = Token(access_token, token_type, expires_in)
        return result