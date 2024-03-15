import struct

class InputStream:
    def __init__(self, buffer):
        self.buffer = buffer
        self.offset = 0
        self.length = len(buffer)

    def seek(self, offset):
        self.offset = offset

    def get_uint8(self):
        val = self.buffer[self.offset]
        self.offset += 1
        return val

    def get_uint16(self):
        val = struct.unpack_from('<H', self.buffer, self.offset)[0]
        self.offset += 2
        return val

    def get_int16(self):
        val = struct.unpack_from('<h', self.buffer, self.offset)[0]
        self.offset += 2
        return val

    def get_uint32(self):
        val = struct.unpack_from('<I', self.buffer, self.offset)[0]
        self.offset += 4
        return val

    def get_int32(self):
        val = struct.unpack_from('<i', self.buffer, self.offset)[0]
        self.offset += 4
        return val

    def get_float32(self):
        val = struct.unpack_from('<f', self.buffer, self.offset)[0]
        self.offset += 4
        return val

    def get_float64(self):
        val = struct.unpack_from('<d', self.buffer, self.offset)[0]
        self.offset += 8
        return val

    def get_varint(self):
        val = 0
        shift = 0
        byte = self.buffer[self.offset]
        self.offset += 1
        while byte & 0x80 != 0:
            val |= (byte & 0x7F) << shift
            shift += 7
            byte = self.buffer[self.offset]
            self.offset += 1
        val |= (byte & 0x7F) << shift
        return val

    def get_string(self, length):
        val = self.buffer[self.offset:self.offset + length].decode('utf-8')
        self.offset += length
        return val
