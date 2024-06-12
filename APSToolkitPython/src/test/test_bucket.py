import unittest

from aps_toolkit.Bucket import PublicKey
from .context import Bucket
from .context import Auth


class TestBucket(unittest.TestCase):
    def setUp(self):
        self.token = Auth().auth2leg()

    def test_get_all_buckets(self):
        bucket = Bucket(self.token)
        buckets = bucket.get_all_buckets()
        self.assertNotEqual(len(buckets), 0)

    def test_create_bucket(self):
        bucket = Bucket(self.token)
        bucket_name = "hello_world_23232"
        policy_key = PublicKey.transient
        response = bucket.create_bucket(bucket_name, policy_key)
        self.assertEqual(response["bucketKey"], bucket_name)

    def test_delete_bucket(self):
        bucket = Bucket(self.token)
        bucket_name = "hello_world_23232"
        result = bucket.delete_bucket(bucket_name)
        self.assertEqual(result, b'')

    def test_get_objects(self):
        bucket = Bucket(self.token)
        bucket_name = "hello_world_23232"
        objects = bucket.get_objects(bucket_name)
        self.assertNotEqual(len(objects), 0)

    def test_upload_object(self):
        bucket = Bucket(self.token)
        bucket_name = "hello_world_23232"
        file_path = "./test/resources/Test.dwg"
        object_name = "Test.dwg"
        response = bucket.upload_object(bucket_name, file_path, object_name)
        self.assertEqual(response["bucketKey"], bucket_name)

    def test_delete_object(self):
        bucket = Bucket(self.token)
        bucket_name = "hello_world_23232"
        object_name = "Test.dwg"
        result = bucket.delete_object(bucket_name, object_name)
        self.assertEqual(result, b'')

    def test_download_object(self):
        bucket = Bucket(self.token)
        bucket_name = "hello_world_23232"
        object_name = "Test.dwg"
        file_path = "./test/resources/Test2.dwg"
        bucket.download_object(bucket_name, object_name, file_path)
        self.assertTrue(True)

    def test_download_stream_object(self):
        bucket = Bucket(self.token)
        bucket_name = "hello_world_23232"
        object_name = "chuong.pdf"
        stream = bucket.download_stream_object(bucket_name, object_name)
        self.assertIsNotNone(stream)
