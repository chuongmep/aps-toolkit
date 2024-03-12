import os
import sys

# Assuming the src folder is at the same level as the test folder
src_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), '..'))
print(src_dir)
sys.path.append(src_dir)
# Now you can import modules from the project folder
from aps_toolkit import PropReader
from aps_toolkit import PropDbReaderRevit
from aps_toolkit import DbReader
from aps_toolkit import Auth
from aps_toolkit import Token
from aps_toolkit import BIM360
from aps_toolkit import Fragments
from aps_toolkit import Derivative
from aps_toolkit import SVFReader

APS_CLIENT_ID = os.environ["APS_CLIENT_ID"]
APS_CLIENT_SECRET = os.environ["APS_CLIENT_SECRET"]
