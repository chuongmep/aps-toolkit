![Platform](https://img.shields.io/badge/platform-Windows-lightgray.svg) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

![ReSharper](https://img.shields.io/badge/ReSharper-2023-yellow) ![Rider](https://img.shields.io/badge/Rider-2023-yellow) ![Visual Studio 2022](https://img.shields.io/badge/Visual_Studio_2022-yellow) ![.NET Framework](https://img.shields.io/badge/.NET_6-yellow)

[![Publish](../../actions/workflows/dotnet.yml/badge.svg)](../../actions)
[![Nuget Version](https://img.shields.io/nuget/v/APSToolkit)](https://www.nuget.org/packages/APSToolkit)
[![NuGet Downloads](https://img.shields.io/nuget/dt/APSToolkit.svg)](https://www.nuget.org/packages/APSToolkit/)

<a href="https://twitter.com/intent/follow?screen_name=chuongmep">
<img src="https://img.shields.io/twitter/follow/chuongmep?style=social&logo=twitter"
alt="follow on Twitter"></a>

## APS Toolkit

APS Toolkit (Former is Forge) is powerful for you to explore `Autodesk Platform Services`(APS). It's built on top of [Autodesk.Forge](https://www.nuget.org/packages/Autodesk.Forge/) and [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/). Forge Toolkit includes some features allow you to read, download and write data from `Autodesk Platform Services` and export to CSV, Excel, JSON, XML, etc.

![APSToolkit](docs/APSToolkit.png)

## Features

- [x] Read/Download SVF Model
- [x] Read/Query Properties Database SQLite
- [x] Read/Download Properties Without Viewer
- [x] Read Geometry Data 
- [x] Read Metadata
- [x] Read Fragments
- [x] Read MeshPacks
- [x] Read Images
- [x] Export Data to CSV
- [x] Export Data to Excel
- [x] Export Data to Parquet

## Installation

Please follow latest update at [APSToolkit Nuget](https://www.nuget.org/packages/APSToolkit)

```bash
<PackageReference Include="APSToolkit" Version="1.*" />
```

Before start you need setup your environment:

```bash
APS_CLIENT_ID = <your client id>
APS_CLIENT_SECRET = <your client secret>
APS_REFRESH_TOKEN = <your refresh token>
```

## Tutorials

- WORKING IN PROGRESS

## Read Properties From Local

```csharp
//TODO : Test Get Properties Local files
string objects_attrs_gzip = @"<yourpath>\Resource\objects_attrs.json.gz";
string objects_vals_gzip = @"<yourpath>\Resource\objects_vals.json.gz";
string objects_ids_gzip = @"<yourpath>\Resource\objects_ids.json.gz";
string objects_avs_gzip = @"<yourpath>\Resource\objects_avs.json.gz";
string objects_offsets_gzip = @"<yourpath>\Resource\objects_offs.json.gz";
PropDbReader proReader = new PropDbReader(objects_ids_gzip, objects_offsets_gzip, objects_avs_gzip,
    objects_attrs_gzip, objects_vals_gzip);
int dbIndex = 3528;
Dictionary<string,string> dictionary = proReader.GetProperties(dbIndex);
Console.WriteLine("done get");
```

## Read Properties From Remote

```csharp

string urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0z";
Autodesk.Forge.TwoLeggedApi twoLeggedApi = new Autodesk.Forge.TwoLeggedApi();
var ClientID = Environment.GetEnvironmentVariable("APS_CLIENT_ID");
var ClientSecret = Environment.GetEnvironmentVariable("APS_CLIENT_SECRET");
Console.WriteLine("Done with Authentication");
dynamic token = twoLeggedApi.Authenticate(ClientID, ClientSecret, "client_credentials",
  new Scope[]
  {
    Scope.DataRead, Scope.DataWrite, Scope.DataCreate, Scope.DataSearch, Scope.BucketCreate, Scope.BucketRead,
    Scope.BucketUpdate, Scope.BucketDelete
  });
var access_token = token.access_token;
PropDbReader probReader = new PropDbReader(urn, access_token);
Dictionary<string,string> properties = probReader.ExportDataToExcel("<filePath>","result.xlsx");
```

## Read Metadata By Query SQLite

```csharp
string sqlQuery = @"
    SELECT _objects_id.id AS dbId, _objects_id.external_id AS externalId, 
           _objects_attr.name AS name,_objects_attr.display_name AS propName , 
           _objects_val.value AS propValue
    FROM _objects_eav
        INNER JOIN _objects_id ON _objects_eav.entity_id = _objects_id.id
        INNER JOIN _objects_attr ON _objects_eav.attribute_id = _objects_attr.id
        INNER JOIN _objects_val ON _objects_eav.value_id = _objects_val.id
    WHERE name = 'ElementId'
    ";
//read query data legacy
var dbReader = new PropDbReader("<urn>","<token>");
DataTable dataTable = DbReader.ExecuteQuery(sqlQuery);
 ```

## Export Data to CSV

```csharp
RevitPropDbReader = new PropDbReaderRevit(urn, Settings.Token2Leg);
List<string> parameters = new List<string>()
{
    "Category",
    "ElementId",
    "name",
    "Level",
};
DataTable dataTable = RevitPropDbReader.GetAllDataByParameter(parameters);
dataTable.ExportToCsv("result.csv");
```

## Export Data ACC To Excel 

```csharp
BIM360 bim360 = new BIM360();
bim360.ExportRevitDataToExcel("<token>", "<filePath>","<versionId>");
```

## Export All Revit Data to Excel 

```csharp
RevitPropDbReader = new PropDbReaderRevit("<urn>", "<token>");
RevitPropDbReader.ExportAllDataToExcel("result.xlsx");
```

## Download SVF Model

```csharp
string dowloadFolder = @"D:\Temp\SVF\MyHouse";
string urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0z";
Autodesk.Forge.TwoLeggedApi twoLeggedApi = new Autodesk.Forge.TwoLeggedApi();
var ClientID = Environment.GetEnvironmentVariable("APS_CLIENT_ID");
var ClientSecret = Environment.GetEnvironmentVariable("APS_CLIENT_SECRET");
Console.WriteLine("Done with Authentication");
dynamic token = twoLeggedApi.Authenticate(ClientID, ClientSecret, "client_credentials",
    new Scope[]
    {
        Scope.DataRead, Scope.DataWrite, Scope.DataCreate, Scope.DataSearch, Scope.BucketCreate, Scope.BucketRead,
        Scope.BucketUpdate, Scope.BucketDelete
    });
var access_token = token.access_token;
await Derivatives.SaveFileSvfAsync("<folder>", urn, access_token);

```
## Read SVF Model Local

```csharp
ForgeSvfReader forgeSvfReader = new ForgeSvfReader();
string svfPath = @"<yourpath>\<name>.svf";
ISvfContent svfContent = forgeSvfReader.ReadSvf(svfPath);
## read svf properties
// get object properties
PropDbReader propDbReader = svfContent.properties;
int dbIndex = 3461;
Dictionary<string, string> properties = propDbReader.GetProperties(dbIndex);
// get object name
string name = propDbReader.GetName(3461);
```

## Read Fragments Stream

```csharp

string urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0z";
Autodesk.Forge.TwoLeggedApi twoLeggedApi = new Autodesk.Forge.TwoLeggedApi();
var ClientID = Environment.GetEnvironmentVariable("APS_CLIENT_ID");
var ClientSecret = Environment.GetEnvironmentVariable("APS_CLIENT_SECRET");
Console.WriteLine("Done with Authentication");
dynamic token = twoLeggedApi.Authenticate(ClientID, ClientSecret, "client_credentials",
  new Scope[]
  {
    Scope.DataRead, Scope.DataWrite, Scope.DataCreate, Scope.DataSearch, Scope.BucketCreate, Scope.BucketRead,
    Scope.BucketUpdate, Scope.BucketDelete
  });
var access_token = token.access_token;
Dictionary<string,ISvfFragment[]> fragments = await Derivatives.ReadFragmentsRemoteAsync(urn, access_token);
```
## ReadFragments Local

```csharp
string FragmentList =
  @".\FragmentList.pack";
byte[] buffer = System.IO.File.ReadAllBytes(FragmentList);
ISvfFragment[] svfFragments = Fragments.parseFragments(buffer);
// try export to csv
using (var writer = new StreamWriter(@".output\fragments.csv"))
using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
{
  csv.WriteRecords(svfFragments);
}
Console.WriteLine("Done with Fragments");
```

## Read Geometries Stream 

```csharp
string urn = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0z";
Autodesk.Forge.TwoLeggedApi twoLeggedApi = new Autodesk.Forge.TwoLeggedApi();
var ClientID = Environment.GetEnvironmentVariable("APS_CLIENT_ID");
var ClientSecret = Environment.GetEnvironmentVariable("APS_CLIENT_SECRET");
Console.WriteLine("Done with Authentication");
dynamic token = twoLeggedApi.Authenticate(ClientID, ClientSecret, "client_credentials",
  new Scope[]
  {
    Scope.DataRead, Scope.DataWrite, Scope.DataCreate, Scope.DataSearch, Scope.BucketCreate, Scope.BucketRead,
    Scope.BucketUpdate, Scope.BucketDelete
  });
var access_token = token.access_token;
var geometries = await Derivatives.ReadGeometriesRemoteAsync(urn, access_token);
```
## Read Geometries Local

```csharp
string geometryMetadata = @"<yourpath>\GeometryMetadata.pf";
byte[] buffer = System.IO.File.ReadAllBytes(me);
var geometries = Geometries.parseGeometries(buffer);
```

NOTE : Please see repo [APSToolkitUnit](./APSToolkitUnit) to get more example.

## Dependencies

- [Autodesk.Forge](https://www.nuget.org/packages/Autodesk.Forge/)
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)

## License
Thís project is licensed under the terms of the [MIT](LICENSE).

Many thanks some repos:

- [forge-convert-utils](https://github.com/petrbroz/forge-convert-utils)
- [UnityForgeImporter](https://github.com/chuongmep/UnityForgeImporter)
- [forge-bucketsmanager-desktop](https://github.com/Autodesk-Forge/forge-bucketsmanager-desktop)

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.