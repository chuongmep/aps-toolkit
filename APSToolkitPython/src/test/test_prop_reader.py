from unittest import TestCase
import os
from .context import PropReader
from .context import Auth


class TestPropDbReader(TestCase):
    def test_enumerate_properties(self):
        token = Auth().auth2leg()
        urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLjAtYnBtcEpXUWJTRUVNdUFac1VETWc_dmVyc2lvbj0yNQ"
        prop_reader = PropReader(urn, token)
        properties = prop_reader.enumerate_properties(1)
        self.assertNotEquals(properties, 0)
