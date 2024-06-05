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
from .PackFileReader import PackFileReader
from .Derivative import Derivative
from .ManifestItem import ManifestItem
from .Token import Token


class Fragments:
    def __init__(self):
        self.visible = False
        self.materialID = 0
        self.geometryID = 0
        self.dbID = 0
        self.transform = None
        self.bbox = [0, 0, 0, 0, 0, 0]

    @staticmethod
    def parse_fragments_from_urn(urn: str, token: Token, region: str = "US") -> dict:
        """
        Parse fragments from urn
        :param urn: the urn of the model
        :param token: the token authentication
        :param region:  the region of hub (default is US)
        :return:  a dictionary of fragments with key is the guid of the manifest item and value is the list of fragments
        """
        fragments = {}
        derivative = Derivative(urn, token, region)
        manifest_items = derivative.read_svf_manifest_items()
        for manifest_item in manifest_items:
            resources = derivative.read_svf_resource_item(manifest_item)
            for resource in resources:
                if resource.local_path.endswith("FragmentList.pack"):
                    bytes_io = derivative.download_stream_resource(resource)
                    buffer = bytes_io.read()
                    frags = Fragments.parse_fragments(buffer)
                    fragments[manifest_item.guid] = frags
        return fragments

    @staticmethod
    def parse_fragments_from_file(file_path) -> list:
        """
        Parse fragments from file
        :param file_path:  FragmentList.pack
        :return:  a list of fragments
        """
        with open(file_path, "rb") as f:
            buffer = f.read()
            return Fragments.parse_fragments(buffer)

    @staticmethod
    def parse_fragments(buffer: bytes) -> list:
        """
        Parse fragments from buffer
        :param buffer:  the buffer of the fragment list
        :return:  a list of fragments
        """
        fragments = []
        pfr = PackFileReader(buffer)

        for i in range(pfr.num_entries()):
            entry_type = pfr.seek_entry(i)
            assert entry_type is not None
            assert entry_type.version > 4

            flags = pfr.get_uint8()
            visible = (flags & 0x01) != 0
            material_id = pfr.get_varint()
            geometry_id = pfr.get_varint()
            transform = pfr.get_transform()
            bbox = [0, 0, 0, 0, 0, 0]
            bbox_offset = [0, 0, 0]
            if entry_type.version > 3:
                if transform is not None and transform.t is not None:
                    bbox_offset[0] = float(transform.t[0])
                    bbox_offset[1] = float(transform.t[1])
                    bbox_offset[2] = float(transform.t[2])

            for j in range(6):
                bbox[j] = pfr.get_float32() + bbox_offset[j % 3]

            db_id = pfr.get_varint()

            fragment = Fragments()
            fragment.visible = visible
            fragment.materialID = material_id
            fragment.geometryID = geometry_id
            fragment.dbID = db_id
            fragment.transform = transform
            fragment.bbox = bbox
            fragments.append(fragment)

        return fragments
