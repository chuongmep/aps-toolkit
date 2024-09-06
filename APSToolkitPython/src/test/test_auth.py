from unittest import TestCase
import os
from .context import Auth


class TestAuth(TestCase):
    def test_auth(self):
        client_id = os.environ['APS_CLIENT_ID']
        client_secret = os.environ['APS_CLIENT_SECRET']
        auth = Auth(client_id, client_secret)
        token = auth.auth2leg()
        self.assertNotEquals(token.access_token, "")

    def test_auth2(self):
        auth = Auth()
        token = auth.auth2leg()
        self.assertNotEquals(token.access_token, "")

    def test_auth3leg(self):
        auth = Auth()
        redirect_uri = "http://localhost:8080/api/auth/callback"
        # https://aps.autodesk.com/en/docs/oauth/v2/developers_guide/scopes
        scopes = 'data:read viewables:read'
        token = auth.auth3leg(redirect_uri, scopes)
        print(token.refresh_token)
        self.assertNotEquals(token.access_token, "")

    def test_auth3legPkce(self):
        auth = Auth()
        redirect_uri = "http://localhost:8080/api/auth/callback"
        # https://aps.autodesk.com/en/docs/oauth/v2/developers_guide/scopes
        scopes = 'data:read viewables:read'
        client_id = os.environ['APS_CLIENT_PKCE_ID']
        token = auth.auth3legPkce(client_id, redirect_uri, scopes)
        print(token.refresh_token)
        self.assertNotEquals(token.access_token, "")

    def test_refresh_token(self):
        auth = Auth()
        token = auth.auth3leg()
        self.assertNotEquals(token.access_token, "")
        print("Refresh token: ", token.refresh_token)
        print("Start refresh token")
        new_token = auth.refresh_new_token(token.refresh_token)
        print("New Fresh Token", new_token.refresh_token)
        self.assertNotEquals(token.access_token, "")
        self.assertNotEquals(token.refresh_token, "")

    def test_refresh_token_from_env(self):
        token = Auth.refresh_token_from_env()
        self.assertNotEquals(token.access_token, "")

    def test_get_user_info(self):
        auth = Auth()
        token = auth.auth3leg()
        user_info = auth.get_user_info()
        self.assertNotEquals(user_info, "")
