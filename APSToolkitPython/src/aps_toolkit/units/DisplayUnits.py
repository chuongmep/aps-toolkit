import json
import requests
import os


class DisplayUnits:
    def __init__(self):
        self._read_units_local()

    def _read_units_stream(self):
        url = "https://gist.githubusercontent.com/chuongmep/497dea0ead458b52d001f2c806f32f9d/raw/a149bc1ee805485303e32122d8ccb8714231992e/units.json"
        try:
            response = requests.get(url)
            if response.status_code == 200:
                self.units = response.json()
                print("Units loaded")
            else:
                print(f"Error: Failed to retrieve units data. Status code: {response.status_code}")
        except Exception as e:
            print(f"Error: {e}")

    def _read_units_local(self):
        # get relative path
        dir_path = os.path.dirname(os.path.realpath(__file__))
        file_name = "units.json"
        file_path = os.path.join(dir_path, file_name)
        try:
            with open(file_path, "r", encoding="utf-8") as file:
                self.units = json.load(file)
                # convert to dict
                self.units = {unit["TypeId"]: unit["UnitLabel"] for unit in self.units}
        except FileNotFoundError:
            print(f"Error: File '{file_path}' not found.")
        except json.JSONDecodeError as e:
            print(f"Error decoding JSON file: {e}")

    def parse_symbol(self, type_id: str):
        if type_id is None:
            return ""
        if "-" in type_id:
            type_id = type_id.split("-")[0]
            return self.units.get(type_id, "")
        else:
            return ""
