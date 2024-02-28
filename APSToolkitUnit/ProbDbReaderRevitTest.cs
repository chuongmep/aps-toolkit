using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using APSToolkit.Auth;
using APSToolkit.Database;
using APSToolkit.Utils;
using NUnit.Framework;

namespace ForgeToolkitUnit;

[TestFixture]
public class ProbDbReaderRevitTest
{
    private PropDbReaderRevit RevitPropDbReader { get; set; }
    [SetUp]
    public void Setup()
    {
        Settings.Token2Leg = Authentication.Get2LeggedToken().Result;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        string ids ="Resources/objects_ids.json.gz";
        string offsets = "Resources/objects_offs.json.gz";
        string avs = "Resources/objects_avs.json.gz";
        string attrs = "Resources/objects_attrs.json.gz";
        string values = "Resources/objects_vals.json.gz";
        RevitPropDbReader = new PropDbReaderRevit(ids,offsets,avs,attrs,values);
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Console.WriteLine($"Time elapsed seconds: {elapsedMs / 1000}");
    }

    [Test]
    [TestCaseSource(typeof(Settings), nameof(Settings.RevitTestUrn))]
    public void GetAllCategoriesTest(string urn)
    {
        RevitPropDbReader = new PropDbReaderRevit(urn, Settings.Token2Leg);
        var categories = RevitPropDbReader.GetAllCategories();
        categories.ExportToCsv("result.csv");
        Assert.AreNotEqual(0, categories.Count);
    }
    [Test]
    [TestCaseSource(typeof(Settings), nameof(Settings.RevitTestUrn))]
    public void TestExportAllDataToExcel(string urn)
    {
        RevitPropDbReader = new PropDbReaderRevit(urn, Settings.Token2Leg);
        RevitPropDbReader.ExportAllDataToExcel("result.xlsx");
    }
    [Test]
    [TestCaseSource(typeof(Settings), nameof(Settings.RevitTestUrn))]
    public void TestExportAllDataToParquet(string urn)
    {
        RevitPropDbReader = new PropDbReaderRevit(urn, Settings.Token2Leg);
        string dir = "./parquet";
        if (!System.IO.Directory.Exists(dir))
        {
            System.IO.Directory.CreateDirectory(dir);
        }
        RevitPropDbReader.ExportAllDataToParquet(dir);
    }
    [Test]
    [TestCaseSource(typeof(Settings), nameof(Settings.RevitRealUrn))]
    public void TestGetAllDataByParameter(string urn)
    {
        RevitPropDbReader = new PropDbReaderRevit(urn, Settings.Token2Leg);
        List<string> parameters = new List<string>()
        {
            "Category",
            "ElementId",
            "name",
            "Level",
        };
        DataTable dataTable = RevitPropDbReader.GetAllDataByParameter(parameters);
        dataTable.ExportDataToExcel("result.xlsx");
    }
    [Test]
    [TestCase("Generic Models")]
    [TestCase("Doors")]
    public void TestExportAllDataToExcelByCategory(string category)
    {
        RevitPropDbReader = new PropDbReaderRevit(Settings._RevitTestUrn, Settings.Token2Leg);
        RevitPropDbReader.ExportAllDataToExcelByCategory("result.xlsx",category,category);
    }
    [Test]
    [TestCase("Doors")]
    public void ExportToParquetStreamTest(string category)
    {
        RevitPropDbReader = new PropDbReaderRevit(Settings._RevitTestUrn, Settings.Token2Leg);
        DataTable dataTable = RevitPropDbReader.GetAllDataByCategory(category);
        byte[] bytes = dataTable.ExportToParquetStream();
        string filePath = "result.parquet";
        System.IO.File.WriteAllBytes(filePath,bytes);
    }

    // [Test]
    // [TestCase("result.xlsx","Generic Models")]
    // public void TestExcelSpacing(string fileName,string sheetName)
    // {
    //     DataTable table = ExcelUtils.ReadDataFromExcelToDataTable(fileName,sheetName);
    //     // just export data not include column name
    //     DataTable fixColumnNames = table.FixColumnNames();
    //     fixColumnNames.ExportToParquet("result.parquet");
    // }

