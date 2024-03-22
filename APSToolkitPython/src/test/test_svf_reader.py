from unittest import TestCase

from .context import SVFReader
from .context import Auth
import os


class TestSVFReader(TestCase):
    def setUp(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLm5KaEpjQkQ1UXd1bjlIV1ktNWViQmc_dmVyc2lvbj0x"
        self.token = Auth().auth2leg()
        self.reader = SVFReader(self.urn, self.token)

    def test_read_contents(self):
        contents = self.reader.read_contents()
        self.assertTrue(len(contents) > 0)

    def test_read_contents_manifest(self):
        manifest_items = self.reader.read_svf_manifest_items()
        contents = self.reader.read_contents(manifest_items[0])
        self.assertTrue(len(contents) > 0)

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

    def test_read_meshes(self):
        meshes = self.reader.read_meshes()
        self.assertTrue(len(meshes) > 0)

    def test_read_meshes_item(self):
        manifest_items = self.reader.read_svf_manifest_items()
        meshes = self.reader.read_meshes(manifest_items[0])
        self.assertTrue(len(meshes) > 0)

    def test_read_properties(self):
        prop_reader = self.reader.read_properties()
        self.assertTrue(len(prop_reader.attrs) > 0)
        self.assertTrue(len(prop_reader.vals) > 0)

    def test_read_materials(self):
        materials = self.reader.read_materials()
        self.assertTrue(len(materials) > 0)

    def test_read_images(self):
        images = self.reader.read_images()
        self.assertTrue(len(images) > 0)

    def test_read_meta_data(self):
        meta_data = self.reader.read_meta_data()
        self.assertTrue(len(meta_data) > 0)

    def test_read_materials_item(self):
        manifest_items = self.reader.read_svf_manifest_items()
        materials = self.reader.read_materials(manifest_items[0])
        self.assertTrue(len(materials) > 0)

    def test_download_svf(self):
        folder = r"./output/svfs/"
        self.reader.download(folder)
        self.assertTrue(len(os.listdir(folder)) > 0)

    def test_read_svf_manifest_items(self):
        manifest_items = self.reader.read_svf_manifest_items()
        self.assertTrue(len(manifest_items) > 0)
