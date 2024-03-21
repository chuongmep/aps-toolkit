from unittest import TestCase
import os
from .context import PropDbReaderCad
from .context import Auth


class TestPropDbReader(TestCase):
    def setUp(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLm5KaEpjQkQ1UXd1bjlIV1ktNWViQmc_dmVyc2lvbj0x"
        self.token = Auth().auth2leg()
        self.prop_reader = PropDbReaderCad(self.urn, self.token)

    def test_get_document_info(self):
        document_info = self.prop_reader.get_document_info()
        self.assertIsNotNone(document_info)
