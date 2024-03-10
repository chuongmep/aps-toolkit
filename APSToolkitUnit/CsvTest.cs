using System.Collections.Generic;
using NUnit.Framework;

namespace ForgeToolkitUnit;

public class CsvTest
{
    public void Setup()
    {
    }

    [Test]
    public void ExportFromObjectTest()
    {
        List<object> data = new List<object>();
        // try add data
        data.Add(new { Name = "Chuong", Age = 30 });
        data.Add(new { Name = "Chuong1", Age = 31 });
        data.Add(new { Name = "Chuong2", Age = 32 });
        data.Add(new { Name = "Chuong3", Age = 34 });
        APSToolkit.Utils.CsvHelper.ExportToCsv(data,"result.csv");
    }

    [Test]
    public void ExportFromDatatableTest()
    {
        var dataTable = new System.Data.DataTable();
        dataTable.Columns.Add("Name", typeof(string));
        dataTable.Columns.Add("Age", typeof(int));
        dataTable.Rows.Add("Chuong", 30);
        dataTable.Rows.Add("Chuong1", 31);
        dataTable.Rows.Add("Chuong2", 32);
        dataTable.Rows.Add("Chuong3", 34);
        APSToolkit.Utils.CsvHelper.ExportToCsv(dataTable,"result.csv");
    }
    [Test]
    public void ReadFromCsvTest()
    {
        var data = APSToolkit.Utils.CsvHelper.ReadFromCsv<object>("result.csv");
        Assert.IsNotNull(data);
    }
}