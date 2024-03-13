from .PathInfo import PathInfo


class ManifestItem:
    def __init__(self, guid, mime, path_info: [PathInfo]):
        self.guid = guid
        self.mime = mime
        self.path_info = path_info
