using System;
using System.Collections.Generic;
using System.Data;
using System.Formats.Asn1;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APSToolkit;
using APSToolkit.BIM360;
using APSToolkit.Database;
using APSToolkit.DesignAutomation;
using APSToolkit.Utils;
using Autodesk.Authentication.Model;
using Autodesk.Forge;
using Autodesk.Forge.DesignAutomation.Model;
using Autodesk.Forge.Model;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using Alias = APSToolkit.DesignAutomation.Alias;
using Engine = APSToolkit.DesignAutomation.Engine;
using Version = APSToolkit.DesignAutomation.Version;

namespace ForgeToolkitUnit;

public class BIM360Test
{
    private BIM360 Bim360 { get; set; }
    [SetUp]
    public void SetUp()
    {
        var auth = new Auth();
        Settings.Token2Leg = auth.Get2LeggedToken().Result;
        var scopes = new List<Scopes>
        {
            Scopes.DataRead, Scopes.DataWrite, Scopes.DataCreate, Scopes.DataSearch, Scopes.BucketCreate,
            Scopes.BucketRead, Scopes.CodeAll,
            Scopes.BucketUpdate, Scopes.BucketDelete
        };
        var refreshToken = Environment.GetEnvironmentVariable("APS_REFRESH_TOKEN", EnvironmentVariableTarget.User);
        Assert.IsNotEmpty(refreshToken);
        Settings.Token3Leg = auth.Refresh3LeggedToken(refreshToken,scopes).Result;
        this.Bim360 =  new BIM360(Settings.Token3Leg);
    }

    [Test]
    public void GetHubsTest()
    {
        BIM360 bim360 = new BIM360();
        DynamicDictionaryItems dictionaryItems = bim360.GetHubs();
        Assert.IsNotEmpty(dictionaryItems);
    }

    [Test]
    [TestCase(Settings.HubId)]
    public void GetProjectsTest(string hubId)
    {
        BIM360 bim360 = new BIM360();
        DynamicDictionaryItems dictionary = bim360.GetProjects(hubId);
        Assert.IsNotEmpty(dictionary);
    }

    [Test]
    [TestCase(Settings.HubId, Settings.ProjectId)]
    public void TestGetTopFolders(string hubId, string projectId)
    {
        BIM360 bim360 = new BIM360();
        // can't use token 3 leg to get top folders, need to use token 2 leg
        Dictionary<string, string> dictionary = bim360.GetTopFolders(hubId, projectId);
        Assert.IsNotEmpty(dictionary);
    }
    [Test]
    [TestCase(Settings.HubId, Settings.ProjectId)]
    public void TestGetTopProjectFilesFolder(string hubId, string projectId)
    {
        BIM360 bim360 = new BIM360();
        // can't use token 3 leg to get top folders, need to use token 2 leg
        (string, string) result = bim360.GetTopProjectFilesFolder(hubId, projectId);
        Assert.IsNotEmpty(result.Item1);
        Assert.IsNotEmpty(result.Item2);
    }

    [Test]
    [TestCase(Settings.ProjectId, Settings.FolderId)]
    public void TestGetItemsFolders(string projectId, string folderId)
    {
        BIM360 bim360 = new BIM360();
        DynamicDictionaryItems items = bim360.GetItemsByFolder( projectId, folderId);
        Assert.IsNotEmpty(items);
    }

    [Test]
    [TestCase(Settings.HubId, "b.ec0f8261-aeca-4ab9-a1a5-5845f952b17d", Settings.FileName)]
    public void TestFindItemFilesByFileName(string hubId, string projectId,
        string fileName)
    {
        BIM360 bim360 = new BIM360();
        List<dynamic> items = bim360.GetItemsByFileName(hubId, projectId, fileName);
        Assert.IsNotEmpty(items);
    }

    [Test]
    [TestCase(Settings.ProjectId, Settings.ItemId)]
    public void GetItemVersionsTest(string projectId, string itemId)
    {
        BIM360 bim360 = new BIM360();
        var result = bim360.GetItemVersions(projectId, itemId);
        Assert.IsNotEmpty(result.ToString());
    }
    [Test]
    [TestCase(Settings.ProjectId, Settings.ItemId)]
    public void TestGetLatestVersionItem(string projectId, string itemId)
    {
        BIM360 bim360 = new BIM360();
        var result = bim360.GetLatestVersionItem(projectId, itemId);
        Assert.IsNotEmpty(result.ToString());
    }

