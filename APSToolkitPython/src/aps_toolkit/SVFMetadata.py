from .Derivative import Derivative
from .ManifestItem import ManifestItem


class SVFMetadata:
    def __init__(self, version, metadata):
        self.version = version
        self.metadata = metadata

    @staticmethod
    def parse_metadata_from_urn(urn, token, region="US") -> dict:
        meta_datas = {}
        derivative = Derivative(urn, token, region)
        manifest_items = derivative.read_svf_manifest_items()
        for manifest_item in manifest_items:
            meta_data = SVFMetadata.parse_metadata_from_derivative(derivative, manifest_item)
            meta_datas[manifest_item.guid] = meta_data
        return meta_datas

    @staticmethod
    def parse_metadata_from_derivative(derivative: [Derivative], manifest_item: [ManifestItem]) -> str:
        metadata = derivative.read_svf_metadata(manifest_item.urn)
        return metadata
