from unittest import TestCase

from .context import Derivative
from .context import Auth


class TestDerivative(TestCase):
    def setUp(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLlhUOFFRSk53UXhpTFE2VE1QbmZRTkE_dmVyc2lvbj0x"
        self.token = Auth().auth2leg()

    def test_translate_job(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLlhUOFFRSk53UXhpTFE2VE1QbmZRTkE_dmVyc2lvbj0x"
        derivative = Derivative(self.urn, self.token)
        response = derivative.translate_job("Project Completion.nwd")
        self.assertNotEquals(response, "")

    def test_check_job_status(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLlhUOFFRSk53UXhpTFE2VE1QbmZRTkE_dmVyc2lvbj0x"
        derivative = Derivative(self.urn, self.token)
        response = derivative.check_job_status()
        self.assertNotEquals(response, "")

    def test_read_svf_manifest_items(self):
        derivative = Derivative(self.urn, self.token)
        manifest_items = derivative.read_svf_manifest_items()
        self.assertNotEquals(len(manifest_items), 0)

    def test_read_svf_resource(self):
        derivative = Derivative(self.urn, self.token)
        svf_resources = derivative.read_svf_resource()
        self.assertNotEquals(len(svf_resources), 0)

    def test_read_svf_resource_item(self):
        derivative = Derivative(self.urn, self.token)
        manifest_items = derivative.read_svf_manifest_items()
        svf_resources = derivative.read_svf_resource_item(manifest_items[0])
        self.assertNotEquals(len(svf_resources), 0)
