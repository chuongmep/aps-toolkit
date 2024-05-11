using APSToolkit;
using APSToolkit.Schema;
using APSToolkit.Utils;
using NUnit.Framework;

namespace ForgeToolkitUnit;

public class SvfReaderTest
{
    [SetUp]
    public void Setup()
    {
        var auth = new Auth();
        Settings.Token2Leg = auth.Get2LeggedToken().Result;
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