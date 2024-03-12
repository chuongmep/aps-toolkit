from unittest import TestCase
from .context import Fragment


class TestFragment(TestCase):
    def setUp(self):
        pass

    def test_read_fragments(self):
        path = r"D:\API\Exyte\svf-viewer\svfs\3D View\08f99ae5-b8be-4f8d-881b-128675723c10\Project Completion\FragmentList.pack"
        # read bytes
        with open(path, 'rb') as f:
            buffer = f.read()
        # read fragments
        fragment = Fragment.Fragments.parse_fragments(buffer)
        self.assertNotEquals(len(fragment), 0)
