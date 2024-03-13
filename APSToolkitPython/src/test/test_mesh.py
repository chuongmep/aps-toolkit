from unittest import TestCase
from .context import Mesh
from .context import Auth


class TestMesh(TestCase):
    def setUp(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0yNA"
        self.token = Auth().auth2leg()
        self.file_path = r".\output\svfs\Resource\3D View\08f99ae5-b8be-4f8d-881b-128675723c10\Project Completion\0.pf"

    def test_parse_mesh_from_file(self):
        mesh = Mesh.parse_mesh_from_file(self.file_path)
        self.assertNotEquals(len(mesh), 0)

    def test_read_mesh_from_urn(self):
        mesh = Mesh.read_mesh_from_urn(self.urn, self.token)
        self.assertNotEquals(len(mesh), 0)
