from unittest import TestCase

from .context import SVFReader
from .context import Auth
from .context import SVFMaterials
import os
from .context import Derivative


class TestMaterial(TestCase):
    def setUp(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0yNA"
        self.token = Auth().auth2leg()
        self.file_path = (r"./output/svfs/Resource/3D View/08f99ae5-b8be-4f8d-881b-128675723c10/Project "
                          r"Completion/Materials.json.gz")

    def test_parse_materials_from_urn(self):
        materials = SVFMaterials.parse_materials_from_urn(self.urn, self.token)
        self.assertNotEquals(len(materials), 0)

    def test_parse_materials_from_manifest_item(self):
        derivative = Derivative(self.urn, self.token)
        manifest_items = derivative.read_svf_manifest_items()
        materials = SVFMaterials.parse_materials_from_manifest_item(derivative, manifest_items[0])
        self.assertNotEquals(len(materials), 0)

    def test_read_from_file(self):
        self.assertTrue(os.path.exists(self.file_path))
        materials = SVFMaterials.parse_materials_from_file(self.file_path)
        self.assertNotEquals(len(materials), 0)
