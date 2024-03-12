import struct
import gzip
from io import BytesIO
from .InputStream import InputStream
from .SvfTransform import SvfTransform
from .SvfManifestType import SvfManifestType


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
            self.types.append(SvfManifestType())
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

    def get_vector3d(self):
        return struct.unpack('<3d', self.buffer[self.offset:self.offset + 24])

    def get_quaternion(self):
        return struct.unpack('<4f', self.buffer[self.offset:self.offset + 16])

    def get_matrix3x3(self):
        return struct.unpack('<9d', self.buffer[self.offset:self.offset + 72])

    def get_transform(self):
        xform_type = self.get_uint8()
        if xform_type == 0:
            return SvfTransform(t=self.get_vector3d())
        elif xform_type == 1:
            q = self.get_quaternion()
            t = self.get_vector3d()
            s = (1.0, 1.0, 1.0)
            return SvfTransform(t=t, q=q, s=s)
        elif xform_type == 2:
            scale = self.get_float32()
            q = self.get_quaternion()
            t = self.get_vector3d()
            s = (scale, scale, scale)
            return SvfTransform(t=t, q=q, s=s)
        elif xform_type == 3:
            matrix = self.get_matrix3x3()
            t = self.get_vector3d()
            return SvfTransform(t=t, matrix=matrix)
        return None
