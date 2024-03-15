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
from .Derivative import Derivative
from .PackFileReader import PackFileReader
from .ManifestItem import ManifestItem


class SVFGeometries:
    def __init__(self, frag_type=None, prim_count=None, pack_id=None, entity_id=None, topo_id=None):
        self.fragment_type = frag_type
        self.primitive_count = prim_count
        self.pack_id = pack_id
        self.entity_id = entity_id
        self.topo_id = topo_id

    @staticmethod
    def parse_geometries_from_urn(urn, token, region="US") -> dict:
        """
        Parse geometries from urn
        :param urn: the urn of the model
        :param token: the token authentication
        :param region:  the region of hub (default is US)
        :return:  a dictionary of geometries with key is the guid of the manifest item and value is the list of geometries
        """
        geometries = {}
        derivative = Derivative(urn, token, region)
        manifest_items = derivative.read_svf_manifest_items()
        for manifest_item in manifest_items:
            geos = SVFGeometries.parse_geos_from_manifest_item(derivative, manifest_item)
            geometries[manifest_item.guid] = geos
        return geometries

    @staticmethod
    def parse_geos_from_manifest_item(derivative: [Derivative], manifest_item: [ManifestItem]) -> list:
        geometries = []
        resources = derivative.read_svf_resource_item(manifest_item)
        for resource in resources:
            if resource.local_path.endswith("GeometryMetadata.pf"):
                bytes_io = derivative.download_stream_resource(resource)
                buffer = bytes_io.read()
                geos = SVFGeometries.parse_geometries(buffer)
                geometries.extend(geos)
        return geometries

    @staticmethod
    def parse_geometries(buffer) -> list:
        """
        Parse geometries from buffer
        :param buffer:  the buffer of the geometry metadata file
        :return:  a list of geometries
        """
        geometries = []
        pfr = PackFileReader(buffer)
        for i in range(pfr.num_entries()):
            entry = pfr.seek_entry(i)
            if entry is not None and entry.version >= 3:
                frag_type = pfr.get_uint8()
                # Skip past object space bbox -- we don't use that
                pfr.seek(pfr.offset + 24)
                prim_count = pfr.get_uint16()
                p_id = pfr.get_string(pfr.get_varint()).replace(".pf", "")
                pack_id = int(p_id)
                entity_id = pfr.get_varint()
                geometry = SVFGeometries(frag_type, prim_count, pack_id, entity_id)
                geometries.append(geometry)

        return geometries
