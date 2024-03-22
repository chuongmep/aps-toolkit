from unittest import TestCase
import os
from .context import Auth
from .context import DbReader


class TestDbReader(TestCase):
    def setUp(self):
        self.client_id = os.environ['APS_CLIENT_ID']
        self.client_secret = os.environ['APS_CLIENT_SECRET']
        self.auth = Auth(self.client_id, self.client_secret)
        self.token = self.auth.auth2leg()
        self.urn = "dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6Y2h1b25nX2J1Y2tldC9NeUhvdXNlLm53Yw"

    def test_reader(self):
        db_reader = DbReader(self.urn, self.token)
        self.assertNotEquals(db_reader, "")

    def test_execute_query(self):
        db_reader = DbReader(self.urn, self.token)
        print(db_reader.db_path)
        query = "SELECT * FROM _objects_id"
        df = db_reader.execute_query(query)
        self.assertNotEquals(df.empty, True)
