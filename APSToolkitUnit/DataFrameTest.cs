using System.Data;
using APSToolkit.Auth;
using APSToolkit.Utils;
using Microsoft.Data.Analysis;
using NUnit.Framework;

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
}