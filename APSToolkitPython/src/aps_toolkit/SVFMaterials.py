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
from .SVFMaterialMapScale import SVFMaterialMapScale
from .SVFMaterialGroup import SVFMaterialGroup
from .Materials import Materials
import json
from .PackFileReader import PackFileReader


class SVFMaterials:
    def __init__(self):
        pass

    @staticmethod
    def parse_materials_from_file(file_path) -> list[Materials]:
        with open(file_path, "rb") as file:
            buffer = file.read()
            return SVFMaterials.parse_materials(buffer)

    @staticmethod
    def parse_materials(buffer) -> list[Materials]:
        materials = []

        # Decompress buffer (assuming it's already implemented)
        buffer = PackFileReader.decompress_buffer(buffer)

        if len(buffer) > 0:
            # Decode buffer to string assuming default encoding
            json_str = buffer.decode()

            # Deserialize JSON string to Materials object
            svf_mat = json.loads(json_str)
            for key, group in svf_mat['materials'].items():
                group_materials = SVFMaterialGroup(**group)
                material = group_materials.materials[group_materials.userassets[0]]
                material = Materials(**material)
                if str(material.definition) == 'SimplePhong':
                    materials.append(SVFMaterials.parse_simple_phong_material(group_materials))
                else:
                    print("Unsupported material definition:", material['definition'])

        return materials

    @staticmethod
    def parse_simple_phong_material(group: [SVFMaterialGroup]):
        result = Materials()
        material = Materials(**group.materials[group.userassets[0]])

        result.diffuse = SVFMaterials.parse_color_property(material, "generic_diffuse", [0, 0, 0, 1])
        result.specular = SVFMaterials.parse_color_property(material, "generic_specular", [0, 0, 0, 1])
        result.ambient = SVFMaterials.parse_color_property(material, "generic_ambient", [0, 0, 0, 1])
        result.emissive = SVFMaterials.parse_color_property(material, "generic_emissive", [0, 0, 0, 1])

        result.glossiness = SVFMaterials.parse_scalar_property(material, "generic_glossiness", 30)
        result.reflectivity = SVFMaterials.parse_scalar_property(material, "generic_reflectivity_at_0deg", 0)
        result.opacity = 1.0 - SVFMaterials.parse_scalar_property(material, "generic_transparency", 0)

        result.metal = SVFMaterials.parse_boolean_property(material, "generic_is_metal", False)

        if material.textures:
            maps = SVFMaterialMap()
            diffuse = SVFMaterials.parse_texture_property(material, group, "generic_diffuse")
            if diffuse:
                maps.diffuse = diffuse

            specular = SVFMaterials.parse_texture_property(material, group, "generic_specular")
            if specular:
                maps.specular = specular

            alpha = SVFMaterials.parse_texture_property(material, group, "generic_alpha")
            if alpha:
                maps.alpha = alpha

            bump = SVFMaterials.parse_texture_property(material, group, "generic_bump")
            if bump:
                if SVFMaterials.parse_boolean_property(material, "generic_bump_is_normal", False):
                    maps.normal = bump
                else:
                    maps.bump = bump

            result.maps = maps

        return result

    @staticmethod
    def parse_boolean_property(material: [Materials], prop, default_value) -> bool:
        if material.properties.booleans is not None and prop in material.properties.booleans:
            return material.properties.booleans[prop]
        else:
            return default_value

    @staticmethod
    def parse_scalar_property(material: [Materials], prop, default_value) -> float:
        if material.properties.scalars is not None and prop in material.properties.scalars:
            return material.properties.scalars[prop]["values"][0]
        else:
            return default_value

    @staticmethod
    def parse_color_property(material: [Materials], prop, default_value) -> [float]:
        if material.properties.colors is not None and prop in material.properties.colors:
            color = material.properties.colors[prop]["values"][0]
            return [color["r"], color["g"], color["b"], color["a"]]
        else:
            return default_value

    @staticmethod
    def parse_texture_property(material: [Materials], group: [SVFMaterialGroup], prop: str) -> SVFMaterialMap:
        if material.textures is not None and prop in material.textures:
            connection = material.textures[prop]["connections"][0]
            texture = group.materials[connection]
            if "unifiedbitmap_Bitmap" in texture.properties.uris:
                uri = texture.properties.uris["unifiedbitmap_Bitmap"]["values"][0]
                texture_uscale, texture_vscale = 0.0, 0.0

                if texture.properties.scalars is not None \
                        and "texture_UScale" in texture.properties.scalars \
                        and "texture_VScale" in texture.properties.scalars:
                    texture_uscale = texture.properties.scalars["texture_UScale"]["values"][0]
                    texture_vscale = texture.properties.scalars["texture_VScale"]["values"][0]

                if uri is not None:
                    return SVFMaterialMap(uri=uri, scale=SVFMaterialMapScale(texture_uscale, texture_vscale))
        return None
