from unittest import TestCase
from .context import Fragments
from .context import Auth


class TestFragment(TestCase):
    def setUp(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0yNA"
        self.token = Auth().auth2leg()
        self.file_path = (r".\output\svfs\Resource\3D View\08f99ae5-b8be-4f8d-881b-128675723c10\Project "
                            r"Completion\FragmentList.pack")

    def test_parse_fragments(self):
        with open(self.file_path, 'rb') as f:
            buffer = f.read()
        fragment = Fragments.parse_fragments(buffer)
        self.assertNotEquals(len(fragment), 0)

    def test_parse_fragments_from_file(self):
        fragment = Fragments.parse_fragments_from_file(self.file_path)
        self.assertNotEquals(len(fragment), 0)

    def test_parse_fragments_from_urn(self):
        fragment = Fragments.parse_fragments_from_urn(self.urn, self.token)
        self.assertNotEquals(len(fragment), 0)