    [Test]
    [TestCase(Settings.ProjectId, Settings.ItemId, 2)]
    public void GetDerivativesUrn(string projectId, string itemId, int version)
    {
        BIM360 bim360 = new BIM360();
        // Please note that input for itemID is lineageUrn not a resourceUrn like : urn:adsk.wipprod:fs.file:vf.Od8txDbKSSelToVg1oc1VA?version=26
        // In this case put file :
        // correct item urn : urn:adsk.wipprod:dm.lineage:Od8txDbKSSelToVg1oc1VA and version id 26
        var derivativesUrn = bim360.GetDerivativesUrn(projectId, itemId, 2);
        Assert.IsNotEmpty(derivativesUrn);
    }
    [Test]
    [TestCase(Settings._RevitTestUrn)]
    public void CheckStatusProcessingDataTest(string urn)
    {
        BIM360 bim360 = new BIM360();
        string status = bim360.CheckStatusProcessingData(urn);
        Console.WriteLine(status);
        Assert.IsNotEmpty(status);
    }

    [Test]
    [TestCase("b.ec0f8261-aeca-4ab9-a1a5-5845f952b17d","urn:adsk.wipprod:dm.lineage:Od8txDbKSSelToVg1oc1VA",28)]
    public void PipelineProcessTest(string projectId,string itemId,int version)
    {
        BIM360 bim360 = new BIM360();
        string urn = string.Empty;
        while (true)
        {
            string status = bim360.CheckStatusProcessingData(projectId, itemId, version);
            if (status != "complete")
            {
                Thread.Sleep(5000);
                continue;
            }
            urn = bim360.GetDerivativesUrn(projectId,itemId,version);
            Console.WriteLine(urn);
            break;
        }
        if(string.IsNullOrEmpty(urn)) Assert.Fail("Can't get urn");
        var auth = new Auth();
        var token =  auth.Get2LeggedToken().Result;
        PropDbReaderRevit propDbReaderRevit = new PropDbReaderRevit(urn,token);
        propDbReaderRevit.ExportAllDataToExcel("result.xlsx");
    }

    [Test]
    [TestCase(Settings.ProjectId,Settings.ItemId,2)]
    public void CheckStatusProcessingDataTest(string projectId,string itemId,int version)
    {
        BIM360 bim360 = new BIM360();
        string status = bim360.CheckStatusProcessingData(projectId,itemId,version);
        Console.WriteLine(status);
        Assert.IsNotEmpty(status);
    }

    [Test]
    [TestCase("b.ec0f8261-aeca-4ab9-a1a5-5845f952b17d", "urn:adsk.wipprod:fs.folder:co.2yCTHGmWSvSCzlaIzdrFKA", @"./Resources/model.rvt")]
    public void UploadFileToBIM360Test(string projectId, string folderUrn, string filePath)
    {
        BIM360 bim360 = new BIM360();
        Task<FileInfoInDocs> derivativesUrn = bim360.UploadFileToBIM360(projectId, folderUrn, filePath);
        Assert.IsNotEmpty(derivativesUrn.Result.VersionId);
    }

    [Test]
    [TestCase(Settings.ProjectId, Settings.FolderId,Settings.FileName)]
    public Task DownloadFileFromBIM360Test(string projectId, string folderUrn, string fileName)
    {
        BIM360 bim360 = new BIM360();
        var Auth = new Auth();
        string token = Auth.Get2LeggedToken().Result.AccessToken;
        string directory = "./";
        Task<string> derivativesUrn = bim360.DownloadFileFromBIM360(projectId, folderUrn, fileName, token, directory);
        Assert.IsNotEmpty(derivativesUrn.Result);
        return Task.CompletedTask;
    }

    [Test]
    [TestCase("b.ec0f8261-aeca-4ab9-a1a5-5845f952b17d", "urn:adsk.wipprod:fs.folder:co.2yCTHGmWSvSCzlaIzdrFKA", "result.xlsx")]
    public void ExportDataUploadBIM360Test(string projectId, string folderUrn, string filePath)
    {
        var RevitPropDbReader = new PropDbReaderRevit(Settings._RevitTestUrn, Settings.Token2Leg);
        string categoryName = "Walls";
        RevitPropDbReader.ExportAllDataToExcelByCategory(filePath, categoryName, categoryName);
        BIM360 bim360 = new BIM360();
        Task<FileInfoInDocs> derivativesUrn = bim360.UploadFileToBIM360(projectId, folderUrn, filePath);
        Assert.IsNotEmpty(derivativesUrn.Result.VersionId);
    }

