from enum import Enum
import pandas as pd
import requests
from .Token import Token
import os


class PublicKey(Enum):
    transient = "transient"
    temporary = "temporary"
    persistent = "persistent"


class Bucket:
    def __init__(self, token: Token, region: str = "US"):
        self.token = token
        self.region = region
        self.host = "https://developer.api.autodesk.com/oss/v2/buckets"

    def get_all_buckets(self) -> pd.DataFrame:
        """
          Retrieves all the buckets from the Autodesk OSS API.

          This method sends a GET request to the Autodesk OSS API and includes an Authorization header with a bearer token for authentication. If the response status code is not 200, it raises an exception with the response content.

          If the request is successful, it processes the JSON response to create a pandas DataFrame. The 'createdDate' field, which is in milliseconds since epoch, is converted to a real date and updated in the DataFrame.

          Returns:
              pd.DataFrame: A DataFrame containing all the buckets.
          """
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
        """
            Creates a new bucket in the Autodesk OSS API.

            This method sends a POST request to the Autodesk OSS API. It includes an Authorization header with a
            bearer token for authentication and a Content-Type header set to "application/json". The bucket name and
            policy key are passed in the body of the request as JSON data. If the response status code is not 200,
            it raises an exception with the response content.

            Args: bucket_name (str): The name of the bucket to be created. policy_key (PublicKey): The policy key for
            the bucket. It can be one of the following: 'transient', 'temporary', or 'persistent'.

            Returns:
                dict: A dictionary containing the response from the Autodesk OSS API.
            """
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
            raise Exception(response.reason)
        return response.json()

    def delete_bucket(self, bucket_name: str):
        """
            Deletes a bucket in the Autodesk OSS API.

            This method sends a DELETE request to the Autodesk OSS API. It includes an Authorization header with a bearer token for authentication. The bucket name is passed in the URL of the request. If the response status code is not 200, it raises an exception with the response content.

            Args:
                bucket_name (str): The name of the bucket to be deleted.

            Returns:
                dict: A dictionary containing the response from the Autodesk OSS API.
            """
        headers = {
            "Authorization": f"Bearer {self.token.access_token}"
        }
        url = f"{self.host}/{bucket_name}"
        response = requests.delete(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        return response.content

    def get_objects(self, bucket_name: str) -> pd.DataFrame:
        """
          Retrieves all the objects in a specified bucket from the Autodesk OSS API.

          This method sends a GET request to the Autodesk OSS API. It includes an Authorization header with a bearer token for authentication. The bucket name is passed in the URL of the request. If the response status code is not 200, it raises an exception with the response content.

          Args:
              bucket_name (str): The name of the bucket from which to retrieve objects.

          Returns:
              pd.DataFrame: A DataFrame containing all the objects in the specified bucket.
          """
        headers = {
            "Authorization": f"Bearer {self.token.access_token}"
        }
        url = f"{self.host}/{bucket_name}/objects"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        data = response.json()
        df = pd.DataFrame(data["items"])
        return df

    def upload_object(self, bucket_name: str, file_path: str, object_name: str) -> dict:
        """
           Uploads an object to a specified bucket in the Autodesk OSS API.

           This method sends a PUT request to the Autodesk OSS API. It includes an Authorization header with a bearer token for authentication and a Content-Type header set to "application/octet-stream". The bucket name and object name are passed in the URL of the request, and the file to be uploaded is passed in the body of the request as binary data. If the response status code is not 200, it raises an exception with the response content.

           Args:
               bucket_name (str): The name of the bucket to which the object will be uploaded.
               file_path (str): The path of the file to be uploaded.
               object_name (str): The name of the object to be created in the bucket.

           Returns:
               dict: A dictionary containing the response from the Autodesk OSS API.
           """
        if not os.path.isabs(file_path):
            file_path = os.path.abspath(file_path)
        with open(file_path, "rb") as file:
            stream = file.read()
            return self.upload_object_stream(bucket_name, stream, object_name)

    def upload_object_stream(self, bucket_name: str, stream: bytes, object_name: str) -> dict:
        """
           Uploads an object to a specified bucket in the Autodesk OSS API.

           This method sends a PUT request to the Autodesk OSS API. It includes an Authorization header with a bearer token for authentication and a Content-Type header set to "application/octet-stream". The bucket name and object name are passed in the URL of the request, and the file to be uploaded is passed in the body of the request as binary data. If the response status code is not 200, it raises an exception with the response content.

           Args:
               bucket_name (str): The name of the bucket to which the object will be uploaded.
               stream (bytes): The stream of the file to be uploaded.
               object_name (str): The name of the object to be created in the bucket.

           Returns:
               dict: A dictionary containing the response from the Autodesk OSS API.
           """
        headers = {
            "Authorization": f"Bearer {self.token.access_token}",
            "Content-Type": "application/octet-stream"
        }
        url = f"{self.host}/{bucket_name}/objects/{object_name}"
        response = requests.put(url, headers=headers, data=stream)
        if response.status_code != 200:
            raise Exception(response.reason)
        return response.json()

    def delete_object(self, bucket_name: str, object_name: str) -> dict:
        """
            Deletes an object from a specified bucket in the Autodesk OSS API.

            This method sends a DELETE request to the Autodesk OSS API. It includes an Authorization header with a bearer token for authentication. The bucket name and object name are passed in the URL of the request. If the response status code is not 200, it raises an exception with the response content.

            Args:
                bucket_name (str): The name of the bucket from which the object will be deleted.
                object_name (str): The name of the object to be deleted.

            Returns:
                dict: A dictionary containing the response from the Autodesk OSS API.
            """
        headers = {
            "Authorization": f"Bearer {self.token.access_token}"
        }
        url = f"{self.host}/{bucket_name}/objects/{object_name}"
        response = requests.delete(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        return response.content

    def download_object(self, bucket_name: str, object_name: str, file_path: str) -> bool:
        """
            Downloads an object from a specified bucket in the Autodesk OSS API.

            This method sends a GET request to the Autodesk OSS API. It includes an Authorization header with a bearer token for authentication. The bucket name and object name are passed in the URL of the request. If the response status code is not 200, it raises an exception with the response content.

            The downloaded content is written to a file at the specified file path.

            Args:
                bucket_name (str): The name of the bucket from which the object will be downloaded.
                object_name (str): The name of the object to be downloaded.
                file_path (str): The path where the downloaded file will be saved.

            Returns:
                None
            """
        headers = {
            "Authorization": f"Bearer {self.token.access_token}"
        }
        url = f"{self.host}/{bucket_name}/objects/{object_name}/signeds3download"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        if not os.path.isabs(file_path):
            file_path = os.path.abspath(file_path)
            if not os.path.exists(os.path.dirname(file_path)):
                os.makedirs(os.path.dirname(file_path))
        result = response.json()
        url = result["url"]
        response = requests.get(url)
        with open(file_path, "wb") as file:
            file.write(response.content)
            file.close()
        return True

    def download_stream_object(self, bucket_name: str, object_name: str) -> bytes:
        """
            Downloads an object from a specified bucket in the Autodesk OSS API.

            This method sends a GET request to the Autodesk OSS API. It includes an Authorization header with a bearer token for authentication. The bucket name and object name are passed in the URL of the request. If the response status code is not 200, it raises an exception with the response content.

            The downloaded content is written to a file at the specified file path.

            Args:
                bucket_name (str): The name of the bucket from which the object will be downloaded.
                object_name (str): The name of the object to be downloaded.

            Returns:
                None
            """
        headers = {
            "Authorization": f"Bearer {self.token.access_token}"
        }
        url = f"{self.host}/{bucket_name}/objects/{object_name}"
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            raise Exception(response.reason)
        return response.content
