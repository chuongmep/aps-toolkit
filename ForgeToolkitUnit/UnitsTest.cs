using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using APSToolkit.Utils;
using Autodesk.Forge;
using NUnit.Framework;

namespace ForgeToolkitUnit;

public class UnitsTest
{
    private static string Token { get; set; }

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void GetAllUnits()
    {
        List<UnitsData> unitsDatas = UnitUtils.GetAllUnits();
        Assert.AreNotEqual(0, unitsDatas.Count);
    }

}