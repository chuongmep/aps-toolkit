from unittest import TestCase

from .context import SVFReader
from .context import Auth
import os


class TestSVFReader(TestCase):
    def setUp(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0yNA"
        self.token = Auth().auth2leg()
        self.reader = SVFReader(self.urn, self.token)

    def test_read_svf(self):
        resources = self.reader.read_sources()
        self.assertTrue(len(resources) > 0)

    def test_read_fragment(self):
        fragments = self.reader.read_fragments()
        self.assertTrue(len(fragments) > 0)

    def test_read_fragment_item(self):
        manifest_items = self.reader.read_svf_manifest_items()
        fragments = self.reader.read_fragments(manifest_items[0])
        self.assertTrue(len(fragments) > 0)

    def test_read_geometries(self):
        geometries = self.reader.read_geometries()
        self.assertTrue(len(geometries) > 0)

    def test_read_geometries_item(self):
        manifest_items = self.reader.read_svf_manifest_items()
        geometries = self.reader.read_geometries(manifest_items[0])
        self.assertTrue(len(geometries) > 0)

    def test_download_svf(self):
        folder = r"./output/svfs/"
        self.reader.download(folder)
        self.assertTrue(len(os.listdir(folder)) > 0)

    def test_read_svf_manifest_items(self):
        manifest_items = self.reader.read_svf_manifest_items()
        self.assertTrue(len(manifest_items) > 0)
