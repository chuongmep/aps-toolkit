using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Threading.Tasks;
using APSToolkit;
using APSToolkit.DesignAutomation;
using Autodesk.Authentication.Model;
using Autodesk.Forge;
using Autodesk.Forge.DesignAutomation.Model;
using NUnit.Framework;
using Alias = APSToolkit.DesignAutomation.Alias;
using Engine = APSToolkit.DesignAutomation.Engine;
using Version = APSToolkit.DesignAutomation.Version;

namespace ForgeToolkitUnit;

[TestFixture]
public class DesignAutomationTest
{

    [SetUp]
    public void Setup()
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
    }

    [TestCase("b.ca790fb5-141d-4ad5-b411-0461af2e9748", "urn:adsk.wipprod:fs.file:vf.HX2O7xKUQfukJ_hgHsrX_A", 35, 35)]
    [Test]
    public async Task RevitDesignAutomateRoomTest(string projectId, string urn, int startVersion, int endVersion)
    {
        int currentVersion = startVersion;
        for (int i = startVersion; i <= endVersion; i++)
        {
            currentVersion = i;
            string versionId = urn + "?version=" + currentVersion;
            DesignAutomateConfiguration designAutomateConfiguration = new DesignAutomateConfiguration()
            {
                AppName = "ExtractRoomAppNew",
                NickName = "chuong",
                Version = Version.v2022,
                Engine = Engine.Revit,
                Alias = Alias.DEV,
                ActivityName = "ExtractRoomAppNewActivity",
                ActivityDescription = "Extract Room Activity in Revit Model",
                PackageZipPath = @"D:\API\Forge\RoomExtraction\plugin\RoomExtractor.zip",
                BundleDescription = "Extract Room in Revit Model",
                ResultFileName = "result",
                ResultFileExt = ".json"
            };
            RevitDesignAutomate revitDesignAutomate = new RevitDesignAutomate(designAutomateConfiguration);
            string callback = "https://webhook.site/6383331c-a324-44dc-b3ae-0e2587e80653";
            Status executeJob = await revitDesignAutomate.ExecuteJob(projectId, versionId, callback);
            Assert.IsTrue(executeJob == Status.Success);
        }
    }

    [TestCase("f10b5c85-fd34-435a-9206-e4a8c21d761c", "3ced2f08-0682-4482-91ec-336f7633284e",
        "Resource\\DataSetParameterAddIn.zip")]
    [Test]
    public async Task SetDataParametersTest(string projectGuid, string modelGuid, string bundlePath)
    {
        DesignAutomateConfiguration configuration = new DesignAutomateConfiguration()
        {
            AppName = "SetDataParameterApp",
            NickName = "chuong",
            Version = Version.v2022,
            Engine = Engine.Revit,
            Alias = Alias.DEV,
            ActivityName = "SetDataParameterActivity",
            ActivityDescription = "Update Metadata Activity in Revit Model",
            PackageZipPath = bundlePath,
            BundleDescription = "Update Metadata in Revit Model",
            ResultFileName = "result",
            ResultFileExt = ".txt"
        };
        RevitSetDataAutomate revitExtractDataAutomate = new RevitSetDataAutomate(configuration);
        string callback = "https://webhook.site/f39a1eb7-8798-4ffa-96ec-72261db5d4de";
        List<Params> paramsList = new List<Params>()
        {
            new Params()
            {
                ElementId = "1323664",
                UniqueId = "d6e39c8c-2c2a-47d6-943b-7fe5ea8461ca-00143290",
                ParameterName = "Comments",
                ParameterValue = "Hello World 222323232"
            }
        };
        InputParams inputParams = new InputParams()
        {
            Region = "US",
            ProjectGuid = projectGuid,
            ModelGuid = modelGuid,
        };

        Token token =
            new Auth(configuration.ClientId, configuration.ClientSecret).Get2LeggedToken().Result;
        string forgeToken2Leg = token.AccessToken;
        if (string.IsNullOrEmpty(Settings.Token3Leg.AccessToken)) Assert.Fail("Can't use forgeToken3Leg .");
        Status executeJob =
            await revitExtractDataAutomate.ExecuteJob(forgeToken2Leg, Settings.Token3Leg.AccessToken, paramsList, inputParams,
                callback);
        Assert.IsTrue(executeJob == Status.Success);
    }

    [TestCase("b.ec0f8261-aeca-4ab9-a1a5-5845f952b17d", "urn:adsk.wipprod:fs.file:vf.Od8txDbKSSelToVg1oc1VA?version=13")]
    [Test]
    public async Task DynamoDesignAutomationTest(string projectId, string versionId)
    {
        string bundlePath =
            @"C:\Users\vho2\Downloads\aps-dynamo-design-automation-nodejs\aps-dynamo-design-automation-nodejs-master\bundles\D4DA.bundle.zip";
        string inputZipPath =
            @"C:\Users\vho2\Downloads\aps-dynamo-design-automation-nodejs\aps-dynamo-design-automation-nodejs-master\sample files\getting-data-from-revit\input.zip";
        DesignAutomateConfiguration configuration = new DesignAutomateConfiguration()
        {
            AppName = "TestDynamoRevitDA",
            NickName = "chuong",
            Version = Version.v2024,
            Engine = Engine.Revit,
            Alias = Alias.DEV,
            ActivityName = "TestDynamoRevitDAActivity",
            ActivityDescription = "TestDynamo Revit Design Automation",
            PackageZipPath = bundlePath,
            BundleDescription = "TestDynamo Revit Design Automation",
            ResultFileName = "result",
            ResultFileExt = ".zip"
        };
        DynamoRevitDesignAutomate dynamoRevitDesignAutomate = new DynamoRevitDesignAutomate(configuration);
        if (string.IsNullOrEmpty(Settings.Token3Leg.AccessToken)) Assert.Fail("Can't use forgeToken3Leg .");
        if (Settings.Token2Leg.AccessToken != null)
        {
            Status executeJob =
                await dynamoRevitDesignAutomate.ExecuteJob(Settings.Token2Leg.AccessToken, projectId, versionId,
                    inputZipPath);
            Assert.IsTrue(executeJob == Status.Success);
        }
    }
}