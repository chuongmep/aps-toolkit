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
        self.urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLjAtYnBtcEpXUWJTRUVNdUFac1VETWc_dmVyc2lvbj0yNQ"

    def test_reader(self):
        db_reader = DbReader(self.urn, self.token)
        self.assertNotEquals(db_reader, "")

    def test_execute_query(self):
        db_reader = DbReader(self.urn, self.token)
        print(db_reader.db_path)
        query = "SELECT * FROM _objects_id"
        df = db_reader.execute_query(query)
        self.assertNotEquals(df.empty, True)
