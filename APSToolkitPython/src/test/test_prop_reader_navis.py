from unittest import TestCase
from .context import PropDbReaderNavis
from .context import Auth


class TestPropDbReaderNavis(TestCase):
    def setUp(self):
        self.token = Auth().auth2leg()
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk5Vd2lWVHljVDJ5N3RydVhSSGM0R3c_dmVyc2lvbj0x"
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

    def test_get_all_parameters(self):
        parameters = self.prop_reader.get_all_parameters()
        self.assertIsNotNone(parameters)

    def test_get_data_by_categories(self):
        df = self.prop_reader.get_data_by_categories(["Item", "Element"])
        self.assertIsNotNone(df)

    def test_get_all_sources_files(self):
        sources = self.prop_reader.get_all_sources_files()
        self.assertIsNotNone(sources)

    def test_get_all_data_by_resources(self):
        data = self.prop_reader.get_all_data_resources()
        self.assertIsNotNone(data)

    def test_get_data_by_resources_categories(self):
        data = self.prop_reader.get_data_resources_by_categories(["Element"])
        self.assertIsNotNone(data)

    def test_get_data_by_category(self):
        data = self.prop_reader.get_data_by_category("Element")
        self.assertIsNotNone(data)
