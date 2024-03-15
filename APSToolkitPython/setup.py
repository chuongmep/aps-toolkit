import setuptools

with open("Readme.md") as f:
    if f is not None:
        readme = f.read()

setuptools.setup(
    name="aps-toolkit",
    version="0.3.4",
    author="chuong mep",
    author_email="chuongpqvn@gmail.com",
    description="A Toolkit Autodesk Platform Services for Python",
    long_description=readme,
    long_description_content_type="text/markdown",
    url="https://github.com/chuongmep/aps-toolkit",
    project_urls={
        "Bug Tracker": "https://github.com/chuongmep/aps-toolkit/issues",
    },
    classifiers=[
        "Programming Language :: Python :: 3",
        "License :: OSI Approved :: MIT License",
        "Operating System :: OS Independent",
    ],
    package_dir={"": "src"},
    packages=setuptools.find_packages(where="src"),
    python_requires=">=3.9",
    install_requires=['requests', 'pandas']
)
