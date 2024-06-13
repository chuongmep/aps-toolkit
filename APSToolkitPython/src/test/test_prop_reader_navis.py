from unittest import TestCase
from .context import PropDbReaderNavis
from .context import Auth


class TestPropDbReaderNavis(TestCase):
    def setUp(self):
        self.token = Auth().auth2leg()
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLnFjbGtOUkpTU3VTR2hyRnJIR29kMUE_dmVyc2lvbj0x"
        self.prop_reader = PropDbReaderNavis(self.urn, self.token)

    def test_get_document_info(self):
        document_info = self.prop_reader.get_document_info()
        self.assertIsNotNone(document_info)

    def test_get_all_data(self):
        data = self.prop_reader.get_all_data()
        self.assertIsNotNone(data)

    def test_get_all_categories(self):
        categories = self.prop_reader.get_all_categories()
        self.assertIsNotNone(categories)

    def test_get_data_by_property(self):
        data = self.prop_reader.get_data_by_category("Item")
        self.assertIsNotNone(data)
