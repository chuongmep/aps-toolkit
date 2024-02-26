using System;
using System.Threading.Tasks;
using APSToolkit.Auth;
using APSToolkit.Database;
using Autodesk.Forge.Model;
using NUnit.Framework;

namespace ForgeToolkitUnit;

public class BucketTest
{
    [SetUp]
    public void Setup()
    {
        Settings.Token2Leg = Authentication.Get2LeggedToken().Result;
    }
    [Test]
    [TestCase("test_data")]
    public void CreateBucketTest(string bucketName)
    {
        BucketStorage bucketStorage = new BucketStorage();
        var bucket = bucketStorage.CreateBucket(Settings.Token2Leg, bucketName,"US",PostBucketsPayload.PolicyKeyEnum.Temporary);
        Assert.IsNotNull(bucket);
    }
    [Test]
    [TestCase("test_data","Resources/model.rvt")]
    public void UploadFileToBucket(string bucketName,string filePath)
    {
        BucketStorage bucketStorage = new BucketStorage();
        var bucket = bucketStorage.UploadFileToBucket(Settings.Token2Leg, bucketName,filePath);
        Assert.IsNotNull(bucket);
    }
    [Test]
    [TestCase("test_data","model_rvt.sdb")]
    public void GetFile(string bucketName,string filePath)
    {
        BucketStorage bucketStorage = new BucketStorage();
        var bucket = bucketStorage.GetFileFromBucket(Settings.Token2Leg, bucketName,filePath);
        Assert.IsNotNull(bucket);
    }
    [Test]
    [TestCase("test_data","model.rvt")]
    public void GetFileSignedUrl(string bucketName,string filePath)
    {
        BucketStorage bucketStorage = new BucketStorage();
        var url = bucketStorage.GetFileSignedUrl(Settings.Token2Leg, bucketName,filePath);
        Console.WriteLine(url);
        Assert.IsNotNull(url);
    }
    [Test]
    [TestCase("test_data","model_rvt.sdb")]
    public void DeleteFile(string bucketName,string filePath)
    {
        BucketStorage bucketStorage = new BucketStorage();
        bucketStorage.DeleteFile(Settings.Token2Leg, bucketName,filePath);
    }
    [Test]
    [TestCase("test_data")]
    public void Delete(string bucketName)
    {
        BucketStorage bucketStorage = new BucketStorage();
        bucketStorage.DeleteBucket(Settings.Token2Leg, bucketName);
    }
}