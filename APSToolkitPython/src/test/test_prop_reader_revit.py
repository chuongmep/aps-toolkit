from unittest import TestCase
import os
from .context import PropDbReaderRevit
from .context import Auth


class TestPropDbReaderRevit(TestCase):
    def setUp(self):
        self.token = Auth().auth2leg()
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0yNA"
        self.prop_reader = PropDbReaderRevit(self.urn, self.token)

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

    def test_get_data_by_category(self):
        df = self.prop_reader.get_data_by_category("Windows")
        # check if dataframe have rows = 1
        df_rows = df.shape[0]
        self.countTestCases(df_rows, 1)

    def test_get_data_by_categories(self):
        df = self.prop_reader.get_data_by_categories(["Ducts", "Rooms"])
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_family(self):
        family_name = "ex_M_MAU_02"
        df = self.prop_reader.get_data_by_family(family_name)
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_family_type(self):
        family_type = "457x191x67UB"
        df = self.prop_reader.get_data_by_family_type(family_type)
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_categories_and_params(self):
        df = self.prop_reader.get_data_by_categories_and_params(["Ducts"],
                                                                ["name", "Category", "System Name", "IfcGUID"])
        self.assertNotEquals(df.empty, True)
