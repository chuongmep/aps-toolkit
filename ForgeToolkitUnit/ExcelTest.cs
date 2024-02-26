using System.Collections.Generic;
using APSToolkit.DesignAutomation;
using APSToolkit.Utils;
using NUnit.Framework;

namespace ForgeToolkitUnit;

public class ExcelTest
{
    [Test]
    public void Setup()
    {

    }
    [Test]
    public void ReadExcelTest()
    {
        string excelPath = @"./Resources/Result.xlsx";
        object[][] data = ExcelUtils.ReadDataFromExcel(excelPath,"Walls");
        Assert.IsNotEmpty(data);
    }
    [Test]
    public void ReadExcelByColumnNameTest()
    {
        string excelPath = @"./Resources/Result.xlsx";
        List<string> data = ExcelUtils.ReadDataByColumnName(excelPath,"Walls","Assembly Code");
        Assert.IsNotEmpty(data);
    }
    [Test]
    public void ReadParamsTest()
    {
        string excelPath = @"./Resources/Result.xlsx";
        List<Params> data = ExcelUtils.ReadParams(excelPath,"Walls","Assembly Code");
        Assert.IsNotEmpty(data);
    }
}