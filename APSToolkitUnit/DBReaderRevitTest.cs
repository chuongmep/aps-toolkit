using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using APSToolkit.Auth;
using APSToolkit.Database;
using APSToolkit.Utils;
using NUnit.Framework;

namespace ForgeToolkitUnit;

[TestFixture]
public class DBReaderRevitTest
{
    [SetUp]
    public void Setup()
    {
        Settings.Token2Leg = Authentication.Get2LeggedToken().Result;
    }

    [Test]
    [TestCase("Resources/model_rvt.sdb")]
    public Task GetAllCategories(string urn)
    {
        DbReaderRevit dbReaderRevit = new DbReaderRevit(urn);
        var categories = dbReaderRevit.GetAllCategories();
        categories.ExportToCsv("result.csv");
        Assert.AreNotEqual(0, categories.Count);
        return Task.CompletedTask;
    }
    [Test]
    [TestCase("Resources/model_rvt.sdb","IFC Parameters")]
    public Task GetValueByPropertyCategoryTest(string urn,string category)
    {
        DbReaderRevit dbReaderRevit = new DbReaderRevit(urn);
        List<Property> properties = dbReaderRevit.GetPropertyByCategory(category);
        Assert.AreNotEqual(0, properties.Count);
        properties.ExportToCsv("result.csv");
        return Task.CompletedTask;
    }
    [Test]
    [TestCase("Resources/model_rvt.sdb","Comments")]
    public Task GetElementByParameterNameTest(string urn,string praName)
    {
        DbReaderRevit dbReaderRevit = new DbReaderRevit(urn);
        var properties = dbReaderRevit.GetPropertyByName(praName);
        Assert.AreNotEqual(0, properties.Count);
        properties.ExportToCsv("result.csv");
        return Task.CompletedTask;
    }
    [Test]
    [TestCase("Resources/model_rvt.sdb","Doors")]
    public Task GetElementsIdByRevitCategoryTest(string urn,string revitCategory)
    {
        DbReaderRevit dbReaderRevit = new DbReaderRevit(urn);
        IEnumerable<string> properties = dbReaderRevit.GetExternalIdByRevitCategory(revitCategory);
        var enumerable = properties as string[] ?? properties.ToArray();
        enumerable.ExportToCsv("result.csv");
        Assert.AreNotEqual(0, enumerable.Count());
        return Task.CompletedTask;
    }
    [Test]
    [TestCase("Resources/model_rvt.sdb","Furniture")]
    public Task GetPropertiesByRevitCategoryTest(string urn,string revitCategory)
    {
        DbReaderRevit dbReaderRevit = new DbReaderRevit(urn);
        IEnumerable<Property> properties = dbReaderRevit.GetPropertiesByRevitCategory(revitCategory,true);
        var enumerable = properties as Property[] ?? properties.ToArray();
        Assert.AreNotEqual(0, enumerable.Count());
        enumerable.ExportToCsv("result.csv");
        return Task.CompletedTask;
    }
    [Test]
    [TestCase("Resources/model_rvt.sdb","Furniture")]
    public Task GetPublicPropertiesByRevitCategoryTest(string urn,string revitCategory)
    {
        DbReaderRevit dbReaderRevit = new DbReaderRevit(urn);
        IEnumerable<Property> properties = dbReaderRevit.GetPublicPropertiesByRevitCategory(revitCategory,true);
        var enumerable = properties as Property[] ?? properties.ToArray();
        Assert.AreNotEqual(0, enumerable.Count());
        enumerable.ExportToCsv("result.csv");
        return Task.CompletedTask;
    }

    [Test]
    [TestCase("Resources/model_rvt.sdb")]
    public void GetAllPropertiesAndTypesByExternalIdTest(string dbPath)
    {
        DbReader dbReader = new DbReader(dbPath);
        string uniqueId = "5bb069ca-e4fe-4e63-be31-f8ac44e80d30-00046bfe";
        IEnumerable<Property> allProperties =
            dbReader.GetPublicPropertiesByExternalId(uniqueId);
        int dbId = dbReader.GetInstanceOf(uniqueId).FirstOrDefault();
        IEnumerable<Property> properties = dbReader.GetPublicPropertiesByDbId(dbId);
        var enumerable = allProperties as Property[] ?? allProperties.ToArray();
        enumerable.ToList().AddRange(properties);
        enumerable.ExportToCsv("result.csv");
        Assert.IsTrue(enumerable.Any());
    }

    [Test]
    // [TestCase("Resources/F10A.sdb","Mechanical Equipment")] // fast 8 second
    [TestCase("Resources/model_rvt.sdb","Walls")] // 2 second
    public Task ExportDataToCsvTest(string dbPath,string revitCategory)
    {
        DbReaderRevit dbReaderRevit = new DbReaderRevit(dbPath);
        dbReaderRevit.ExportDataToCsv(revitCategory,"result.csv",true);
        return Task.CompletedTask;
    }
    [Test]
    // [TestCase("Resources/F10A.sdb","Mechanical Equipment")] // fast 8 second
    [TestCase("Resources/model_rvt.sdb","Walls")] // 2 second
    public Task ExportDataToExcelTest(string dbPath,string revitCategory)
    {
        DbReaderRevit dbReaderRevit = new DbReaderRevit(dbPath);
        dbReaderRevit.ExportDataToExcel(revitCategory,"result.xlsx",revitCategory,true);
        return Task.CompletedTask;
    }
    [Test]
    // [TestCase("Resources/F10A.sdb")] // so  slow
    [TestCase("Resources/model_rvt.sdb")]
    public Task ExportAllDataToExcelTest(string dbPath)
    {
        DbReaderRevit dbReaderRevit = new DbReaderRevit(dbPath);
        dbReaderRevit.ExportAllDataToExcel("result.xlsx",true);
        return Task.CompletedTask;
    }
    [Test]
    // [TestCase("Resources/F10A.sdb")]
    [TestCase("Resources/model_rvt.sdb")]
    public Task GetAllDataGroupByRevitCategoryTest(string dbPath)
    {
        DbReaderRevit dbReaderRevit = new DbReaderRevit(dbPath);
        dbReaderRevit.GetAllDataGroupByRevitCategory(true);
        return Task.CompletedTask;
    }
}