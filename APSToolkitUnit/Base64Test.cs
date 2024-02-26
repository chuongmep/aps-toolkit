using System;
using APSToolkit.Utils;
using NUnit.Framework;

namespace ForgeToolkitUnit;

public class Base64Test
{
    [Test]
    [TestCase("urn:adsk.wipprod:fs.file:vf.Od8txDbKSSelToVg1oc1VA?version=17")]
    public void ToBase64StringTest(string versionId)
    {
        string expected = "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0xNw==";
        string actual = Base64Convert.ToBase64String(versionId);
        Assert.AreEqual(expected, actual);
    }
    [Test]
    [TestCase("dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0xNw==")]
    public void FromBase64StringTest(string base64String)
    {
        string expected = "urn:adsk.wipprod:fs.file:vf.Od8txDbKSSelToVg1oc1VA?version=17";
        string actual = Base64Convert.FromBase64String(base64String);
        Assert.AreEqual(expected, actual);
    }
    [Test]
    [TestCase("urn:adsk.wipprod:fs.file:vf.Od8txDbKSSelToVg1oc1VA?version=26","dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0yNg")]
    [TestCase("urn:adsk.wipprod:fs.file:vf.DjXtlXoJQyS6D1R-gRhI8A?version=5","dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLkRqWHRsWG9KUXlTNkQxUi1nUmhJOEE_dmVyc2lvbj01")]
    public void VersionIdToUrnTest(string versionId,string expectation)
    {
        string actual = Base64Convert.ToBase64String(versionId);
        // just get 82 characters
        actual = actual.Substring(0, 82);
        Assert.AreEqual(expectation, actual);
    }

    [Test]
    [TestCase("dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLk9kOHR4RGJLU1NlbFRvVmcxb2MxVkE_dmVyc2lvbj0yNg","urn:adsk.wipprod:fs.file:vf.Od8txDbKSSelToVg1oc1VA?version=26")]
    public void UrnToVersionIdTest(string urn,string expectation)
    {
        // add 2 characters to make it 84 characters
        urn += "==";
        string actual = Base64Convert.FromBase64String(urn);
        Console.WriteLine(actual);
        Assert.AreEqual(expectation, actual);
    }
}