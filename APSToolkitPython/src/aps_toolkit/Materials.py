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
from .MaterialProperties import MaterialProperties
from .SVFMaterialMap import SVFMaterialMap


class Materials:
    def __init__(self, diffuse=None, specular=None, ambient=None, emissive=None, glossiness=None, reflectivity=None,
                 opacity=None, metal=None, map: [SVFMaterialMap] = None, tag=None,
                 proteinType=None, definition=None, transparent=None, keywords=None, categories=None,
                 properties: [MaterialProperties] = None,
                 textures=None):
        self.diffuse = diffuse
        self.specular = specular
        self.ambient = ambient
        self.emissive = emissive
        self.glossiness = glossiness
        self.reflectivity = reflectivity
        self.opacity = opacity
        self.metal = metal
        self.map = map
        self.tag = tag
        self.proteinType = proteinType
        self.definition = definition
        self.transparent = transparent
        self.keywords = keywords
        self.categories = categories
        self.properties = properties
        self.textures = textures
