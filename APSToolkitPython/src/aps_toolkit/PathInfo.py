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


class PathInfo:
    def __init__(self, root_file_name: str = None, base_path: str = None, local_path: str = None, urn: str = None):
        self.root_file_name = root_file_name
        self.local_path = local_path
        self.base_path = base_path
        self.urn = urn
        self.files = []
