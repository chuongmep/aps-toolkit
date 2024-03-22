from unittest import TestCase

from .context import Derivative
from .context import SVFMetadata
from .context import Auth


class TestSVFMetadata(TestCase):
    def setUp(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0yNA"
        self.token = Auth().auth2leg()

    def test_parse_metadata_from_urn(self):
        meta_datas = SVFMetadata.parse_metadata_from_urn(self.urn, self.token)
        self.assertNotEquals(len(meta_datas), 0)
