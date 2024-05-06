from unittest import TestCase
import os
from .context import AuthGoogleColab


class TestAuth(TestCase):
    def test_auth(self):
        client_id = os.environ['APS_CLIENT_ID']
        client_secret = os.environ['APS_CLIENT_SECRET']
        auth = AuthGoogleColab(client_id, client_secret)
        token = auth.auth2leg()
        self.assertNotEquals(token.access_token, "")

    def test_auth2leg(self):
        auth = AuthGoogleColab()
        token = auth.auth2leg()
        self.assertNotEquals(token.access_token, "")

    def test_auth3leg(self):
        auth = AuthGoogleColab()
        redirect_uri = "http://localhost:8080/api/auth/callback"
        # https://aps.autodesk.com/en/docs/oauth/v2/developers_guide/scopes
        scopes = 'data:read viewables:read'
        token = auth.auth3leg(redirect_uri, scopes)
        print(token.refresh_token)
        self.assertNotEquals(token.access_token, "")

    def test_auth3legPkce(self):
        auth = AuthGoogleColab()
        redirect_uri = "http://localhost:8080/api/auth/callback"
        # https://aps.autodesk.com/en/docs/oauth/v2/developers_guide/scopes
        scopes = 'data:read viewables:read'
        client_id = os.environ['APS_CLIENT_PKCE_ID']
        token = auth.auth3legPkce(client_id, redirect_uri, scopes)
        print("Refresh Token:", token.refresh_token)
        self.assertNotEquals(token.access_token, "")
