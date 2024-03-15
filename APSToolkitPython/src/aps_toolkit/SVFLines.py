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
class SVFLines:
    def __init__(self, isLines=False, v_count=None, l_count=None, vertices=None, indices=None, colors=None,
                 line_width=None):
        self.isLines = isLines
        self.v_count = v_count
        self.l_count = l_count
        self.vertices = vertices
        self.indices = indices
        self.colors = colors
        self.line_width = line_width
