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
