from unittest import TestCase
from .context import PropDbReaderNavis
from .context import Auth


class TestPropDbReaderNavis(TestCase):
    def setUp(self):
        self.token = Auth().auth2leg()
        self.urn = "dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6Y2h1b25nX2J1Y2tldC9NeUhvdXNlLm53Yw"
        self.prop_reader = PropDbReaderNavis(self.urn, self.token)

    def test_get_document_info(self):
        document_info = self.prop_reader.get_document_info()
        self.assertIsNotNone(document_info)

    def test_get_all_categories(self):
        categories = self.prop_reader.get_all_categories()
        self.assertIsNotNone(categories)

    def test_get_data_by_property(self):
        data = self.prop_reader.get_data_by_category("Item")
        self.assertIsNotNone(data)
