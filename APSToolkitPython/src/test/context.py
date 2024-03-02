import os
import sys

# Assuming the src folder is at the same level as the test folder
src_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), '..'))

sys.path.append(src_dir)
# Now you can import modules from the project folder
from aps_toolkit_python import PropReader
from aps_toolkit_python import PropDbReaderRevit
from aps_toolkit_python import Auth
from aps_toolkit_python import Token
from aps_toolkit_python import BIM360

APS_CLIENT_ID = os.environ["APS_CLIENT_ID"]
APS_CLIENT_SECRET = os.environ["APS_CLIENT_SECRET"]
