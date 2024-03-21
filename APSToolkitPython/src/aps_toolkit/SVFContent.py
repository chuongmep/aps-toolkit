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


class SVFContent:
    def __init__(self, metadata=None, fragments=None, geometries=None, meshpacks=None, materials=None, properties=None,
                 images=None):
        self.metadata = metadata
        self.fragments = fragments
        self.geometries = geometries
        self.meshpacks = meshpacks
        self.materials = materials
        self.properties = properties
        self.images = images
