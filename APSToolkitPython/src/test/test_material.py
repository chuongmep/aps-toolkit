from unittest import TestCase

from .context import SVFReader
from .context import Auth
from .context import SVFMaterials
import os


class TestMaterial(TestCase):
    def setUp(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0yNA"
        self.token = Auth().auth2leg()
        self.file_path = (r".\output\svfs\Resource\3D View\08f99ae5-b8be-4f8d-881b-128675723c10\Project "
                          r"Completion\Materials.json.gz")

    def test_read_from_file(self):
        materials = SVFMaterials.parse_materials_from_file(self.file_path)
        self.assertNotEquals(len(materials), 0)
