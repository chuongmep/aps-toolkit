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
