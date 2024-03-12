from unittest import TestCase

from .context import SVFReader
from .context import Auth
import os

class TestSVFReader(TestCase):
    def setUp(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0yNA"
        self.token = Auth().auth2leg()

    def test_read_svf(self):
        reader = SVFReader(self.urn, self.token)
        resources = reader.read_sources()
        self.assertTrue(len(resources) > 0)
    def test_download_svf(self):
        reader = SVFReader(self.urn, self.token)
        folder = r"./data/svfs/"
        reader.download(folder)
        self.assertTrue(len(os.listdir(folder)) > 0)