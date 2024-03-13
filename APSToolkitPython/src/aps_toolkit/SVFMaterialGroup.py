from .Materials import Materials


class SVFMaterialGroup:
    def __init__(self, version, userassets, materials: [Materials]):
        self.version = version
        self.userassets = userassets
        self.materials = materials
