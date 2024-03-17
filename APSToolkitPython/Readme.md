APSToolkit 

## Requirements

- Python 3.9+


## How to install the project

```bash
pip install aps-toolkit
```

## Get Started

```python
from aps_toolkit import Auth
auth = Auth()
token = auth.auth2leg()
```

## Tutorial

Please refer to the github [tutorial](https://github.com/chuongmep/aps-toolkit)

## How to build the project

Install package : pip install wheel

```bash
python setup.py bdist_wheel
python setup.py sdist
python setup.py bdist_wheel sdist
```

## Install the project at local

- Remove the old package at site-packages folder
- Install the new updated package
```bash
pip install dist/<name>.whl
```

## Publish the project

- Run the following command to build the package
```bash
python setup.py bdist_wheel sdist
```
- Run the following command to check and upload the package to pypi
```bash
python -m twine check dist/*
python -m twine upload dist/*
```

## Issue Known

- [cannot import name 'appengine' from 'requests.packages.urllib3.contrib'](https://stackoverflow.com/questions/76175487/sudden-importerror-cannot-import-name-appengine-from-requests-packages-urlli)

```python
pip install --upgrade twine requests-toolbelt
```

## License

This project is licensed under the GNU **General Public License V3 License** - see the [[LICENSE.md](https://en.wikipedia.org/wiki/GNU_General_Public_License)](