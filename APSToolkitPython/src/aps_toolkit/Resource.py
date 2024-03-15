from urllib.parse import urljoin, quote


class Resource:
    def __init__(self, file_name, remote_path, local_path):
        self.host = "https://developer.api.autodesk.com"
        self.file_name = file_name
        self.remote_path = self._resolve_path_slashes(remote_path)
        self.url = self._resolve_url(remote_path)
        self.local_path = self._resolve_path_slashes(local_path)

    def _resolve_path_slashes(self, path):
        url_with_forward_slashes = path.replace('\\', '/')
        return url_with_forward_slashes

    def _resolve_url(self, remote_path):
        url_with_forward_slashes = remote_path.replace('\\', '/')
        return urljoin(self.host, quote(url_with_forward_slashes, safe=':/'))
