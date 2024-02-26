using APSToolkit;
using APSToolkit.Auth;
using APSToolkit.Schema;
using APSToolkit.Utils;
using NUnit.Framework;

namespace ForgeToolkitUnit;

public class SvfReaderTest
{
    [SetUp]
    public void Setup()
    {
        Settings.Token2Leg = Authentication.Get2LeggedToken().Result;
    }

    [Test]
    public void ReadSvfTest()
    {
        string path = @"\MyRoom.svf";
        ISvfContent svfContent = SvfReader.ReadSvf(path);
        svfContent.fragments.ExportToCsv("result.csv");
        Assert.AreNotEqual(0, svfContent.fragments.Length);
        Assert.AreNotEqual(0, svfContent.geometries.Length);
    }
}