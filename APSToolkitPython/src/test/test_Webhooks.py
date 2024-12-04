from unittest import TestCase
import os
from .context import Webhooks
from .context import Auth

class TestWebhooks(TestCase):

    def setUp(self):
        token = Auth().auth2leg()
        self.hooks = Webhooks(token)

    def test_get_all_hooks(self):
        result = self.hooks.get_all_hooks()
        self.assertIsNotNone(result)

    def test_batch_report_all_hooks(self):
        result = self.hooks.batch_report_all_hooks()
        self.assertIsNotNone(result)

    def test_get_all_app_hooks(self):
        result = self.hooks.get_all_app_hooks()
        self.assertIsNotNone(result)

    def test_batch_report_all_app_hooks(self):
        result = self.hooks.batch_report_all_app_hooks()
        self.assertIsNotNone(result)

    def test_get_hook_by_id(self):
        hook_id = "1c7844f5-adcd-4225-b8a6-d7a47cca05e4"
        result = self.hooks.get_hook_by_id(hook_id,"dm.version.added","data")
        self.assertIsNotNone(result)

    def test_delete_hook_by_id(self):
        hook_id = '4e911371-dc48-4380-9ebb-e126bbb312b1'
        event = 'dm.version.added'
        result = self.hooks.delete_hook_by_id(hook_id, event,"data")
        print(result)

    def test_create_system_event_hook(self):
        scope = {"folder": "urn:adsk.wipprod:fs.folder:co.stvP5WhOSWGMFPPINBxwcA"}  # changed to a dictionary
        callback_url = "http://localhost:8080/api/webhooks/callback"
        hookAttribute = {
            "callbackUrl": callback_url,
            "scope": scope,
            "name": "foo",
            "special_data": "hello world",
        }
        result = self.hooks.create_system_event_hook(scope, callback_url, hookAttribute=hookAttribute)
        self.assertIsNotNone(result)
