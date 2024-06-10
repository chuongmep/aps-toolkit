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
import struct
import gzip
from io import BytesIO
from .InputStream import InputStream
from .SVFTransform import SVFTransform
from .SVFManifestType import SVFManifestType


class PackFileReader(InputStream):
    def __init__(self, buffer):
        super().__init__(self.decompress_buffer(buffer))
        self.type = self.get_string(self.get_varint())
        self.version = self.get_int32()
        self.entries = []
        self.types = []
        self.parse_contents()

    @staticmethod
    def decompress_buffer(inputBuffer):
        if inputBuffer[0] == 31 and inputBuffer[1] == 139:
            with gzip.GzipFile(fileobj=BytesIO(inputBuffer), mode='rb') as compressedStream:
                return compressedStream.read()
        return inputBuffer

    def parse_contents(self):
        original_offset = self.offset
        self.seek(self.length - 8)
        entries_offset = self.get_uint32()
        types_offset = self.get_uint32()

        self.seek(entries_offset)
        entries_count = self.get_varint()
        self.entries = [self.get_uint32() for _ in range(entries_count)]

        self.seek(types_offset)
        types_count = self.get_varint()
        self.types = []
        for _ in range(types_count):
            type_class = self.get_string(self.get_varint())
            type_val = self.get_string(self.get_varint())
            version = self.get_varint()
            self.types.append(SVFManifestType())
            self.types[-1].type_class = type_class
            self.types[-1].type = type_val
            self.types[-1].version = version

        self.seek(original_offset)

    def num_entries(self):
        return len(self.entries)

    def seek_entry(self, i):
        if i >= len(self.entries):
            return None
        offset = self.entries[i]
        self.seek(offset)
        type_index = self.get_uint32()
        if type_index >= len(self.types):
            return None
        return self.types[type_index]

    def get_vector3d(self) -> tuple:
        return struct.unpack('<3d', self.buffer[self.offset:self.offset + 24])

    def get_quaternion(self):
        return struct.unpack('<4f', self.buffer[self.offset:self.offset + 16])

    def get_matrix3x3(self):
        return struct.unpack('<9d', self.buffer[self.offset:self.offset + 72])

    def get_transform(self):
        xform_type = self.get_uint8()
        if xform_type == 0:
            return SVFTransform(t=self.get_vector3d())
        elif xform_type == 1:
            q = self.get_quaternion()
            t = self.get_vector3d()
            s = (1.0, 1.0, 1.0)
            return SVFTransform(t=t, q=q, s=s)
        elif xform_type == 2:
            scale = self.get_float32()
            q = self.get_quaternion()
            t = self.get_vector3d()
            s = (scale, scale, scale)
            return SVFTransform(t=t, q=q, s=s)
        elif xform_type == 3:
            matrix = self.get_matrix3x3()
            t = self.get_vector3d()
            return SVFTransform(t=t, matrix=matrix)
        return None
