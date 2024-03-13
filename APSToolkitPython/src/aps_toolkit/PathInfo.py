class PathInfo:
    def __init__(self, root_file_name=None, base_path=None, local_path=None, urn=None):
        self.root_file_name = root_file_name
        self.local_path = local_path
        self.base_path = base_path
        self.urn = urn
        self.files = []