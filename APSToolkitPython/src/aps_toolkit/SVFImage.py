from .Derivative import Derivative
from .ManifestItem import ManifestItem


class SVFImage:
    def __init__(self, name, data: [bytes]):
        self.name = name
        self.data = data

    @staticmethod
    def parse_images_from_urn(urn, token, region="US") -> dict:
        images = {}
        derivative = Derivative(urn, token, region)
        manifest_items = derivative.read_svf_manifest_items()
        for manifest_item in manifest_items:
            list_bytes_images = SVFImage.parse_images_from_derivative(derivative, manifest_item)
            images[manifest_item.guid] = list_bytes_images
        return images

    @staticmethod
    def parse_images_from_derivative(derivative: [Derivative], manifest_item: [ManifestItem]) -> list:
        images = []
        resources = derivative.read_svf_resource_item(manifest_item)
        for resource in resources:
            if resource.local_path.endswith(".png"):
                bytes_io = derivative.download_stream_resource(resource)
                buffer = bytes_io.read()
                bytes = SVFImage(resource.file_name, buffer)
                images.append(bytes)
        return images