    [Test]
    [TestCaseSource(typeof(Settings), nameof(Settings.RevitTestUrn))]
    public void GetAllFamiliesTest(string urn)
    {
        var families = RevitPropDbReader.GetAllFamilies();
        families.ExportToCsv("result.csv");
        Assert.AreNotEqual(0, families.Count);
    }
    [Test]
    [TestCaseSource(typeof(Settings), nameof(Settings.RevitTestUrn))]
    public void GetAllFamilyTypesTest(string urn)
    {
        var families = RevitPropDbReader.GetAllFamilyTypes();
        families.ExportToCsv("result.csv");
        Assert.AreNotEqual(0, families.Count);
    }
    [Test]
    [TestCase("Furniture")]
    [TestCase("Rooms")]
    [TestCase("Doors")]
    public void GetDataByCategoryTest(string category)
    {
        RevitPropDbReader = new PropDbReaderRevit(Settings._RevitTestUrn, Settings.Token2Leg);
        DataTable dataTable = RevitPropDbReader.GetAllDataByCategory(category);
        dataTable.ExportDataToExcel("result.xlsx");
        Assert.AreNotEqual(0, dataTable.Rows);
    }

    [Test]
    [TestCase("Basic Wall")]
    public void GetDataByFamilyTest(string familyName)
    {
        RevitPropDbReader = new PropDbReaderRevit(Settings._RevitTestUrn, Settings.Token2Leg);
        DataTable dataTable = RevitPropDbReader.GetAllDataByFamily(familyName);
        dataTable.ExportToCsv("result.csv");
        Assert.AreNotEqual(0, dataTable.Rows);
    }
    [Test]
    [TestCase("Sheet")]
    public void GetAllDataByFamilyTypeTest(string typeName)
    {
        RevitPropDbReader = new PropDbReaderRevit(Settings._RevitTestUrn, Settings.Token2Leg);
        DataTable dataTable = RevitPropDbReader.GetAllDataByFamilyType(typeName);
        dataTable.ExportToCsv("result.csv");
        Assert.AreNotEqual(0, dataTable.Rows);
    }

    [Test]
    [TestCase("Furniture")]
    [TestCase("Generic Models")]
    public void GetDataByCategoryParquetTest(string category)

    {
        RevitPropDbReader = new PropDbReaderRevit(Settings._RevitTestUrn, Settings.Token2Leg);
        DataTable dataTable = RevitPropDbReader.GetAllDataByCategory(category);
        dataTable.ExportToParquet("result.parquet");
        Assert.AreNotEqual(0, dataTable.Rows);
    }
    [Test]
    [TestCase("Rooms",new string[]{"Name","Number","Area","Volume","Workset","Level","Comments"})]
    [TestCase("Doors",new string[]{"name","ExternalId","Category","Area","Volume","Workset","Level","Comments"})]
    public void GetDataByCategoryAndParametersTest(string category,string[] parameterNames)

    {
        RevitPropDbReader = new PropDbReaderRevit(Settings._RevitTestUrn, Settings.Token2Leg);
        RevitPropDbReader.Configuration.IsGetBBox = false;
        DataTable dataTable = RevitPropDbReader.GetDataByCategoryAndParameters(category,parameterNames.ToList());
        dataTable.ExportToCsv("result.csv");
        Assert.AreNotEqual(0, dataTable.Rows);
    }
    [Test]
    [TestCase(new []{"Revit Rooms","Doors"},new string[]{"ExternalId","Name","Category","Number","Area","Volume","Workset","Level","Comments"})]
    public void GetDataByCategoriesAndParametersTest(string[] category,string[] parameterNames)

    {
        RevitPropDbReader = new PropDbReaderRevit(Settings._RevitTestUrn, Settings.Token2Leg);
        RevitPropDbReader.Configuration.IsGetBBox = false;
        DataTable dataTable = RevitPropDbReader.GetDataByCategoriesAndParameters(category.ToList(),parameterNames.ToList());
        dataTable.ExportToCsv("result.csv");
        Assert.AreNotEqual(0, dataTable.Rows);
    }

    [Test]
    [TestCase(Settings._RevitTestUrn)]
    public void GetAllDataTest(string urn)
    {
        RevitPropDbReader = new PropDbReaderRevit(urn, Settings.Token2Leg);
        var start = DateTime.Now;
        Dictionary<int, string> allCategories = RevitPropDbReader.GetAllCategories();
        foreach (KeyValuePair<int,string> cate in allCategories)
        {
            Console.WriteLine($"Processing category: {cate.Value}");
            DataTable dataTable = RevitPropDbReader.GetAllDataByCategory(cate.Value);
            Assert.AreNotEqual(0, dataTable.Rows);
        }
        var end = DateTime.Now;
        Console.WriteLine($"Time elapsed seconds: {(end - start).TotalSeconds}");
    }


}