    [Test]
    [TestCase(Settings.ProjectId, Settings.ItemId)]
    public async Task PublishModelTest(string projectId, string itemId)
    {
        BIM360 bim360 = new BIM360();
        RestResponse restResponse = bim360.PublishModel(Settings.Token3Leg.AccessToken, projectId, itemId);
        Assert.IsTrue(restResponse.IsSuccessful);
    }

    [Test]
    [TestCase(Settings.ProjectId, Settings.FolderId, Settings.FileName)]
    public void GetItemByFolder(string projectId, string folderId, string fileName)
    {
        BIM360 bim360 = new BIM360();
        dynamic item = bim360.GetItemByFolder(projectId, folderId, fileName);
        Assert.IsNotNull(item);
    }

    //MyHouse.RVT
    [Test]
    [TestCase(Settings.ProjectId, Settings.FolderId,
        "result.xlsx", Settings.ProjectGuid, Settings.ModelGuid,
        @"./Resources/DataSetParameterAddIn.zip", Settings.FileName)]
    public async Task TestDownloadExcelAndUpdateData(string projectId, string folderUrn, string fileName,
        string projectGuid,
        string modelGuid, string bundlePath, string modelName)
    {
        BIM360 bim360 = new BIM360();
        var Auth = new Auth();
        string token = Auth.Get2LeggedToken().Result.AccessToken;
        string directory = "./";
        Task<string> filePath = bim360.DownloadFileFromBIM360(projectId, folderUrn, fileName, token, directory);
        Assert.IsNotEmpty(filePath.Result);
        string excelPath = filePath.Result;
        List<Params> readParams = ExcelUtils.ReadParams(excelPath, "Walls", "Assembly Code");
        dynamic item = bim360.GetItemByFolder(projectId, folderUrn, modelName);
        string itemId = item.id;
        Version version = DAUtils.GetRevitVersionByItem(token, projectId, item).Result;
        DesignAutomateConfiguration configuration = new DesignAutomateConfiguration()
        {
            AppName = $"DemoSetDataParameterApp{version}",
            NickName = "chuong",
            Version = version,
            Engine = Engine.Revit,
            Alias = Alias.DEV,
            ActivityName = $"DemoSetDataParameterActivity{version}",
            ActivityDescription = "Update Metadata Activity in Revit Model",
            PackageZipPath = bundlePath,
            BundleDescription = "Update Metadata in Revit Model",
            ResultFileName = "result",
            ResultFileExt = ".txt"
        };
        RevitSetDataAutomate revitExtractDataAutomate = new RevitSetDataAutomate(configuration);
        string callback = "https://webhook.site/f39a1eb7-8798-4ffa-96ec-72261db5d4de";
        InputParams inputParams = new InputParams()
        {
            Region = "US",
            ProjectGuid = projectGuid,
            ModelGuid = modelGuid,
        };
        var clientID = Environment.GetEnvironmentVariable("APS_CLIENT_ID", EnvironmentVariableTarget.User);
        var clientSecret = Environment.GetEnvironmentVariable("APS_CLIENT_SECRET", EnvironmentVariableTarget.User);
        Scope[] scope = new Scope[]
            { Scope.DataRead, Scope.DataWrite, Scope.DataCreate, Scope.BucketRead, Scope.BucketCreate, Scope.CodeAll };
        string accessToken = Settings.Token3Leg.AccessToken;
        if (string.IsNullOrEmpty(accessToken)) Assert.Fail("Can't use forgeToken3Leg .");
        Status executeJob =
            await revitExtractDataAutomate.ExecuteJob(token, accessToken, readParams, inputParams,
                callback);
        Assert.IsTrue(executeJob == Status.Success);
        // publish model
        RestResponse restResponse = bim360.PublishModel(accessToken, projectId, itemId);
        Assert.IsTrue(restResponse.IsSuccessful);
    }

