from os.path import join
import os
from .Derivative import Derivative


class SVFReader:
    def __init__(self, urn, token, region="US"):
        self.urn = urn
        self.token = token
        self.region = region
        self.derivative = Derivative(self.urn, self.token, self.region)

    def read_sources(self):
        resources = self.derivative.read_svf_resource()
        return resources
    def _read_contents(self):
        #TODO : 
        pass

    def download(self, output_dir):
        resources = self.read()
        for resource in resources:
            localpath = resource.local_path
            combined_path = join(output_dir, localpath)
            if not os.path.exists(os.path.dirname(combined_path)):
                os.makedirs(os.path.dirname(combined_path))
            self.derivative.download_resource(resource, combined_path)