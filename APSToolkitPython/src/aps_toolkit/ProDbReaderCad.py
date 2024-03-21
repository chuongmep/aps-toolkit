"""
Copyright (C) 2024  chuongmep.com

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
"""
from typing import List
import re
import pandas as pd
import json
import requests
from .PropReader import PropReader
from .ManifestItem import ManifestItem


class PropDbReaderCad(PropReader):
    def __int__(self, urn, token, region="US", manifest_item: [ManifestItem] = None):
        super().__init__(urn, token, region, manifest_item)

    def get_document_info(self) -> pd.Series:
        schema_name = "DocumentData"
        indexs = []
        # get index at value DocumentData
        for i, s in enumerate(self.ids):
            if s == schema_name:
                indexs.append(i)
        df = self.get_recursive_ids(indexs)
        series = df.iloc[0]
        return series
