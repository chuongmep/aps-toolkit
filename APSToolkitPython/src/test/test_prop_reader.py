from unittest import TestCase
import os
from .context import PropReader
from .context import Auth


class TestPropDbReader(TestCase):
    def setUp(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0yNA"
        self.token = Auth().auth2leg()
        self.prop_reader = PropReader(self.urn, self.token)

    def test_enumerate_properties(self):
        properties = self.prop_reader.enumerate_properties(1)
        self.assertNotEquals(properties, 0)

    def test_get_properties(self):
        properties = self.prop_reader.get_properties(1)
        self.assertNotEquals(properties, 0)

    def test_get_all_properties(self):
        properties = self.prop_reader.get_all_properties(1)
        self.assertNotEquals(properties, 0)

    def test_get_instance(self):
        instance = self.prop_reader.get_instance(1)
        self.assertEquals(len(instance), 0)

    def test_get_children(self):
        children = self.prop_reader.get_children(1)
        self.assertNotEquals(len(children), 0)

    def test_get_parent(self):
        parent = self.prop_reader.get_parent(1)
        self.assertEquals(len(parent), 0)
