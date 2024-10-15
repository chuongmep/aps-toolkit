from unittest import TestCase
from .context import PropDbReaderRevit
from .context import Auth


class TestPropDbReaderRevit(TestCase):
    def setUp(self):
        self.token = Auth().auth2leg()
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0zMg"
        self.prop_reader = PropDbReaderRevit(self.urn, self.token)
        # pass

    def test_init_local(self):
        pro = PropDbReaderRevit.read_from_resource(
            r"C:\Users\vho2\AppData\Local\Temp\output\output\Resource\3D View\{3D} 960621\{3D}.svf")
        self.assertIsNotNone(pro)

    def test_get_document_info(self):
        document_info = self.prop_reader.get_document_info()
        self.assertIsNotNone(document_info)

    def test_get_documentId(self):
        documentId = self.prop_reader.get_document_id()
        self.assertIsNotNone(documentId)

    def test_get_levels(self):
        levels = self.prop_reader.get_levels()
        self.assertNotEquals(len(levels), 0)

    def test_get_grids(self):
        grids = self.prop_reader.get_grids()
        self.assertNotEquals(len(grids), 0)

    def test_get_phases(self):
        phases = self.prop_reader.get_phases()
        self.assertNotEquals(len(phases), 0)

    def test_get_all_categories(self):
        categories = self.prop_reader.get_all_categories()
        print(categories)
        self.assertNotEquals(categories, 0)

    def test_get_all_families(self):
        families = self.prop_reader.get_all_families()
        self.assertNotEquals(families, 0)

    def test_get_all_families_types(self):
        families_types = self.prop_reader.get_all_families_types()
        self.assertNotEquals(families_types, 0)

    def test_get_categories_families_types(self):
        categories_families_types = self.prop_reader.get_categories_families_types()
        self.assertNotEquals(categories_families_types, 0)

    def test_get_cats_fams_types_params(self):
        categories_families_types_params = self.prop_reader.get_cats_fams_types_params()
        self.assertNotEquals(categories_families_types_params, 0)

    def test_get_all_data(self):
        data = self.prop_reader.get_all_data(display_unit=True)
        self.assertIsNotNone(data)

    def test_get_bounding_boxs(self):
        bounding_boxes = self.prop_reader.get_all_bounding_boxs()
        self.assertNotEquals(len(bounding_boxes), 0)

    def test_get_data_by_category(self):
        df = self.prop_reader.get_data_by_category("Furniture", True, True, True)
        # check if dataframe have rows = 1
        df_rows = df.shape[0]
        self.assertNotEquals(df_rows, 0)

    def test_get_data_by_categories(self):
        df = self.prop_reader.get_data_by_categories(["Doors", "Windows"], is_add_family_name=True)
        self.assertNotEquals(df.empty, True)

    # noinspection PyInterpreter
    def test_get_data_by_family(self):
        family_name = "Seating-LAMMHULTS-PENNE-Chair"
        df = self.prop_reader.get_data_by_family(family_name)
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_families(self):
        family_names = ["Seating-LAMMHULTS-PENNE-Chair", "Sheet"]
        df = self.prop_reader.get_data_by_families(family_names)
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_family_type(self):
        family_type = "Plastic-Seat"
        df = self.prop_reader.get_data_by_family_type(family_type)
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_family_types(self):
        family_types = ["CL_W1"]
        df = self.prop_reader.get_data_by_family_types(family_types)
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_categories_and_params(self):
        categories = ["Doors"]
        params = ["ElementId", "Name", "Category", "CategoryId", "Level", "Workset", "Area Type", "Number", "Area",
                  "Perimeter"]
        df = self.prop_reader.get_data_by_categories_and_params(categories, params, True, display_unit=False)
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_parameters(self):
        df = self.prop_reader.get_data_by_parameters(["Name", "Category", "ElementId", "Width", "Height",
                                                      "IfcGUID", "Family Name"], True)
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_external_id(self):
        external_id = "652ae298-920d-4c0c-a25b-0f9dc79857d7-000fafee"
        df = self.prop_reader.get_data_by_external_id(external_id, True)
        self.assertNotEquals(df.empty, True)

    def test_get_data_by_element_id(self):
        element_id = 289790
        parameters = self.prop_reader.get_data_by_element_id(element_id)
        self.assertIsNotNone(parameters)
        self.assertNotEquals(len(parameters), 0)

    def test_get_all_parametes(self):
        parameters = self.prop_reader.get_all_parameters()
        self.assertNotEquals(parameters, 0)

    def test_get_units_mapping(self):
        units = self.prop_reader.get_units_mapping()
        self.assertNotEquals(units, 0)