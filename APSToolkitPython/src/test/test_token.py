from unittest import TestCase
import os
from .context import Token
from .context import RevokeType
from .context import ClientType
from .context import Auth


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
