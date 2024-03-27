import pandas as pd

from .Token import Token
import requests
import pandas
from datetime import datetime
from enum import Enum


class PublicKey(Enum):
    transient = "transient"
    temporary = "temporary"
    persistent = "persistent"


class Bucket:
    def __init__(self, token: Token, region="US"):
        self.token = token
        self.region = region
        self.host = "https://developer.api.autodesk.com/oss/v2/buckets"

    def get_all_buckets(self) -> pd.DataFrame:
        headers = {
            "Authorization": f"Bearer {self.token.access_token}"
        }
        response = requests.get(self.host, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        data = response.json()
        df = pd.DataFrame(data["items"])
        milliseconds_since_epoch = df["createdDate"]
        seconds_since_epoch = milliseconds_since_epoch // 1000
        real_date = pd.to_datetime(seconds_since_epoch, unit="s")
        df["createdDate"] = real_date
        return df

    def create_bucket(self, bucket_name: str, policy_key: PublicKey) -> dict:
        headers = {
            "Authorization": f"Bearer {self.token.access_token}",
            "Content-Type": "application/json"
        }
        data = {
            "bucketKey": bucket_name,
            "policyKey": policy_key.value
        }
        response = requests.post(self.host, headers=headers, json=data)
        if response.status_code != 200:
            raise Exception(response.content)
        return response.json()

    def delete_bucket(self, bucket_name: str) -> dict:
        headers = {
            "Authorization": f"Bearer {self.token.access_token}"
        }
        url = f"{self.host}/{bucket_name}"
        response = requests.delete(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        return response.content

    def get_objects(self, bucket_name: str) -> pd.DataFrame:
        headers = {
            "Authorization": f"Bearer {self.token.access_token}"
        }
        url = f"{self.host}/{bucket_name}/objects"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        data = response.json()
        df = pd.DataFrame(data["items"])
        return df

    def upload_object(self, bucket_name: str, file_path: str, object_name: str) -> dict:
        headers = {
            "Authorization": f"Bearer {self.token.access_token}",
            "Content-Type": "application/octet-stream"
        }
        url = f"{self.host}/{bucket_name}/objects/{object_name}"
        with open(file_path, "rb") as file:
            response = requests.put(url, headers=headers, data=file)
            if response.status_code != 200:
                raise Exception(response.content)
            return response.json()

    def delete_object(self, bucket_name: str, object_name: str) -> dict:
        headers = {
            "Authorization": f"Bearer {self.token.access_token}"
        }
        url = f"{self.host}/{bucket_name}/objects/{object_name}"
        response = requests.delete(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        return response.content

    def download_object(self, bucket_name: str, object_name: str, file_path: str) -> None:
        headers = {
            "Authorization": f"Bearer {self.token.access_token}"
        }
        url = f"{self.host}/{bucket_name}/objects/{object_name}"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.content)
        with open(file_path, "wb") as file:
            file.write(response.content)
            file.close()
        return None
