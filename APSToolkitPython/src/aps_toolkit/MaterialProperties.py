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
class MaterialProperties:
    def __init__(self, integers=None, booleans=None, strings=None, uris=None, scalars=None, colors=None,
                 choicelists=None, uuids=None, references=None):
        self.integers = integers
        self.booleans = booleans
        self.strings = strings
        self.uris = uris
        self.scalars = scalars
        self.colors = colors
        self.choicelists = choicelists
        self.uuids = uuids
        self.references = references
