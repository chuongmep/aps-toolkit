from unittest import TestCase
from .context import SVFImage
from .context import Auth
from .context import Derivative


class TestImage(TestCase):
    def setUp(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0yNA"
        self.token = Auth().auth2leg()

    def test_parse_images_from_urn(self):
        images = SVFImage.parse_images_from_urn(self.urn, self.token)
        self.assertNotEquals(len(images), 0)

    def test_parse_images_from_derivative(self):
        derivative = Derivative(self.urn, self.token)
        manifest_items = derivative.read_svf_manifest_items()
        images = SVFImage.parse_images_from_derivative(derivative, manifest_items[0])
        self.assertNotEquals(len(images), 0)
