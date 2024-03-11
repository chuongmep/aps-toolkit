from unittest import TestCase
import os
from .context import PropDbReaderRevit
from .context import Auth


class TestPropDbReaderRevit(TestCase):
    def setUp(self):
        self.token = Auth().auth2leg()
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0yNA"
        self.prop_reader = PropDbReaderRevit(self.urn, self.token)

    def test_get_document_info(self):
        document_info = self.prop_reader.get_document_info()
        self.assertIsNotNone(document_info)

    def test_get_documentId(self):
        documentId = self.prop_reader.get_document_id()
        self.assertIsNotNone(documentId)

    def test_get_levels(self):
        levels = self.prop_reader.get_levels()
        self.assertNotEquals(len(levels), 0)

    def test_get_grids(self):
        grids = self.prop_reader.get_grids()
        self.assertNotEquals(len(grids), 0)

    def test_get_phases(self):
        phases = self.prop_reader.get_phases()
        self.assertNotEquals(len(phases), 0)

    def test_get_all_categories(self):
        categories = self.prop_reader.get_all_categories()
        print(categories)
        self.assertNotEquals(categories, 0)

    def test_get_all_families(self):
        families = self.prop_reader.get_all_families()
        self.assertNotEquals(families, 0)

    def test_get_all_families_types(self):
        families_types = self.prop_reader.get_all_families_types()
        self.assertNotEquals(families_types, 0)

    def test_get_all_data(self):
        data = self.prop_reader.get_all_data()
        self.assertIsNotNone(data)

    def test_get_data_by_category(self):
        df = self.prop_reader.get_data_by_category("Windows", True)
        # check if dataframe have rows = 1
        df_rows = df.shape[0]
        self.assertNotEquals(df_rows, 0)

    def test_get_data_by_categories(self):
        df = self.prop_reader.get_data_by_categories(["Doors", "Windows"])
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_family(self):
        family_name = "Seating-LAMMHULTS-PENNE-Chair"
        df = self.prop_reader.get_data_by_family(family_name)
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_family_type(self):
        family_type = "Plastic-Seat"
        df = self.prop_reader.get_data_by_family_type(family_type)
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_categories_and_params(self):
        df = self.prop_reader.get_data_by_categories_and_params(["Doors", "Windows"],
                                                                ["name", "Category", "ElementId", "Width", "Height",
                                                                 "IfcGUID"], True)
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_external_id(self):
        external_id = "6d22740f-4d3f-4cc6-a442-8c98ddd54f1f-0004923b"
        df = self.prop_reader.get_data_by_external_id(external_id, True)
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_element_id(self):
        element_id = 289790
        parameters = self.prop_reader.get_data_by_element_id(element_id)
        self.assertIsNotNone(parameters)
        self.assertNotEquals(len(parameters), 0)

    def test_get_all_parametes(self):
        parameters = self.prop_reader.get_all_parameters()
        self.assertNotEquals(parameters, 0)
