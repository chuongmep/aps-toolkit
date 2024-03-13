from os.path import join
import os
from .Derivative import Derivative
from .Fragments import Fragments
from .Geometries import Geometries
from .ManifestItem import ManifestItem
from .Resource import Resource
from .PropReader import PropReader

class SVFReader:
    def __init__(self, urn, token, region="US"):
        self.urn = urn
        self.token = token
        self.region = region
        self.derivative = Derivative(self.urn, self.token, self.region)

    def _read_contents(self):
        # TODO :
        pass

    def read_sources(self) -> dict[str, Resource]:
        resources = self.derivative.read_svf_resource()
        return resources

    def read_svf_manifest_items(self):
        items = self.derivative.read_svf_manifest_items()
        return items

    def read_fragments(self, manifest_item: [ManifestItem] = None) -> dict:
        fragments = {}
        if manifest_item:
            resources = self.derivative.read_svf_resource_item(manifest_item)
            for resource in resources:
                if resource.local_path.endswith("FragmentList.pack"):
                    bytes_io = self.derivative.download_stream_resource(resource)
                    buffer = bytes_io.read()
                    frags = Fragments.parse_fragments(buffer)
                    fragments[manifest_item.guid] = frags
        else:
            fragments = Fragments.parse_fragments_from_urn(self.urn, self.token, self.region)
        return fragments

    def read_geometries(self, manifest_item: [ManifestItem] = None) -> dict:
        geometries = {}
        if manifest_item:
            geos = Geometries.parse_geos_from_manifest_item(self.derivative, manifest_item)
            geometries[manifest_item.guid] = geos
        else:
            geometries = Geometries.parse_geometries_from_urn(self.urn, self.token, self.region)
        return geometries

    def read_properties(self) -> PropReader:
        return PropReader(self.urn, self.token, self.region)

    def download(self, output_dir, manifest_item: [ManifestItem] = None):
        if manifest_item:
            resources = self.derivative.read_svf_resource_item(manifest_item)
            for resource in resources:
                localpath = resource.local_path
                combined_path = join(output_dir, localpath)
                if not os.path.exists(os.path.dirname(combined_path)):
                    os.makedirs(os.path.dirname(combined_path))
                self.derivative.download_resource(resource, combined_path)
        else:
            resources = self.read_sources()
            for _, items in resources.items():
                for source in items:
                    localpath = source.local_path
                    combined_path = join(output_dir, localpath)
                    if not os.path.exists(os.path.dirname(combined_path)):
                        os.makedirs(os.path.dirname(combined_path))
                    self.derivative.download_resource(source, combined_path)
