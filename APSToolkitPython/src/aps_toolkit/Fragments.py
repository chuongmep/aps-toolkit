from .PackFileReader import PackFileReader
from .Derivative import Derivative

class Fragments:
    def __init__(self):
        self.visible = False
        self.materialID = 0
        self.geometryID = 0
        self.dbID = 0
        self.transform = None
        self.bbox = [0, 0, 0, 0, 0, 0]

    @staticmethod
    def parse_fragments_from_urn(urn,token, region="US"):
        fragments = []
        derivative = Derivative(urn, token, region)
        resources = derivative.read_svf_resource()
        for resource in resources:
            if resource.local_path.endswith("FragmentList.pack"):
                bytes_io = derivative.download_stream_resource(resource)
                buffer = bytes_io.read()
                frags = Fragments.parse_fragments(buffer)
                fragments.extend(frags)
        return fragments


    @staticmethod
    def parse_fragments(buffer: bytes):
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
                    bbox_offset[0] = transform.t[0]
                    bbox_offset[1] = transform.t[1]
                    bbox_offset[2] = transform.t[2]

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
