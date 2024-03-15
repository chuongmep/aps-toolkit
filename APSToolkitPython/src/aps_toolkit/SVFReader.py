"""
Copyright (C) 2024  chuongmep.com

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
"""
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

    def read_svf_manifest_items(self):
        items = self.derivative.read_svf_manifest_items()
        return items

    def _read_contents(self):
        # TODO :
        pass

    def download(self, output_dir):
        resources = self.read_sources()
        for resource in resources:
            localpath = resource.local_path
            combined_path = join(output_dir, localpath)
            if not os.path.exists(os.path.dirname(combined_path)):
                os.makedirs(os.path.dirname(combined_path))
            self.derivative.download_resource(resource, combined_path)
