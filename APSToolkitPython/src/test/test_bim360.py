from unittest import TestCase
import os

from .context import BIM360

from .context import Auth


class TestBIM360(TestCase):
    def setUp(self):
        self.token = Auth().auth2leg()
        self.bim360 = BIM360(self.token)
        self.hub_id = "b.1715cf2b-cc12-46fd-9279-11bbc47e72f6"
        self.project_id = "b.1f7aa830-c6ef-48be-8a2d-bd554779e74b"
        self.folder_id = "urn:adsk.wipprod:fs.folder:co.5ufH-U8yRjaZ-USJxxN_Mw"
        self.item_id = "urn:adsk.wipprod:dm.lineage:DjXtlXoJQyS6D1R-gRhI8A"

    def test_get_hubs(self):
        hubs = self.bim360.get_hubs()
        self.assertNotEquals(hubs, 0)

    def test_get_projects(self):
        hubs = self.bim360.get_hubs()
        projects = self.bim360.get_projects(hubs['data'][0]['id'])
        self.assertNotEquals(projects, 0)

    def test_get_top_folders(self):
        hubs = self.bim360.get_hubs()
        projects = self.bim360.get_projects(hubs['data'][0]['id'])
        top_folders = self.bim360.get_top_folders(hubs['data'][0]['id'], projects['data'][0]['id'])
        self.assertNotEquals(top_folders, 0)

    def test_get_folder_contents(self):
        hubs = self.bim360.get_hubs()
        projects = self.bim360.get_projects(hubs['data'][0]['id'])
        top_folders = self.bim360.get_top_folders(hubs['data'][0]['id'], projects['data'][0]['id'])
        folder_contents = self.bim360.get_folder_contents(projects['data'][0]['id'], top_folders['data'][0]['id'])
        self.assertNotEquals(folder_contents, 0)
    def test_get_item_info(self):
        item_info = self.bim360.get_item_info(self.project_id, self.item_id)
        self.assertNotEquals(item_info, 0)
    def test_get_item_versions(self):
        items =self.bim360.batch_report_items(self.project_id, self.folder_id)
        self.assertNotEquals(items, 0)


    def test_batch_report_item_versions(self):
        df = self.bim360.batch_report_item_versions(self.project_id, self.item_id)
        self.assertNotEquals(df.empty, True)
    def test_get_urn_item_version(self):
        urn = self.bim360.get_urn_item_version(self.project_id, self.item_id,2)
        self.assertNotEquals(urn, "")

    def test_batch_report_projects(self):
        df = self.bim360.batch_report_projects(self.hub_id)
        self.assertNotEquals(df.empty, True)

    def test_get_item__display_name(self):
        item_name = self.bim360.get_item_display_name(self.project_id, self.item_id)
        self.assertNotEquals(item_name, "")