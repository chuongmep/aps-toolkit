using APSToolkit.Auth;
using Autodesk.Forge;
using NUnit.Framework;

namespace ForgeToolkitUnit;

public class VersionTest
{
    public Token Token { get; set; }
    [SetUp]
    public void Setup()
    {
        Token = Authentication.Get2LeggedToken().Result;
    }

    [Test]
    [TestCase("b.ca790fb5-141d-4ad5-b411-0461af2e9748","urn:adsk.wipprod:dm.lineage:HX2O7xKUQfukJ_hgHsrX_A")]
    // [TestCase("b.ca790fb5-141d-4ad5-b411-0461af2e9748", "urn:adsk.wipprod:fs.file:vf.HX2O7xKUQfukJ_hgHsrX_A",32,32)]
    public void TestGetInfoVersion(string projectId,string itemid)
    {
        VersionsApi versionsApi = new VersionsApi();
        versionsApi.Configuration.AccessToken = Token.access_token;
        ItemsApi itemsApi = new ItemsApi();
        itemsApi.Configuration.AccessToken = Token.access_token;
        dynamic versions = itemsApi.GetItemVersions(projectId, itemid);
        string versionId = versions.data[0].id;
        var version = versionsApi.GetVersion(projectId, versionId);
        string modelName = version.data.attributes.displayName;
        dynamic latestVersion = version.data.attributes.versionNumber;
        Assert.IsNotEmpty(versionId);
        Assert.IsNotEmpty(version);
        Assert.IsNotEmpty(modelName);
        Assert.IsNotEmpty(latestVersion);
    }
}