    [Test]
    [TestCase("ec0f8261-aeca-4ab9-a1a5-5845f952b17d","W0cQMm5bZyq9ngSyO5IMOA")]
    public void TestGetAllDataOriginalProperties(string projectId,string indexVersionId)
    {
        List<BIMObject> allProperties = Bim360.GetAllDataOriginalProperties(projectId,indexVersionId);
        Assert.IsNotEmpty(allProperties);
    }
    [Test]
    [TestCase("ec0f8261-aeca-4ab9-a1a5-5845f952b17d","W0cQMm5bZyq9ngSyO5IMOA")]
    public void TestGetAllDataByIndexVersionId(string projectId,string indexId)
    {
        BIMData[] allProperties = Bim360.GetAllDataByIndexVersionId(projectId,indexId);
        Assert.IsNotEmpty(allProperties);
    }
    [Test]
    [TestCase("ec0f8261-aeca-4ab9-a1a5-5845f952b17d","urn:adsk.wipprod:fs.file:vf.Od8txDbKSSelToVg1oc1VA?version=26")]
    public void TestGetAllDataByVersionId(string projectId,string versionId)
    {
        BIMData[] bimDatas = Bim360.GetAllDataByVersionId(projectId,versionId);
        BIMData bimData = bimDatas.FirstOrDefault(x => x.externalId == "5bb069ca-e4fe-4e63-be31-f8ac44e80d30-000471ee");
        bimData.properties.ExportToCsv("result.csv");
        Assert.IsNotEmpty(bimDatas);
    }

    [Test]
    [TestCase("1f7aa830-c6ef-48be-8a2d-bd554779e74b","urn:adsk.wipprod:fs.file:vf.0-bpmpJWQbSEEMuAZsUDMg?version=18")]
    [TestCase(Settings.ProjectId,Settings.VersionId)]
    public void BimDataToDataTableTest(string projectId,string versionId)
    {
        BIMData[] bimDatas = Bim360.GetAllDataByVersionId(projectId,versionId);
        DataTable dataTable = Bim360.BimDataToDataTable(bimDatas);
        dataTable.ExportDataToExcel("result2.xlsx");
        Assert.IsNotEmpty(dataTable.Rows);

    }
    [Test]
    [TestCase("ec0f8261-aeca-4ab9-a1a5-5845f952b17d","urn:adsk.wipprod:fs.file:vf.Od8txDbKSSelToVg1oc1VA?version=13")]
    public void TestBuildPropertyIndexesAsync(string projectId,string urnVersionId)
    {
        List<string> versions = new List<string>() { urnVersionId };
        var result = Bim360.BuildPropertyIndexesAsync(projectId,versions).Result;
        var indexs = result?.indexes;
        Assert.IsNotNull(indexs);
    }
    [Test]
    [TestCase("ec0f8261-aeca-4ab9-a1a5-5845f952b17d","urn:adsk.wipprod:fs.file:vf.Od8txDbKSSelToVg1oc1VA?version=13")]
    public void TestGetPropDbReader(string projectId,string urnVersionId)
    {
        BIM360 bim360 = new BIM360();
        PropDbReader result = bim360.GetPropDbReader(urnVersionId);
        Assert.AreNotEqual(0,result.attrs?.Length);
    }
    [Test]
    [TestCase("ec0f8261-aeca-4ab9-a1a5-5845f952b17d","urn:adsk.wipprod:fs.file:vf.Od8txDbKSSelToVg1oc1VA?version=21")]
    public void TestGetPropDbReaderRevit(string projectId,string urnVersionId)
    {
        BIM360 bim360 = new BIM360();
        PropDbReaderRevit result = bim360.GetPropDbReaderRevit(urnVersionId);
        Assert.AreNotEqual(0,result.attrs?.Length);
    }
    [Test]
    [TestCase("ec0f8261-aeca-4ab9-a1a5-5845f952b17d","4gGKMrakcPL47x6eou0Ufw")]
    public void TestGetPropertyIndexesStatusAsync(string projectId,string indexId)
    {
        var result = Bim360.GetPropertyIndexesStatusAsync(projectId,indexId).Result;
        Assert.IsNotEmpty(result);
    }
    [Test]
    [TestCase("ec0f8261-aeca-4ab9-a1a5-5845f952b17d","4gGKMrakcPL47x6eou0Ufw")]
    public void TestGetPropertyIndexesManifestAsync(string projectId,string indexId)
    {
        var result = Bim360.GetPropertyIndexesManifestAsync(projectId,indexId).Result;
        Assert.IsNotEmpty(result);
    }
    [Test]
    [TestCase("ec0f8261-aeca-4ab9-a1a5-5845f952b17d","4gGKMrakcPL47x6eou0Ufw")]
    public void TestGetPropertyFieldsAsync(string projectId,string indexId)
    {

        BIM360 bim360 = new BIM360(Settings.Token3Leg);
        var result = bim360.GetPropertyFieldsAsync(projectId,indexId).Result;
        Assert.IsNotEmpty(result);
    }
    [Test]
    [TestCase("ec0f8261-aeca-4ab9-a1a5-5845f952b17d","4gGKMrakcPL47x6eou0Ufw")]
    public void TestGetPropertiesResultsAsync(string projectId,string indexId)
    {
        BIM360 bim360 = new BIM360(Settings.Token3Leg);
        List<JObject> result = bim360.GetPropertiesResultsAsync(projectId,indexId).Result;
        Assert.IsNotEmpty(result);
    }
    [Test]
    [TestCase("ec0f8261-aeca-4ab9-a1a5-5845f952b17d","4gGKMrakcPL47x6eou0Ufw","result.parquet")]
    public void ExportRevitDataToParquet(string projectId,string indexId,string filePath)

