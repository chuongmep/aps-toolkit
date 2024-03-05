using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using APSToolkit.Auth;
using APSToolkit.Database;
using APSToolkit.Utils;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ForgeToolkitUnit;

[TestFixture]
public class ProbDbReaderTest
{
    private PropDbReader PropDbReader { get; set; }
    [SetUp]
    public void Setup()
    {
        Settings.Token2Leg = Authentication.Get2LeggedToken().Result;
        // start wath time
        var watch = System.Diagnostics.Stopwatch.StartNew();
        // PropDbReader = new PropDbReader(Settings._RevitUrn, Settings.Token2Leg);
        string ids ="Resources/objects_ids.json.gz";
        string offsets = "Resources/objects_offs.json.gz";
        string avs = "Resources/objects_avs.json.gz";
        string attrs = "Resources/objects_attrs.json.gz";
        string values = "Resources/objects_vals.json.gz";
        PropDbReader = new PropDbReader(ids,offsets,avs,attrs,values);
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Console.WriteLine($"Time elapsed seconds: {elapsedMs / 1000}");
    }

    [Test]
    public void PropDbReaderSQLTest()
    {
        Assert.AreNotEqual(0, PropDbReader.avs!.Length);
        Assert.AreNotEqual(0, PropDbReader.offsets!.Length);
        Assert.AreNotEqual(0, PropDbReader.ids!.Length);
        Assert.AreNotEqual(0, PropDbReader.vals!.Length);
    }

    [Test]
    public void GetPropertiesTest()
    {
        Dictionary<string, string> properties = PropDbReader.GetAllProperties(3);
        properties.ExportDataToExcel(@"result.xlsx");
        Assert.AreNotEqual(0, properties.Count);
    }

    [Test]
    public void GetPublicPropertiesTest()
    {
        Dictionary<string, string> properties = PropDbReader.GetPublicProperties(3);
        properties.ExportDataToExcel(@"result.xlsx");
        Assert.AreNotEqual(0, properties.Count);
    }

    [Test]
    public void EnumeratePropertiesTest()
    {
        // cate furniture
        int dbid = 3527;
        Property[] properties = PropDbReader.EnumerateProperties(dbid);
        properties.ExportToCsv(@"result.csv");
        Assert.AreNotEqual(0, properties.Length);
    }

    [Test]
    public void EnumerateFullRevitPropertiesTest()
    {
        int dbId = 3829;
        List<KeyValuePair<string, string>> properties = PropDbReader.GetPublicProperties(dbId).ToList();
        int instanceOf = PropDbReader.GetInstanceOf(dbId).FirstOrDefault();
        if (instanceOf != 0)
        {
            properties.AddRange(PropDbReader.GetPublicProperties(instanceOf));
        }

        string name = PropDbReader.GetName(dbId);
        if (!string.IsNullOrEmpty(name))
        {
            properties.Add(new KeyValuePair<string, string>("Name", name));
            string elementId = PropDbReader.GetElementId(dbId);
            if (!string.IsNullOrEmpty(elementId))
            {
                properties.Add(new KeyValuePair<string, string>("ElementId", elementId));
            }
        }

        var guid = PropDbReader.GetExternalId(dbId);
        if (!string.IsNullOrEmpty(guid))
        {
            properties.Add(new KeyValuePair<string, string>("UniqueId", guid));
        }

        List<KeyValuePair<string, string>> keyValuePairs = properties.OrderBy(x => x.Key).ToList();
        // bring elementid and uniqueid to top
        int index = keyValuePairs.FindIndex(x => x.Key == "UniqueId");
        if (index != -1)
        {
            var uniqueId = keyValuePairs[index];
            keyValuePairs.RemoveAt(index);
            keyValuePairs.Insert(0, uniqueId);
        }
        index = keyValuePairs.FindIndex(x => x.Key == "ElementId");
        if (index != -1)
        {
            var elementId = keyValuePairs[index];
            keyValuePairs.RemoveAt(index);
            keyValuePairs.Insert(0, elementId);
        }

        keyValuePairs.ExportDataToExcel(@"result.xlsx");
        Assert.AreNotEqual(0, properties.Count);
    }

    [Test]
    public void ExportDataToExcelTest()
    {
        PropDbReader.ExportDataToExcel(@"result.xlsx","Metadata");
    }
    [Test]
    public void GetAllPropertiesTest()
    {
        Property[] byCategory = PropDbReader.GetAllProperties();
        Assert.AreNotEqual(0, byCategory.Length);
    }
    [Test]
    public void GetPropertiesByCategoryTest()
    {
        int dbId = 1;
        Dictionary<string,List<KeyValuePair<string,object>>> byCategory = PropDbReader.GetPropertiesByCategory(dbId);
        Assert.AreNotEqual(0, byCategory.Count);
    }

    [Test]
    public void GetDataByDbidTest()
    {
        int dbId = 1;
        DataTable byCategory = PropDbReader.GetDataByDbId(dbId);
        Assert.AreNotEqual(0, byCategory.Rows.Count);
    }


}