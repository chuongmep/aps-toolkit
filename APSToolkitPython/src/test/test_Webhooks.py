from unittest import TestCase
import os
from .context import Webhooks


class TestWebhooks(TestCase):

    def setUp(self):
        self.hooks = Webhooks()

    def test_get_all_hooks(self):
        result = self.hooks.get_all_hooks()
        self.assertIsNotNone(result)

    def test_batch_report_all_hooks(self):
        result = self.hooks.batch_report_all_hooks()
        self.assertIsNotNone(result)

    def test_get_all_app_hooks(self):
        result = self.hooks.get_all_app_hooks()
        self.assertIsNotNone(result)

    def test_get_hook_by_id(self):
        hook_id = "d7ead216-08ec-4bee-ab60-5ca4f137d946"
        result = self.hooks.get_hook_by_id(hook_id)
        self.assertIsNotNone(result)

    def test_delete_hook_by_ida(self):
        hook_id = '72c5818e-2ce7-4d71-aec5-183d2c9ff925'
        event = 'dm.folder.copied.out'
        result = self.hooks.delete_hook_by_id(hook_id, event)
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