    {
        var scopes = new List<Scopes>
        {
            Scopes.DataRead, Scopes.DataWrite, Scopes.DataCreate, Scopes.DataSearch, Scopes.BucketCreate,
            Scopes.BucketRead, Scopes.CodeAll,
            Scopes.BucketUpdate, Scopes.BucketDelete
        };
        var refreshToken = Environment.GetEnvironmentVariable("APS_REFRESH_TOKEN", EnvironmentVariableTarget.User);
        Assert.IsNotEmpty(refreshToken);
        Auth auth = new Auth();
        var token3Leg = auth.Refresh3LeggedToken(refreshToken,scopes).Result;
        BIM360 bim360 = new BIM360(token3Leg);
        bim360.ExportRevitDataToParquet(projectId,indexId,filePath);
    }
    [Test]
    [TestCase(Settings._RevitTestUrn)]
    public void TestGetLevelsFromAecModelData(string urn)
    {
       var scopes = new List<Scopes>
        {
            Scopes.DataRead, Scopes.DataWrite, Scopes.DataCreate, Scopes.DataSearch, Scopes.BucketCreate,
            Scopes.BucketRead, Scopes.CodeAll,
            Scopes.BucketUpdate, Scopes.BucketDelete
        };
        var refreshToken = Environment.GetEnvironmentVariable("APS_REFRESH_TOKEN", EnvironmentVariableTarget.User);
        Assert.IsNotEmpty(refreshToken);
        Auth auth = new Auth();
        var token3Leg = auth.Refresh3LeggedToken(refreshToken!,scopes).Result;
        BIM360 bim360 = new BIM360(token3Leg);
        List<Level> result = bim360.GetLevelsFromAecModelData(urn).Result;
        Assert.IsNotEmpty(result);
    }
    [Test]
    [TestCase("result.xlsx",Settings.VersionId)]
    public void ExportRevitDataToExcel(string filePath,string versionId)
    {
        BIM360 bim360 = new BIM360();
        bim360.ExportRevitDataToExcel(filePath,versionId);
    }
    [Test]
    [TestCase(Settings.HubId,Settings.ProjectId)]
    public void TestBatchExportAllRevitToExcel(string hubId,string projectId)
    {
        BIM360 bim360 = new BIM360();
        string directory = "./output";
        bim360.BatchExportAllRevitToExcel(directory, hubId, projectId,true);
    }
    [Test]
    [TestCase(Settings.ProjectId,Settings.FolderId)]
    public void TestBatchExportAllRevitToExcelByFolder(string projectId,string folderId)
    {
        BIM360 bim360 = new BIM360();
        string directory = @"./output";
        bim360.BatchExportAllRevitToExcelByFolder(directory, projectId,folderId,true);
    }

    [Test]
    [TestCase("b.1f7aa830-c6ef-48be-8a2d-bd554779e74b","urn:adsk.wipprod:fs.folder:co.dEsE_6gCT6q0Kz7cRSGx0w")]
    public void BatchReportItemsTest(string projectId,string folderId)
    {
        BIM360 bim360 = new BIM360();
        DataTable dataTable = bim360.BatchReportItems(projectId,folderId,".rvt",true);
        dataTable.ExportToCsv(@"result.csv");
    }
    [Test]
    [TestCase("b.1f7aa830-c6ef-48be-8a2d-bd554779e74b","urn:adsk.wipprod:dm.lineage:NxImvLT2T0yiFi0yZW_cKw")]
    public void BatchReportItemVersionsTest(string projectId,string itemId)
    {
        BIM360 bim360 = new BIM360();
        DataTable dataTable = bim360.BatchReportItemVersions(projectId,itemId);
        dataTable.ExportToCsv(@"result.csv");
    }
}