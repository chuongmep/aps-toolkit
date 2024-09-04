from unittest import TestCase
import os
from .context import Token
from .context import RevokeType
from .context import ClientType
from .context import Auth
import datetime
import time


class TestAuth(TestCase):

    def test_revoke_token_private(self):
        token = Auth().auth2leg()
        os.environ['APS_ACCESS_TOKEN'] = token.access_token
        token = Token.revoke(RevokeType.TOKEN_PRIVATE)
        self.assertNotEquals(token.access_token, "")

    def test_revoke_refresh_token_private(self):
        token = Token.revoke(RevokeType.REFRESH_TOKEN_PRIVATE)
        self.assertNotEquals(token.access_token, "")

    def test_introspect(self):
        token = Auth().auth2leg()
        token.set_env()
        result = token.introspect(ClientType.PRIVATE)
        self.assertNotEquals(result, "")

    def test_is_expired(self):
        token = Auth().auth2leg()
        # downtime to 1 minutes
        token.expires_in = time.time() - 1 * 60
        self.assertTrue(token.is_expired())
        token.expires_in = time.time() + 1 * 60
        self.assertFalse(token.is_expired())

    def test_refresh_token(self):
        token = Auth().auth3leg()
        token.refresh()
        token.refresh()
        self.assertNotEqual(token.access_token, "")
