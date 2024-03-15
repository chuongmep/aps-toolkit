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
from .SVFUVMap import SVFUVMap
from .PackFileReader import PackFileReader
import math
from .SVFLines import SVFLines
from .SVFPoints import SVFPoints
from .Derivative import Derivative
import re
from .ManifestItem import ManifestItem


class SVFMesh:
    def __init__(self, v_count=None, t_count=None, uv_count=None, attrs=None, flags=None, comment=None,
                 uv_maps: [SVFUVMap] = None,
                 indices=None, vertices=None, normals=None, colors=None, min=None, max=None):
        self.v_count = v_count
        self.t_count = t_count
        self.uv_count = uv_count
        self.attrs = attrs
        self.flags = flags
        self.comment = comment
        self.uv_maps = uv_maps
        self.indices = indices
        self.vertices = vertices
        self.normals = normals
        self.colors = colors
        self.min = min
        self.max = max

    @staticmethod
    def parse_mesh_from_urn(urn, token, region="US") -> dict:
        """
        Reads mesh data from a specified URN (Uniform Resource Name) and returns a dictionary of mesh packs.

        Parameters:
        urn (str): The URN from which to read the mesh data.
        token (str): The token used for authentication.
        region (str, optional): The region where the data is stored. Defaults to "US".

        Returns:
        dict: A dictionary where the keys are GUIDs of manifest items and the values are lists of meshes associated with each manifest item.

        The function works by creating a `Derivative` object using the provided URN, token, and region. It then reads the manifest items from the `Derivative` object. For each manifest item, it reads the associated resources and filters them to include only those with a file name that matches the pattern `<number>.pf`. For each of these filtered resources, the function downloads the resource as a stream, reads the content into a buffer, and parses the buffer into a mesh using the `Mesh.parse_mesh` method. The parsed meshes are stored in a list. Finally, the function adds the list of parsed meshes to a dictionary, using the manifest item's GUID as the key. This dictionary is returned as the result of the function.
        """
        derivative = Derivative(urn, token, region)
        manifest_items = derivative.read_svf_manifest_items()
        mesh_packs = {}
        for manifest_item in manifest_items:
            meshes_manifest_item = SVFMesh.parse_mesh_from_manifest_item(derivative, manifest_item)
            mesh_packs[manifest_item.guid] = meshes_manifest_item
        return mesh_packs

    @staticmethod
    def parse_mesh_from_manifest_item(derivative:[Derivative], manifest_item: [ManifestItem]) -> list:
        svf_resources = derivative.read_svf_resource_item(manifest_item)
        # filter the resources have file extension .pf and name follow <number>.pf
        pattern = re.compile(r"^\d+\.pf$")
        file_packs = [resource for resource in svf_resources if pattern.match(resource.file_name)]
        meshes_manifest_item = []
        for file_pack in file_packs:
            bytes_io = derivative.download_stream_resource(file_pack)
            buffer = bytes_io.read()
            meshes = SVFMesh.parse_mesh(buffer)
            meshes_manifest_item.extend(meshes)
        return meshes_manifest_item
    @staticmethod
    def parse_mesh_from_file(file_path):
        with open(file_path, "rb") as file:
            buffer = file.read()
        return SVFMesh.parse_mesh(buffer)

    @staticmethod
    def parse_mesh(buffer):
        mesh_packs = []
        pfr = PackFileReader(buffer)
        for i in range(pfr.num_entries()):
            entry = pfr.seek_entry(i)
            assert entry is not None
            assert entry.version >= 1
            if entry.type == "Autodesk.CloudPlatform.OpenCTM":
                mesh_packs.append(SVFMesh.parse_mesh_octm(pfr))
            elif entry.type == "Autodesk.CloudPlatform.Lines":
                mesh_packs.append(SVFMesh.parse_lines(pfr, entry.version))
            elif entry.type == "Autodesk.CloudPlatform.Points":
                mesh_packs.append(SVFMesh.parse_points(pfr, entry.version))
        return mesh_packs

    @staticmethod
    def parse_mesh_octm(pfr: [PackFileReader]):
        fourcc = pfr.get_string(4)
        assert fourcc == "OCTM"

        version = pfr.get_int32()
        assert version == 5

        method = pfr.get_string(3)
        pfr.get_uint8()  # Read the last 0 char of the RAW or MG2 fourCC

        if method == "RAW":
            return SVFMesh.parse_mesh_raw(pfr)
        else:
            print("Unsupported OpenCTM method " + method)
            return None

    @staticmethod
    def parse_mesh_raw(pfr: [PackFileReader]):
        vcount = pfr.get_int32()  # Num of vertices
        tcount = pfr.get_int32()  # Num of triangles
        uvcount = pfr.get_int32()  # Num of UV maps
        attrs = pfr.get_int32()  # Number of custom attributes per vertex
        flags = pfr.get_int32()  # Additional flags (e.g., whether normals are present)
        comment = pfr.get_string(pfr.get_int32())

        # Indices
        name = pfr.get_string(4)
        assert name == "INDX"
        indices = [pfr.get_uint32() for _ in range(tcount * 3)]

        # Vertices
        name = pfr.get_string(4)
        assert name == "VERT"
        vertices = []
        min_values = [float('inf')] * 3
        max_values = [float('-inf')] * 3
        for _ in range(vcount):
            x, y, z = pfr.get_float32(), pfr.get_float32(), pfr.get_float32()
            min_values = [min(x, min_values[0]), min(y, min_values[1]), min(z, min_values[2])]
            max_values = [max(x, max_values[0]), max(y, max_values[1]), max(z, max_values[2])]
            vertices.extend([x, y, z])

        # Normals
        normals = None
        if flags & 1 != 0:
            name = pfr.get_string(4)
            assert name == "NORM"
            normals = []
            for _ in range(vcount):
                x, y, z = pfr.get_float32(), pfr.get_float32(), pfr.get_float32()
                # Make sure the normals have unit length
                dot = x * x + y * y + z * z
                if dot != 1.0:
                    length = math.sqrt(dot)
                    x /= length
                    y /= length
                    z /= length
                normals.extend([x, y, z])

        # Parse zero or more UV maps
        uvmaps = []
        for _ in range(uvcount):
            name = pfr.get_string(4)
            assert name == "TEXC"
            uvmap_name = pfr.get_string(pfr.get_int32())
            uvmap_file = pfr.get_string(pfr.get_int32())
            uvs = []
            for _ in range(vcount):
                u, v = pfr.get_float32(), 1.0 - pfr.get_float32()
                uvs.extend([u, v])
            uvmaps.append({"name": uvmap_name, "file": uvmap_file, "uvs": uvs})

        # Parse custom attributes (currently we only support "Color" attrs)
        colors = None
        if attrs > 0:
            name = pfr.get_string(4)
            assert name == "ATTR"
            for _ in range(attrs):
                attr_name = pfr.get_string(pfr.get_int32())
                if attr_name == "Color":
                    colors = [pfr.get_float32() for _ in range(vcount * 4)]
                else:
                    pfr.seek(pfr.offset + vcount * 4)

        mesh = SVFMesh(vcount, tcount, uvcount, attrs, flags, comment, uvmaps, indices, vertices, normals, colors,
                       min_values, max_values)
        if normals is not None:
            mesh.normals = normals
        if colors is not None:
            mesh.colors = colors

        return mesh

    @staticmethod
    def parse_lines(pfr: [PackFileReader], entry_version) -> SVFLines:
        assert entry_version >= 2

        vertex_count = pfr.get_uint16()
        index_count = pfr.get_uint16()
        bounds_count = pfr.get_uint16()  # Ignoring for now
        line_width = pfr.get_float32() if entry_version > 2 else 1.0
        has_colors = pfr.get_uint8() != 0
        is_line = True
        l_count = index_count // 2
        vertices = [pfr.get_float32() for _ in range(vertex_count * 3)]
        indices = [pfr.get_uint16() for _ in range(index_count)]
        lines = SVFLines(is_line, vertex_count, l_count, vertices, indices, None, line_width)
        # Parse colors
        if has_colors:
            lines.colors = [pfr.get_float32() for _ in range(vertex_count * 3)]

        # TODO: Parse polyline bounds

        return lines

    @staticmethod
    def parse_points(self, pfr: [PackFileReader], entry_version) -> SVFPoints:
        assert entry_version >= 2
        vertex_count = pfr.get_uint16()
        index_count = pfr.get_uint16()
        point_size = pfr.get_float32()
        has_colors = pfr.get_uint8() != 0
        vertices = [pfr.get_float32() for _ in range(vertex_count * 3)]
        points = SVFPoints(True, vertex_count, vertices, None, point_size)
        # Parse colors
        if has_colors:
            points.colors = [pfr.get_float32() for _ in range(vertex_count * 3)]
        return points
