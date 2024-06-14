from unittest import TestCase
import os
from .context import PropReader
from .context import Auth


class TestPropDbReader(TestCase):
    def setUp(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLkRlZFNWdHhIUXlxS3YzY29vSjdxS1E_dmVyc2lvbj0x"
        self.token = Auth().auth2leg()
        self.prop_reader = PropReader(self.urn, self.token)

    def test_enumerate_properties(self):
        properties = self.prop_reader.enumerate_properties(14)
        self.assertNotEquals(properties, 0)

    def test_get_recursive_ids(self):
        ids = self.prop_reader.get_recursive_ids([14, 15])
        self.assertNotEquals(len(ids), 0)

    def test_get_all_properties_names(self):
        properties = self.prop_reader.get_all_properties_names()
        self.assertNotEquals(len(properties), 0)

    def test_get_properties(self):
        properties = self.prop_reader.get_properties(14)
        self.assertNotEquals(properties, 0)

    def test_get_all_properties(self):
        properties = self.prop_reader.get_all_properties(1)
        self.assertNotEquals(properties, 0)

    def test_get_entities_table(self):
        df = self.prop_reader.get_entities_table()
        self.assertNotEquals(df.empty, True)

    def test_get_values_table(self):
        df = self.prop_reader.get_values_table()
        self.assertNotEquals(df.empty, True)

    def test_get_attributes_table(self):
        df = self.prop_reader.get_attributes_table()
        self.assertNotEquals(df.empty, True)

    def test_get_avs_table(self):
        # TODO
        df = self.prop_reader.get_avs_table()
        self.assertNotEquals(df.empty, True)

    def test_get_offsets_table(self):
        df = self.prop_reader.get_offsets_table()
        self.assertNotEquals(df.empty, True)

    def test_get_property_values_by_names(self):
        values = self.prop_reader.get_property_values_by_names(["Comments", "name"])
        self.assertNotEquals(len(values), 0)

    def test_get_property_values_by_display_names(self):
        values = self.prop_reader.get_property_values_by_display_names(["Category", "Name"])
        self.assertNotEquals(len(values), 0)

    def test_get_instance(self):
        instance = self.prop_reader.get_instance(1)
        self.assertEquals(len(instance), 0)

    def test_get_children(self):
        children = self.prop_reader.get_children(1)
        self.assertNotEquals(len(children), 0)

    def test_get_parent(self):
        parent = self.prop_reader.get_parent(1)
        self.assertEquals(len(parent), 0)
