from unittest import TestCase
from .context import Auth
from .context import SVFGeometries


class TestGeometries(TestCase):
    def setUp(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0yNA"
        self.token = Auth().auth2leg()
        self.file_path = (r".\output\svfs\Resource\3D View\08f99ae5-b8be-4f8d-881b-128675723c10\Project "
                          r"Completion\GeometryMetadata.pf")

    def test_parse_geometries(self):
        with open(self.file_path, 'rb') as f:
            buffer = f.read()
        geometries = SVFGeometries.parse_geometries(buffer)
        self.assertNotEquals(len(geometries), 0)

    def test_parse_geometries_from_urn(self):
        geometries = SVFGeometries.parse_geometries_from_urn(self.urn, self.token)
        self.assertNotEquals(len(geometries), 0)
