from unittest import TestCase
from .context import Mesh


class TestMesh(TestCase):
    def setUp(self):
        self.file_path = r".\output\svfs\Resource\3D View\08f99ae5-b8be-4f8d-881b-128675723c10\Project Completion\0.pf"

    def test_parse_mesh_from_file(self):
        mesh = Mesh.parse_mesh_from_file(self.file_path)
        self.assertNotEquals(len(mesh), 0)
