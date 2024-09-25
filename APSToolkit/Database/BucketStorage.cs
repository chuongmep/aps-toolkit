// Copyright (c) chuongmep.com. All rights reserved

using Autodesk.Forge;
using Autodesk.Forge.Model;

namespace APSToolkit.Database;

public class BucketStorage
{
    private Token? Token { get; set; }
    public BucketStorage(Token? token)
    {
        Token = token;
    }
    public BucketStorage()
    {
        var auth = new Auth();
        Token = auth.Get2LeggedToken().Result;
    }
    /// <summary>
    /// Creates a new bucket in Autodesk Forge Data Management service.
    /// </summary>
    /// <param name="bucketName">The name of the bucket to be created.</param>
    /// <param name="region">The region where the bucket should be created (default is "US").</param>
    /// <param name="Policy">The policy for the bucket (default is Transient).</param>
    /// <returns>A dynamic object representing the created bucket.</returns>
    /// <remarks>
    /// This method creates a new bucket in Autodesk Forge Data Management service using the specified access token,
    /// bucket name, region, and policy. The created bucket is returned as a dynamic object.
    /// </remarks>
    public dynamic CreateBucket(string bucketName, string region = "US",PostBucketsPayload.PolicyKeyEnum Policy=PostBucketsPayload.PolicyKeyEnum.Transient)
    {
        BucketsApi bucketsApi = new BucketsApi();
        bucketsApi.Configuration.AccessToken = Token.AccessToken;
        PostBucketsPayload postBuckets = new PostBucketsPayload(bucketName, null, Policy);
        dynamic bucket = bucketsApi.CreateBucket(postBuckets, region);
        return bucket;
    }
    /// <summary>
    /// Uploads a file to the specified bucket in Autodesk Forge Data Management service.
    /// If the specified bucket does not exist, it will be created before uploading the file.
    /// </summary>
    /// <param name="bucketKey">The key of the bucket where the file should be uploaded.</param>
    /// <param name="filePath">The path of the file to be uploaded.</param>
    /// <returns>A dynamic object representing the uploaded file.</returns>
    /// <remarks>
    /// This method checks if the specified bucket exists and creates it if not.
    /// It then uploads the specified file to the given bucket in Autodesk Forge Data Management service
    /// using the provided access token, bucket key, and file path. The uploaded file is returned as a dynamic object.
    /// </remarks>
    public dynamic UploadFileToBucket(string bucketKey, string filePath)
    {
        // check if bucket exists
        BucketsApi bucketsApi = new BucketsApi();
        bucketsApi.Configuration.AccessToken = Token.AccessToken;
        dynamic buckets = bucketsApi.GetBuckets();
        bool bucketExist = false;
        foreach (KeyValuePair<string, dynamic> bucket in new DynamicDictionaryItems(buckets.items))
        {
            if (bucket.Value.bucketKey == bucketKey)
            {
                bucketExist = true;
                break;
            }
        }
        if (!bucketExist)
        {
            CreateBucket(bucketKey);
        }
        ObjectsApi objectsApi = new ObjectsApi();
        objectsApi.Configuration.AccessToken = Token.AccessToken;
        dynamic file = objectsApi.UploadObject(bucketKey, Path.GetFileName(filePath),
            (int) new FileInfo(filePath).Length, new FileStream(filePath, FileMode.Open));
        return file;
    }

    /// <summary>
    /// Retrieves a file from the specified bucket in Autodesk Forge Data Management service as a MemoryStream.
    /// </summary>
    /// <param name="buketKey">The key of the bucket from which the file should be retrieved.</param>
    /// <param name="fileName">The name of the file to be retrieved.</param>
    /// <returns>A MemoryStream containing the contents of the retrieved file.</returns>
    /// <remarks>
    /// This method retrieves a file from the specified bucket in Autodesk Forge Data Management service
    /// using the provided access token, bucket key, and file name. The file contents are returned as a MemoryStream.
    /// </remarks>
    public MemoryStream  GetFileFromBucket(string buketKey,string fileName)
    {
        ObjectsApi objectsApi = new ObjectsApi();
        objectsApi.Configuration.AccessToken = Token.AccessToken;
        MemoryStream stream = objectsApi.GetObject(buketKey, fileName);
        return stream;
    }
    /// <summary>
    /// Deletes the specified bucket from Autodesk Forge Data Management service.
    /// </summary>
    /// <param name="bucketKey">The key of the bucket to be deleted.</param>
    /// <remarks>
    /// This method deletes the specified bucket from Autodesk Forge Data Management service
    /// using the provided access token and bucket key.
    /// </remarks>
    public void DeleteBucket(string bucketKey)
    {
        BucketsApi bucketsApi = new BucketsApi();
        bucketsApi.Configuration.AccessToken = Token.AccessToken;
        bucketsApi.DeleteBucket(bucketKey);
    }
    /// <summary>
    /// Deletes the specified file from the specified bucket in Autodesk Forge Data Management service.
    /// </summary>
    /// <param name="bucketKey">The key of the bucket from which the file should be deleted.</param>
    /// <param name="fileName">The name of the file to be deleted.</param>
    /// <remarks>
    /// This method deletes the specified file from the specified bucket in Autodesk Forge Data Management service
    /// using the provided access token, bucket key, and file name.
    /// </remarks>
    public void DeleteFile(string bucketKey,string fileName)
    {
        ObjectsApi objectsApi = new ObjectsApi();
        objectsApi.Configuration.AccessToken = Token.AccessToken;
        objectsApi.DeleteObject(bucketKey, fileName);
    }
    /// <summary>
    /// Retrieves a signed URL for the specified file in the specified bucket from Autodesk Forge Data Management service.
    /// </summary>
    /// <param name="token">The access token for authentication.</param>
    /// <param name="bucketKey">The key of the bucket containing the file.</param>
    /// <param name="fileName">The name of the file for which a signed URL is requested.</param>
    /// <returns>A string representing the signed URL for the specified file.</returns>
    /// <remarks>
    /// This method retrieves a signed URL for the specified file in the specified bucket from Autodesk Forge Data Management service
    /// using the provided access token, bucket key, and file name. The signed URL is returned as a string.
    /// </remarks>
    public string GetFileSignedUrl(string bucketKey,string fileName)
    {
        ObjectsApi objectsApi = new ObjectsApi();
        objectsApi.Configuration.AccessToken = Token.AccessToken;
        dynamic signedUrl = objectsApi.CreateSignedResource(bucketKey, fileName, new PostBucketsSigned(10));
        return signedUrl.signedUrl;
    }
}