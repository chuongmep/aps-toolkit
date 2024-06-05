import pandas as pd

from .Token import Token
from .Auth import Auth
import requests


class Webhooks:
    def __init__(self, token: Token = None, region="US"):
        if token:
            self.token = token
        else:
            auth = Auth()
            self.token = auth.auth2leg()

        self.host = "https://developer.api.autodesk.com"
        self.region = region

    def get_all_hooks(self) -> dict:
        """
        Retrieves a paginated list of webhooks created in the context of a Client or Application. This API accepts 2-legged token of the application only. If the pageState query string is not specified, the first page is returned.
        :return:
        """
        url = f"{self.host}/webhooks/v1/hooks"
        headers = {
            "Authorization": f"{self.token.token_type} {self.token.access_token}",
            "x-ads-region": self.region
        }
        response = requests.get(url, headers=headers)
        if response.status_code == 200:
            return response.json()
        else:
            raise Exception(response.reason)

    def batch_report_all_hooks(self) -> pd.DataFrame:
        """
        Return all hooks in a pandas DataFrame, include all the properties of the hooks
        :return: :class:`pd.DataFrame` - A pandas DataFrame with all the hooks
        """
        data = self.get_all_hooks()["data"]
        if not data:
            raise Exception("No data found")
        df = pd.DataFrame()
        for item in data:
            props_dict = {}
            for key, value in item.items():
                if isinstance(value, dict):
                    for k, v in value.items():
                        props_dict[k] = v
                else:
                    props_dict[key] = value
            single_df = pd.DataFrame(props_dict, index=[0])
            df = pd.concat([df, single_df], ignore_index=True)
        df = df.dropna(axis=1, how='all')
        return df

    def get_all_app_hooks(self) -> dict:
        """
        Retrieves a paginated list of webhooks created in the context of a Client or Application. This API accepts 2-legged token of the application only. If the pageState query string is not specified, the first page is returned.
        :return:
        """
        url = f"{self.host}/webhooks/v1/app/hooks"
        headers = {
            "Authorization": f"{self.token.token_type} {self.token.access_token}",
            "x-ads-region": self.region
        }
        response = requests.get(url, headers=headers)
        if response.status_code == 200:
            return response.json()
        else:
            raise Exception(response.reason)

    def get_hook_by_id(self, hook_id: str, event: str = "dm.version.added", system: str = "data") -> dict:
        """
        Get details of a webhook based on its webhook ID
        https://aps.autodesk.com/en/docs/webhooks/v1/reference/http/webhooks/systems-system-events-event-hooks-hook_id-GET/
        :param event: The event type. Default is dm.version.added.
        :param system: The system type. Default is data.
        :param hook_id: The ID of the webhook.
        :return:
        """
        url = f"{self.host}/webhooks/v1/systems/{system}/events/{event}/hooks/{hook_id}"
        headers = {
            "Authorization": f"{self.token.token_type} {self.token.access_token}",
            "x-ads-region": self.region
        }
        response = requests.get(url, headers=headers)
        return response.json()

    def delete_hook_by_id(self, hook_id: str, event: str = "dm.version.added", system: str = "data"):
        """
        Delete a webhook based on its webhook ID
        https://aps.autodesk.com/en/docs/webhooks/v1/reference/http/webhooks/systems-system-events-event-hooks-hook_id-DELETE/
        :param event: The event type. Default is dm.version.added.
        :param system: The system type. Default is data.
        :param hook_id: The ID of the webhook.
        :return:
        """
        url = f"{self.host}/webhooks/v1/systems/{system}/events/{event}/hooks/{hook_id}"
        headers = {
            "Authorization": f"{self.token.token_type} {self.token.access_token}",
            "x-ads-region": self.region
        }
        return requests.delete(url, headers=headers)

    def create_system_event_hook(self, scope: str, callback_url: str = "http://localhost:8080/api/webhooks/callback",
                                 event: str = "dm.version.added", system: str = "data", hookAttribute: str = None):
        """
        Add new webhooks to receive the notification on all the events.
        https://aps.autodesk.com/en/docs/webhooks/v1/reference/http/webhooks/systems-system-hooks-POST/
        :param scope: An object that represents the extent to where the event is monitored. For example, if the scope is folder, the webhooks service generates a notification for the specified event occurring in any sub folder or item within that folder. Please refer to the individual event specification pages for valid scopes.
        :param callback_url: Callback URL registered for the webhook.
        :param event: The event type. Default is dm.version.added. see https://aps.autodesk.com/en/docs/webhooks/v1/reference/events
        :param system: A system for example: data for Data Management. Default is data.
        :param hookAttribute: A user-defined JSON object, which you can use to store/set some custom information. The maximum size of the JSON object (content) should be less than 1KB
        :return:
        """
        url = f"{self.host}/webhooks/v1/systems/{system}/events/{event}/hooks"
        headers = {
            "Authorization": f"{self.token.token_type} {self.token.access_token}",
            "Content-Type": "application/json",
            "x-ads-region": self.region
        }
        data = {
            "callbackUrl": callback_url,
            "scope": scope,
        }
        if hookAttribute:
            data["hookAttribute"] = hookAttribute
        response = requests.post(url, headers=headers, json=data)
        return response.json()
