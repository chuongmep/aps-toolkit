from unittest import TestCase
import os
from .context import PropReader
from .context import Auth


class TestPropDbReader(TestCase):
    def setUp(self):
        pass
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLkotQ2laSHpGVEd5LUEwLVJmaEVVTVE_dmVyc2lvbj04"
        self.token = Auth().auth2leg()
        self.prop_reader = PropReader(self.urn, self.token)

    def test_read_from_svf(self):
        path = r"C:\Users\vho2\AppData\Local\Temp\output\output\Resource\3D View\{3D} 960621\{3D}.svf"
        prop = PropReader.read_from_resource(path)
        child = prop.get_properties(1)
        self.assertNotEqual(prop, 0)

    def test_read_from_resource(self):
        path = r"C:\Users\vho2\AppData\Local\Temp\output\output\Resource"
        prop = PropReader.read_from_resource(path)
        self.assertNotEqual(prop.ids, 0)
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
