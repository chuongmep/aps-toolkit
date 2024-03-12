from unittest import TestCase

from .context import Derivative
from .context import  Auth

class TestDerivative(TestCase):
    def setUp(self):
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0yNA"
        self.token = Auth().auth2leg()

    def test_read_svf(self):
        derivative = Derivative.Derivative(self.urn, self.token)
        svf_urns = derivative.ReadSvf()
        self.assertNotEquals(len(svf_urns), 0)
