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
