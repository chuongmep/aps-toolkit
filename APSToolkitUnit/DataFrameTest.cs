using System.Data;
using System.Data.Common;
using APSToolkit.Auth;
using APSToolkit.Utils;
using Microsoft.Data.Analysis;
using NUnit.Framework;
using DataFrame = Microsoft.Data.Analysis.DataFrame;

namespace ForgeToolkitUnit;

public class DataFrameTest
{
    [SetUp]
    public void SetUp()
    {

    }
    [Test]
    public void DataTableToDataFrame()
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("Data", typeof(byte[]));
        dt.Rows.Add(1, "Name1", new byte[] { 1, 2, 3 });
        dt.Rows.Add(2, "Name2", new byte[] { 4, 5, 6 });
        dt.Rows.Add(3, "Name3", new byte[] { 7, 8, 9 });
        DataFrame dataFrame = dt.ToDataFrame();
        Assert.AreEqual(3, dataFrame.Rows.Count);
        Assert.AreEqual(3, dataFrame.Columns.Count);
    }

    [Test]
    public void LoadFromDataTableTest()
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("Data", typeof(byte[]));
        dt.Rows.Add(1, "Name1", new byte[] { 1, 2, 3 });
        dt.Rows.Add(2, "Name2", new byte[] { 4, 5, 6 });
        dt.Rows.Add(3, "Name3", new byte[] { 7, 8, 9 });
        DataFrame dataFrame = APSToolkit.Utils.DataFrame.LoadFromDataTable(dt);
        Assert.AreEqual(3, dataFrame.Rows.Count);
        Assert.AreEqual(3, dataFrame.Columns.Count);
    }
    [Test]
    [TestCase("./Resources/result.parquet")]
    public void LoadFromParquetTest(string parquet)
    {
        DataFrame dataFrame = APSToolkit.Utils.DataFrame.LoadFromParquet(parquet);
        Assert.AreEqual(3, dataFrame.Rows.Count);
    }

    [Test]
    [TestCase("./Resources/result.xlsx","Walls")]
    public void LoadFromExcelTest(string excelPath,string sheetName)
    {
        DataFrame dataFrame = APSToolkit.Utils.DataFrame.LoadFromExcel(excelPath,sheetName);
        Assert.AreNotEqual(0, dataFrame.Rows.Count);
        Assert.AreNotEqual(0, dataFrame.Columns.Count);
    }
}