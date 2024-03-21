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

    def test_get_all_layers(self):
        layers = self.prop_reader.get_all_layers()
        self.assertNotEquals(len(layers), 0)

    def test_get_all_categories(self):
        categories = self.prop_reader.get_all_categories()
        self.assertNotEquals(len(categories), 0)

    def test_get_all_data(self):
        data = self.prop_reader.get_all_data()
        self.assertIsNotNone(data)

    def test_get_data_by_category(self):
        df = self.prop_reader.get_data_by_category("Lines")
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_categories(self):
        df = self.prop_reader.get_data_by_categories(["Lines", "Circles"])
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_categories_and_params(self):
        df = self.prop_reader.get_data_by_categories_and_params(["Lines", "Circles"], ["Name", "Layer", "Color", "type"])
        self.assertNotEquals(df.empty, True)
