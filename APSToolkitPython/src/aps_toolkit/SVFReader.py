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
from .Fragments import Fragments
from .SVFGeometries import SVFGeometries
from .ManifestItem import ManifestItem
from .Resource import Resource
from .PropReader import PropReader
from .SVFContent import SVFContent
from .SVFMesh import SVFMesh
from .Materials import Materials
from .SVFMaterials import SVFMaterials
from .SVFImage import SVFImage
from .SVFMetadata import SVFMetadata


class SVFReader:
    def __init__(self, urn, token, region="US"):
        self.urn = urn
        self.token = token
        self.region = region
        self.derivative = Derivative(self.urn, self.token, self.region)

    def read_contents(self, manifest_item: [ManifestItem] = None) -> list[SVFContent]:
        contents = []
        if manifest_item:
            content = self._read_contents_manifest(manifest_item)
            contents.append(content)
        else:
            manifest_items = self.read_svf_manifest_items()
            for manifest_item in manifest_items:
                content = self._read_contents_manifest(manifest_item)
                contents.append(content)
        return contents

    def _read_contents_manifest(self, manifest_item: [ManifestItem] = None) -> SVFContent:
        content = SVFContent()
        content.fragments = self.read_fragments(manifest_item).items().__iter__().__next__()[1]
        content.geometries = self.read_geometries(manifest_item).items().__iter__().__next__()[1]
        content.properties = self.read_properties()
        content.meshpacks = self.read_meshes(manifest_item).items().__iter__().__next__()[1]
        content.materials = self.read_materials(manifest_item).items().__iter__().__next__()[1]
        content.images = self.read_images(manifest_item).items().__iter__().__next__()[1]
        content.metadata = self.read_meta_data(manifest_item).items().__iter__().__next__()[1]
        return content

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
            geos = SVFGeometries.parse_geos_from_manifest_item(self.derivative, manifest_item)
            geometries[manifest_item.guid] = geos
        else:
            geometries = SVFGeometries.parse_geometries_from_urn(self.urn, self.token, self.region)
        return geometries

    def read_meshes(self, manifest_item: [ManifestItem] = None) -> dict:
        meshes = {}
        if manifest_item:
            mesh = SVFMesh.parse_mesh_from_manifest_item(self.derivative, manifest_item)
            meshes[manifest_item.guid] = mesh
        else:
            meshes = SVFMesh.parse_mesh_from_urn(self.urn, self.token, self.region)
        return meshes

    def read_materials(self, manifest_item: [ManifestItem] = None) -> dict[str, list[Materials]]:
        materials = {}
        if manifest_item:
            mats = SVFMaterials.parse_materials_from_manifest_item(self.derivative, manifest_item)
            materials[manifest_item.guid] = mats
        else:
            materials = SVFMaterials.parse_materials_from_urn(self.urn, self.token, self.region)
        return materials

    def read_images(self, manifest_item: [ManifestItem] = None) -> dict[str, list[SVFImage]]:
        images = {}
        if manifest_item:
            imgs = SVFImage.parse_images_from_derivative(self.derivative, manifest_item)
            images[manifest_item.guid] = imgs
        else:
            images = SVFImage.parse_images_from_urn(self.urn, self.token, self.region)
        return images

    def read_meta_data(self, manifest_item: [ManifestItem] = None) -> dict:
        meta_datas = {}
        if manifest_item:
            meta_data = SVFMetadata.parse_metadata_from_derivative(self.derivative, manifest_item)
            meta_datas[manifest_item.guid] = meta_data
        else:
            meta_datas = SVFMetadata.parse_metadata_from_urn(self.urn, self.token, self.region)
        return meta_datas

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
