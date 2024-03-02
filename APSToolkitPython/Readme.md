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

## Publish the project

```bash
python -m twine check dist/*
python -m twine upload dist/*
```