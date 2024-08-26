from unittest import TestCase
import os

from .context import BIM360

from .context import Auth


class TestBIM360(TestCase):
    def setUp(self):
        self.token = Auth().auth2leg()
        self.bim360 = BIM360(self.token)
        self.hub_id = "b.1715cf2b-cc12-46fd-9279-11bbc47e72f6"
        self.project_id = "b.ca790fb5-141d-4ad5-b411-0461af2e9748"
        self.folder_id = "urn:adsk.wipprod:fs.folder:co.uX9MsdjjSraK_3p5qXyE_A"
        self.item_id = "urn:adsk.wipprod:dm.lineage:wGXA2ljoSQaXtGOEepawIg"

    def test_parse_url(self):
        url = "https://acc.autodesk.com/docs/files/projects/ca790fb5-141d-4ad5-b411-0461af2e9748?folderUrn=urn%3Aadsk.wipprod%3Afs.folder%3Aco.uX9MsdjjSraK_3p5qXyE_A&entityId=urn%3Aadsk.wipprod%3Adm.lineage%3AwGXA2ljoSQaXtGOEepawIg&viewModel=detail&moduleId=folders"
        result = self.bim360.parse_url(url)
        # check projectid
        self.assertEqual(result['project_id'], "b." + "ca790fb5-141d-4ad5-b411-0461af2e9748")
        self.assertEqual(result['folder_urn'], "urn:adsk.wipprod:fs.folder:co.uX9MsdjjSraK_3p5qXyE_A")
        self.assertEqual(result['entity_id'], "urn:adsk.wipprod:dm.lineage:wGXA2ljoSQaXtGOEepawIg")


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

    def test_get_top_folder_project_files(self):
        project_files = self.bim360.get_top_folder_project_files(self.hub_id, self.project_id)
        self.assertNotEquals(project_files, 0)

    def test_get_item_info(self):
        item_info = self.bim360.get_item_info(self.project_id, self.item_id)
        self.assertNotEquals(item_info, 0)

    def test_get_latest_derivative_urn(self):
        urn = self.bim360.get_latest_derivative_urn(self.project_id, self.item_id)
        self.assertNotEquals(urn, "")

    def test_get_item_versions(self):
        items = self.bim360.get_item_versions(self.project_id, self.item_id)
        self.assertNotEquals(len(items), 0)

    def test_batch_report_item_versions(self):
        df = self.bim360.batch_report_item_versions(self.project_id, self.item_id)
        self.assertNotEquals(df.empty, True)

    def test_batch_report_folder_contents(self):
        df = self.bim360.batch_report_folder_contents(self.project_id, self.folder_id, 2)
        self.assertNotEquals(df.empty, True)

    def test_get_urn_item_version(self):
        urn = self.bim360.get_urn_item_version(self.project_id, self.item_id, 2)
        self.assertNotEquals(urn, "")

    def test_batch_report_projects(self):
        df = self.bim360.batch_report_projects(self.hub_id)
        self.assertNotEquals(df.empty, True)

    def test_batch_report_top_folders(self):
        df = self.bim360.batch_report_top_folders(self.hub_id, self.project_id)
        self.assertNotEquals(df.empty, True)

    def test_batch_report_items(self):
        self.bim360.token = Auth().auth3leg()
        df = self.bim360.batch_report_items(self.project_id, self.folder_id, ".rvt", True)
        self.assertNotEquals(df.empty, True)

    def test_get_item__display_name(self):
        item_name = self.bim360.get_item_display_name(self.project_id, self.item_id)
        self.assertNotEquals(item_name, "")

    def test_create_object_storage(self):
        object_name = "hello.pdf"
        result = self.bim360.create_object_storage(self.project_id, self.folder_id, object_name)
        id = result['data']['id']
        sign = self.bim360.signeds_3_upload(id)
        upload_key = sign['uploadKey']
        url = sign['urls'][0]
        path = r"C:\Users\chuongho\Downloads\Feature Summary - Macro Tools Renovation.pdf"
        upload = self.bim360.upload_file_to_signed_url(url, path)
        complete = self.bim360.complete_upload(upload_key, id)
        try:
            file_version = self.bim360.create_first_version_file(self.project_id, self.folder_id, object_name, id)
        except Exception as e:
            error = "Another object with the same name already exists in this container"
            if error in str(e):
                print("File already exists")
                item_id = self.bim360.get_item_id(self.project_id, self.folder_id, object_name)
                file_version = self.bim360.create_new_file_version(self.project_id, item_id, object_name, id)

        self.assertNotEquals(result, 0)

    def test_upload_file_item(self):
        path = r"./test/resources/Test.dwg"
        full_path = os.path.abspath(path)
        result = self.bim360.upload_file_item(self.project_id, self.folder_id, full_path)
        self.assertNotEquals(result, 0)

    def test_copy_file_item(self):
        target_folder_id = "urn:adsk.wipprod:fs.folder:co.ThXlEqHBSomEoh_TdHE5AA"
        result = self.bim360.copy_file_item(self.item_id, self.project_id, self.project_id, target_folder_id)
        self.assertNotEquals(result, 0)

    def test_copy_folder_contents(self):
        target_folder_id = "urn:adsk.wipprod:fs.folder:co._i893SszR2S-8DfcCvAzIg"
        self.bim360.copy_folder_contents(self.project_id, self.folder_id, self.project_id, target_folder_id)

    def test_delete_file_item(self):
        file_name = "Test.dwg"
        result = self.bim360.delete_file_item(self.project_id, self.folder_id, file_name)
        self.assertNotEquals(result, 0)

    def test_download_file_item(self):
        file_name = "Test.dwg"
        file_path = r"./test/resources/Test2.dwg"
        file_path_result = self.bim360.download_file_item(file_path, self.project_id, self.folder_id, file_name, 1)
        size = os.path.getsize(file_path)
        size_result = os.path.getsize(file_path_result)
        self.assertEqual(size, size_result)

    def test_download_file_stream_item(self):
        file_name = "Test.dwg"
        byte_io = self.bim360.download_file_stream_item(self.project_id, self.folder_id, file_name, 2)
        self.assertNotEquals(byte_io, 0)
        # download
        with open(file_name, 'wb') as f:
            f.write(byte_io.read())
        #open
        size = os.path.getsize(file_name)
        self.assertNotEquals(size, 0)
    def test_restore_file_item(self):
        item_id = "urn:adsk.wipprod:fs.file:vf.-wv2uodvSgaXmUZ4O0oYkw";
        result = self.bim360.restore_file_item(self.project_id, item_id, 2)
        self.assertNotEquals(result, 0)

    def test_create_folder(self):
        folder_name = "TestFolder"
        result = self.bim360.create_folder(self.project_id, self.folder_id, folder_name)
        self.assertNotEquals(result, 0)

    def test_rename_folder(self):
        result = self.bim360.rename_folder(self.project_id, self.folder_id, "Test")
        self.assertNotEquals(result, 0)
