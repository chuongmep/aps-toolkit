using System.Data;
using System.Net;
using APSToolkit.Database;
using APSToolkit.Utils;
using Autodesk.Forge;
using Autodesk.Forge.Client;
using Autodesk.Forge.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
// Copyright (c) chuongmep.com. All rights reserved

namespace APSToolkit.BIM360;

public class BIM360
{
    private static string Host = "https://developer.api.autodesk.com";

    /// <summary>
    /// Retrieves the hubs available for the specified access token in Autodesk BIM 360.
    /// </summary>
    /// <param name="AccessToken">The access token for authentication.</param>
    /// <returns>
    /// A <see cref="DynamicDictionaryItems"/> containing information about the available hubs.
    /// </returns>
    /// <remarks>
    /// This method utilizes the HubsApi to asynchronously retrieve the hubs available for the specified access token.
    /// It returns a <see cref="DynamicDictionaryItems"/> object containing detailed information about each hub.
    /// </remarks>
    /// <example>
    /// The following example demonstrates how to use the GetHubs method:
    /// <code>
    /// var accessToken = "your_access_token";
    /// var hubs = GetHubs(accessToken);
    /// foreach (var hubInfo in hubs)
    /// {
    ///     Console.WriteLine($"Hub ID: {hubInfo.Value.id}");
    ///     Console.WriteLine($"Hub Name: {hubInfo.Value.attributes.name}");
    ///     // Access more properties as needed...
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="HubsApi"/>
    /// <seealso cref="DynamicDictionaryItems"/>
    public DynamicDictionaryItems GetHubs(string AccessToken)
    {
        var hubsApi = new HubsApi();
        hubsApi.Configuration.AccessToken = AccessToken;
        dynamic result = hubsApi.GetHubsAsync().Result;
        return new DynamicDictionaryItems(result.data);
    }
    /// <summary>
    /// Retrieves the projects within a specified hub in Autodesk BIM 360.
    /// </summary>
    /// <param name="AccessToken">The access token for authentication.</param>
    /// <param name="hubId">The unique identifier of the hub.</param>
    /// <returns>
    /// A <see cref="DynamicDictionaryItems"/> containing information about the projects within the specified hub.
    /// </returns>
    /// <remarks>
    /// This method utilizes the ProjectsApi to asynchronously retrieve the projects within a specified hub.
    /// It returns a <see cref="DynamicDictionaryItems"/> object containing detailed information about each project.
    /// </remarks>
    /// <example>
    /// The following example demonstrates how to use the GetProjects method:
    /// <code>
    /// var accessToken = "your_access_token";
    /// var hubId = "your_hub_id";
    /// var projects = GetProjects(accessToken, hubId);
    /// foreach (var projectInfo in projects)
    /// {
    ///     Console.WriteLine($"Project ID: {projectInfo.Value.id}");
    ///     Console.WriteLine($"Project Name: {projectInfo.Value.attributes.displayName}");
    ///     // Access more properties as needed...
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="ProjectsApi"/>
    /// <seealso cref="DynamicDictionaryItems"/>
    public DynamicDictionaryItems GetProjects(string AccessToken, string hubId)
    {
        var projectsApi = new ProjectsApi();
        projectsApi.Configuration.AccessToken = AccessToken;
        dynamic result = projectsApi.GetHubProjectsAsync(hubId).Result;
        return new DynamicDictionaryItems(result.data);
    }

    /// <summary>
    /// Retrieves the top-level folders within a specified project in Autodesk BIM 360.
    /// </summary>
    /// <param name="AccessToken">The access token for authentication.</param>
    /// <param name="hubId">The unique identifier of the hub.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <returns>
    /// A <see cref="Dictionary{TKey, TValue}"/> containing the top-level folders in the project, where the key is the folder ID and the value is the folder name.
    /// </returns>
    /// <remarks>
    /// This method utilizes the ProjectsApi to asynchronously retrieve the top-level folders within a specified project in a hub.
    /// It iterates through the folders in the project and populates a dictionary with folder IDs as keys and folder names as values.
    /// </remarks>
    /// <example>
    /// The following example demonstrates how to use the GetTopFolders method:
    /// <code>
    /// var accessToken = "your_access_token";
    /// var hubId = "your_hub_id";
    /// var projectId = "your_project_id";
    /// var topFolders = GetTopFolders(accessToken, hubId, projectId);
    /// foreach (var folder in topFolders)
    /// {
    ///     Console.WriteLine($"Folder ID: {folder.Key}");
    ///     Console.WriteLine($"Folder Name: {folder.Value}");
    ///     // Access more properties as needed...
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="ProjectsApi"/>
    /// <seealso cref="Dictionary{TKey, TValue}"/>
    public Dictionary<string, string> GetTopFolders(string AccessToken, string hubId, string projectId)
    {
        var folders = new Dictionary<string, string>();
        var projectsApi = new ProjectsApi();
        projectsApi.Configuration.AccessToken = AccessToken;
        dynamic result = projectsApi.GetProjectTopFoldersAsync(hubId, projectId).Result;
        foreach (KeyValuePair<string, dynamic> folderInfo in new DynamicDictionaryItems(result.data))
        {
            string name = (string)folderInfo.Value.attributes.displayName;
            string id = (string)folderInfo.Value.id;
            folders.Add(id, name);
        }

        return folders;
    }
    public (string,string) GetTopProjectFilesFolder(string AccessToken, string hubId, string projectId)
    {
        var projectsApi = new ProjectsApi();
        projectsApi.Configuration.AccessToken = AccessToken;
        dynamic result = projectsApi.GetProjectTopFoldersAsync(hubId, projectId).Result;
        foreach (KeyValuePair<string, dynamic> folderInfo in new DynamicDictionaryItems(result.data))
        {
            string name = (string)folderInfo.Value.attributes.displayName;
            string id = (string)folderInfo.Value.id;
            if (name == "Project Files")
            {
                return (id, name);
            }
        }
        return ("", "");
    }

    /// <summary>
    /// Retrieves information about a specific item within a specified folder in Autodesk BIM 360.
    /// </summary>
    /// <param name="AccessToken">The access token for authentication.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="folderId">The unique identifier of the folder within the project.</param>
    /// <param name="fileName">The name of the file for which information is being retrieved.</param>
    /// <returns>
    /// A <see cref="DynamicDictionaryItems"/> containing information about the specified item, or null if the item is not found.
    /// </returns>
    /// <remarks>
    /// This method utilizes the FoldersApi to asynchronously retrieve the contents of a specified folder in a project.
    /// It then iterates through the items in the folder and returns the information about the item matching the specified file name.
    /// </remarks>
    /// <example>
    /// The following example demonstrates how to use the GetItemByFolder method:
    /// <code>
    /// var accessToken = "your_access_token";
    /// var projectId = "your_project_id";
    /// var folderId = "your_folder_id";
    /// var fileName = "your_file_name.txt";
    /// var itemInfo = GetItemByFolder(accessToken, projectId, folderId, fileName);
    /// if (itemInfo != null)
    /// {
    ///     Console.WriteLine($"Item ID: {itemInfo.id}");
    ///     Console.WriteLine($"File Name: {itemInfo.attributes.displayName}");
    ///     // Access more properties as needed...
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Item not found in the specified folder.");
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="FoldersApi"/>
    /// <seealso cref="DynamicDictionaryItems"/>
    public dynamic? GetItemByFolder(string AccessToken, string projectId, string folderId, string fileName)
    {
        var foldersApi = new FoldersApi();
        foldersApi.Configuration.AccessToken = AccessToken;
        dynamic result = foldersApi.GetFolderContentsAsync(projectId, folderId).Result;
        foreach (KeyValuePair<string, dynamic> itemInfo in new DynamicDictionaryItems(result.data))
        {
            string name = (string)itemInfo.Value.attributes.displayName;
            if (name == fileName)
            {
                return itemInfo.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Retrieves a list of items within a specified folder in Autodesk BIM 360.
    /// </summary>
    /// <param name="AccessToken">The access token for authentication.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="folderId">The unique identifier of the folder within the project.</param>
    /// <returns>
    /// A <see cref="DynamicDictionaryItems"/> containing information about items within the specified folder.
    /// </returns>
    /// <remarks>
    /// This method utilizes the FoldersApi to asynchronously retrieve the contents of a specified folder in a project.
    /// The result is returned as a DynamicDictionaryItems object, allowing easy access to item information.
    /// </remarks>
    /// <example>
    /// The following example demonstrates how to use the GetItemsByFolder method:
    /// <code>
    /// var accessToken = "your_access_token";
    /// var projectId = "your_project_id";
    /// var folderId = "your_folder_id";
    /// var itemsInFolder = GetItemsByFolder(accessToken, projectId, folderId);
    /// foreach (var item in itemsInFolder)
    /// {
    ///     Console.WriteLine($"Item ID: {item.Value.id}");
    ///     Console.WriteLine($"File Name: {item.Value.attributes.displayName}");
    ///     // Add more properties as needed...
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="FoldersApi"/>
    /// <seealso cref="DynamicDictionaryItems"/>
    public DynamicDictionaryItems GetItemsByFolder(string AccessToken, string projectId, string folderId)
    {
        var foldersApi = new FoldersApi();
        foldersApi.Configuration.AccessToken = AccessToken;
        dynamic result = foldersApi.GetFolderContentsAsync(projectId, folderId).Result;
        return new DynamicDictionaryItems(result.data);
    }

    /// <summary>
    /// Retrieves a list of items with a specified file name in Autodesk BIM 360.
    /// </summary>
    /// <param name="AccessToken">The access token for authentication.</param>
    /// <param name="hubId">The unique identifier of the hub.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="fileName">The name of the file to search for.</param>
    /// <param name="stopFirstFind">Optional parameter indicating whether to stop searching after the first find. Default is true.</param>
    /// <returns>
    /// A list of dynamic objects containing information about items with the specified file name.
    /// </returns>
    /// <remarks>
    /// This method utilizes the FoldersApi to asynchronously retrieve the contents of the top folder in a project.
    /// It searches for items with the specified file name, and if the file is found, it is added to the result list.
    /// If the stopFirstFind parameter is set to true, the search stops after the first find.
    /// </remarks>
    /// <example>
    /// The following example demonstrates how to use the GetItemsByFileName method:
    /// <code>
    /// var accessToken = "your_access_token";
    /// var hubId = "your_hub_id";
    /// var projectId = "your_project_id";
    /// var fileName = "your_file_name";
    /// var stopFirstFind = true;
    /// var items = GetItemsByFileName(accessToken, hubId, projectId, fileName, stopFirstFind);
    /// foreach (var item in items)
    /// {
    ///     Console.WriteLine($"Item ID: {item.Value.id}");
    ///     Console.WriteLine($"File Name: {item.Value.attributes.displayName}");
    ///     // Add more properties as needed...
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="FoldersApi"/>
    public List<dynamic?> GetItemsByFileName(string AccessToken, string hubId, string projectId,
        string fileName, bool stopFirstFind = true)
    {
        var files = new List<dynamic?>();
        string TopFolderId = GetTopFolders(AccessToken, hubId, projectId)
            .FirstOrDefault(x => x.Value == "Project Files").Key;
        var foldersApi = new FoldersApi();
        foldersApi.Configuration.AccessToken = AccessToken;
        dynamic result = foldersApi.GetFolderContentsAsync(projectId, TopFolderId).Result;
        bool isFounded = false;
        foreach (KeyValuePair<string, dynamic> itemInfo in new DynamicDictionaryItems(result.data))
        {
            string name = (string)itemInfo.Value.attributes.displayName;
            string id = (string)itemInfo.Value.id;
            // if type folder, recursive
            if (itemInfo.Value.type == "folders")
            {
                RecursiveFileInFolder(AccessToken, projectId, id, fileName, stopFirstFind, ref files, ref isFounded);
            }
            else if (name == fileName)
            {
                files.Add(itemInfo);
            }
        }

        return files;
    }

    private void RecursiveFileInFolder(string AccessToken, string projectId, string folderId, string fileName,
        bool isStopFirstFind,
        ref List<dynamic?> files, ref bool IsFounded)
    {
        if (IsFounded && isStopFirstFind) return;
        var foldersApi = new FoldersApi();
        foldersApi.Configuration.AccessToken = AccessToken;
        dynamic result = foldersApi.GetFolderContentsAsync(projectId, folderId).Result;
        foreach (KeyValuePair<string, dynamic> itemInfo in new DynamicDictionaryItems(result.data))
        {
            string name = (string)itemInfo.Value.attributes.displayName;
            string id = (string)itemInfo.Value.id;
            if (itemInfo.Value.type == "folders")
            {
                RecursiveFileInFolder(AccessToken, projectId, id, fileName, isStopFirstFind, ref files, ref IsFounded);
            }
            else if (itemInfo.Value.type == "items" && name == fileName)
            {
                files.Add(itemInfo);
                IsFounded = true;
            }
        }
    }

    /// <summary>
    /// Retrieves the versions of a specific item in Autodesk BIM 360.
    /// </summary>
    /// <param name="AccessToken">The access token for authentication.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="itemId">The unique identifier of the item.</param>
    /// <returns>
    /// A DynamicDictionaryItems containing the versions information for the specified item.
    /// </returns>
    /// <remarks>
    /// This method utilizes the ItemsApi to asynchronously retrieve the versions of a specific item in Autodesk BIM 360.
    /// The result is encapsulated in a DynamicDictionaryItems for convenient access to version information.
    /// </remarks>
    /// <example>
    /// The following example demonstrates how to use the GetItemVersions method:
    /// <code>
    /// var accessToken = "your_access_token";
    /// var projectId = "your_project_id";
    /// var itemId = "your_item_id";
    /// var versions = GetItemVersions(accessToken, projectId, itemId);
    /// foreach (var versionInfo in versions)
    /// {
    ///     Console.WriteLine($"Version ID: {versionInfo.Value.id}");
    ///     Console.WriteLine($"Version Number: {versionInfo.Value.attributes.versionNumber}");
    ///     // Add more properties as needed...
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="ItemsApi"/>
    /// <seealso cref="DynamicDictionaryItems"/>
    public DynamicDictionaryItems GetItemVersions(string AccessToken, string projectId, string itemId)
    {
        var itemsApi = new ItemsApi();
        itemsApi.Configuration.AccessToken = AccessToken;
        dynamic result = itemsApi.GetItemVersionsAsync(projectId, itemId).Result;
        DynamicDictionaryItems dictionaryItems = new DynamicDictionaryItems(result.data);
        return dictionaryItems;
    }

    /// <summary>
    /// Retrieves the latest version number of an item in Autodesk BIM 360.
    /// </summary>
    /// <param name="AccessToken">The access token for authentication.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="itemId">The unique identifier of the item.</param>
    /// <returns>
    /// An integer representing the latest version number of the specified item.
    /// </returns>
    /// <remarks>
    /// This method utilizes the ItemsApi to retrieve the versions of a specific item in Autodesk BIM 360.
    /// It then counts the number of versions to determine the latest version of the item.
    /// </remarks>
    /// <seealso cref="ItemsApi"/>
    public dynamic? GetLatestVersionItem(string AccessToken, string projectId, string itemId)
    {
        var itemsApi = new ItemsApi();
        itemsApi.Configuration.AccessToken = AccessToken;
        dynamic result = itemsApi.GetItemVersionsAsync(projectId, itemId).Result;
        if(result==null) return null;
        return result.data[0];
    }

    /// <summary>
    /// Returns the status of processing data by urn model
    /// </summary>
    /// <param name="AccessToken"></param>
    /// <param name="urn"></param>
    /// <returns></returns>
    public string CheckStatusProcessingData(string AccessToken, string urn)
    {
        var derivativeApi = new DerivativesApi();
        derivativeApi.Configuration.AccessToken = AccessToken;
        dynamic manifest = derivativeApi.GetManifestAsync(urn).Result;
        string status = manifest.progress;
        return status;
    }

    /// <summary>
    /// Return the status of processing data by itemid and version
    /// </summary>
    /// <param name="AccessToken"></param>
    /// <param name="projectId"></param>
    /// <param name="itemId"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    public string CheckStatusProcessingData(string AccessToken,string projectId, string itemId,int version)
    {
        var derivativeApi = new DerivativesApi();
        derivativeApi.Configuration.AccessToken = AccessToken;
        string urn =  GetDerivativesUrn(AccessToken, projectId,itemId,version);
        return CheckStatusProcessingData(AccessToken,urn);
    }

    /// <summary>
    /// Retrieves the unique identifier (URN) of derivatives for a specific version of an item in Autodesk BIM 360.
    /// </summary>
    /// <param name="AccessToken">The access token for authentication.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="itemId">The unique identifier of the item.</param>
    /// <param name="version">The version number for which derivatives URN is requested.</param>
    /// <returns>
    /// A nullable string representing the unique identifier (URN) of derivatives for the specified version,
    /// or null if the version or derivatives information is not found.
    /// </returns>
    /// <remarks>
    /// This method retrieves the versions of an item and iterates through them to find the specified version.
    /// If the version is found, it returns the derivatives URN associated with that version.
    /// </remarks>
    /// <seealso cref="GetItemVersions"/>
    public string? GetDerivativesUrn(string AccessToken, string projectId, string itemId, int version)
    {
        var versions = GetItemVersions(AccessToken, projectId, itemId);
        foreach (KeyValuePair<string, dynamic> itemInfo in versions)
        {
            if (itemInfo.Value.attributes.versionNumber == version)
            {
                return itemInfo.Value.relationships.derivatives.data.id;
            }
        }
        return null;
    }
    /// <summary>
    /// Asynchronously uploads a file to Autodesk BIM 360 and creates the corresponding file item.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="folderUrn">The unique identifier (URN) of the folder to upload the file to.</param>
    /// <param name="filePath">The local file path of the file to be uploaded.</param>
    /// <param name="accessToken">The access token for authentication.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is a FileInfoInDocs
    /// representing the details of the uploaded file in Autodesk BIM 360.
    /// </returns>
    /// <remarks>
    /// This asynchronous method performs several steps:
    /// 1. Creates file storage for the uploaded file in the specified project and folder.
    /// 2. Uploads the file to the created file storage.
    /// 3. Creates or appends a version for the uploaded file in the specified project and folder.
    /// The resulting FileInfoInDocs contains details about the uploaded file in Autodesk BIM 360.
    /// </remarks>
    /// <seealso cref="CreateFileStorage"/>
    /// <seealso cref="UploadFileAsync"/>
    /// <seealso cref="CreateFileItemOrAppendVersionAsync"/>
    /// <seealso cref="FileInfoInDocs"/>
    public async Task<FileInfoInDocs> UploadFileToBIM360(string projectId, string folderUrn, string filePath,
        string accessToken)
    {
        string filename = Path.GetFileName(filePath);
        var fileMemoryStream = new MemoryStream(File.ReadAllBytes(filePath));
        string objectStorageId =
            await CreateFileStorage(projectId, folderUrn, filename, accessToken).ConfigureAwait(false);
        ObjectDetails objectDetails =
            await UploadFileAsync(objectStorageId, fileMemoryStream, accessToken).ConfigureAwait(false);
        FileInfoInDocs fileInfo = await CreateFileItemOrAppendVersionAsync(projectId, folderUrn, objectDetails.ObjectId,
            filename, accessToken).ConfigureAwait(false);
        return fileInfo;
    }
    /// <summary>
    /// Asynchronously downloads a file from Autodesk BIM 360 to a local directory.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="folderId">The unique identifier of the folder containing the file.</param>
    /// <param name="fileName">The name of the file to be downloaded.</param>
    /// <param name="accessToken">The access token for authentication.</param>
    /// <param name="directoryOutput">The local directory path where the file will be saved.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is a nullable string
    /// representing the local file path where the downloaded file is saved, or null if the download fails.
    /// </returns>
    /// <remarks>
    /// This asynchronous method performs the following steps:
    /// 1. Reads the file from Autodesk BIM 360 as a byte stream.
    /// 2. Writes the byte stream to a local file in the specified directory.
    /// The resulting string is the local file path where the downloaded file is saved.
    /// </remarks>
    /// <seealso cref="ReadFileFromBIM360Stream"/>
    public async Task<string?> DownloadFileFromBIM360(string projectId, string folderId, string fileName,
        string accessToken, string directoryOutput)
    {
        string? filePath = Path.Combine(directoryOutput, fileName);
        byte[]? fileBytes = await ReadFileFromBIM360Stream(projectId, folderId, fileName, accessToken)
            .ConfigureAwait(false);
        if (fileBytes != null)
        {
            await File.WriteAllBytesAsync(filePath, fileBytes).ConfigureAwait(false);
            return filePath;
        }

        return null;
    }

    public async Task<byte[]?> ReadFileFromBIM360Stream(string projectId, string folderId, string fileName,
        string accessToken)
    {
        var itemsApi = new ItemsApi();
        itemsApi.Configuration.AccessToken = accessToken;
        var items = await GetFolderItems(projectId, folderId, accessToken).ConfigureAwait(false);
        var item = items.Cast<KeyValuePair<string, dynamic>>().FirstOrDefault(item =>
            item.Value.attributes.displayName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        if (item.Value != null)
        {
            var versions = await itemsApi.GetItemVersionsAsync(projectId, item.Value.id).ConfigureAwait(false);
            var versionItem = versions.data[0];
            // download file by url
            var downloadUrl = versionItem.relationships.storage.meta.link.href;
            // download the item
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                using (var response = await client.GetAsync(downloadUrl))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var fileStream = await response.Content.ReadAsStreamAsync())
                        {
                            var contentStream = await response.Content.ReadAsStreamAsync();
                            using (var memoryStream = new MemoryStream())
                            {
                                await contentStream.CopyToAsync(memoryStream);
                                return memoryStream.ToArray();
                            }
                        }
                    }
                }
            }
        }

        return null;
    }

    private static async Task<string> CreateFileStorage(string projectId, string folderUrn, string filename,
        string accessToken)
    {
        ProjectsApi projectsApi = new ProjectsApi();
        projectsApi.Configuration.AccessToken = accessToken;

        var storageRelData =
            new StorageRelationshipsTargetData(StorageRelationshipsTargetData.TypeEnum.Folders, folderUrn);
        var storageTarget = new CreateStorageDataRelationshipsTarget(storageRelData);
        var storageRel = new CreateStorageDataRelationships(storageTarget);
        var attributes =
            new BaseAttributesExtensionObject(string.Empty, string.Empty, new JsonApiLink(string.Empty), null);
        var storageAtt = new CreateStorageDataAttributes(filename, attributes);
        var storageData = new CreateStorageData(CreateStorageData.TypeEnum.Objects, storageAtt, storageRel);
        var storage = new CreateStorage(new JsonApiVersionJsonapi(JsonApiVersionJsonapi.VersionEnum._0), storageData);

        try
        {
            var res = await projectsApi.PostStorageAsync(projectId, storage).ConfigureAwait(false);
            string id = res.data.id;
            return id;
        }
        catch (ApiException ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private static async Task<ObjectDetails> UploadFileAsync(string objectStorageId, MemoryStream fileMemoryStream,
        string accessToken)
    {
        var objectInfo = ExtractObjectInfo(objectStorageId);
        // Get object upload url via OSS Direct-S3 API
        var objectsApi = new ObjectsApi();
        objectsApi.Configuration.AccessToken = accessToken;

        var payload = new List<UploadItemDesc>
        {
            new UploadItemDesc(objectInfo.ObjectKey, fileMemoryStream)
        };

        var results = await objectsApi.uploadResources(
            objectInfo.BucketKey,
            payload
        ).ConfigureAwait(false);

        if (results[0].Error)
        {
            throw new Exception(results[0].completed.ToString());
        }

        var json = results[0].completed.ToJson();
        return json.ToObject<ObjectDetails>();
    }

    private static ObjectInfo ExtractObjectInfo(string objectId)
    {
        var result = System.Text.RegularExpressions.Regex.Match(objectId, ".*:.*:(.*)/(.*)");
        var bucketKey = result.Groups[1].Value;
        ;
        var objectKey = result.Groups[2].Value;

        return new ObjectInfo
        {
            BucketKey = bucketKey,
            ObjectKey = objectKey
        };
    }

    private static async Task<FileInfoInDocs> CreateFileItemOrAppendVersionAsync(string projectId, string folderUrn,
        string objectId, string filename, string accessToken)
    {
        ItemsApi itemsApi = new ItemsApi();
        itemsApi.Configuration.AccessToken = accessToken;
        string? itemId = "";
        string versionId = "";
        // check if item exists
        var items = await GetFolderItems(projectId, folderUrn, accessToken).ConfigureAwait(false);
        var item = items.Cast<KeyValuePair<string, dynamic>>().FirstOrDefault(item =>
            item.Value.attributes.displayName.Equals(filename, StringComparison.OrdinalIgnoreCase));
        FileInfoInDocs? fileInfo;
        if (item.Value != null)
        {
            //Get ItemId of our file
            itemId = item.Value.id;

            //Lets create a new version
            versionId = await UpdateVersionAsync(projectId, itemId, objectId, filename, accessToken)
                .ConfigureAwait(false);

            fileInfo = new FileInfoInDocs
            {
                ProjectId = projectId,
                FolderUrn = folderUrn,
                ItemId = itemId,
                VersionId = versionId
            };

            return fileInfo;
        }

        var itemBody = new CreateItem
        (
            new JsonApiVersionJsonapi
            (
                JsonApiVersionJsonapi.VersionEnum._0
            ),
            new CreateItemData
            (
                CreateItemData.TypeEnum.Items,
                new CreateItemDataAttributes
                (
                    DisplayName: filename,
                    new BaseAttributesExtensionObject
                    (
                        Type: "items:autodesk.bim360:File",
                        Version: "1.0"
                    )
                ),
                new CreateItemDataRelationships
                (
                    new CreateItemDataRelationshipsTip
                    (
                        new CreateItemDataRelationshipsTipData
                        (
                            CreateItemDataRelationshipsTipData.TypeEnum.Versions,
                            CreateItemDataRelationshipsTipData.IdEnum._1
                        )
                    ),
                    new CreateStorageDataRelationshipsTarget
                    (
                        new StorageRelationshipsTargetData
                        (
                            StorageRelationshipsTargetData.TypeEnum.Folders,
                            Id: folderUrn
                        )
                    )
                )
            ),
            new List<CreateItemIncluded>
            {
                new CreateItemIncluded
                (
                    CreateItemIncluded.TypeEnum.Versions,
                    CreateItemIncluded.IdEnum._1,
                    new CreateStorageDataAttributes
                    (
                        filename,
                        new BaseAttributesExtensionObject
                        (
                            Type: "versions:autodesk.bim360:File",
                            Version: "1.0"
                        )
                    ),
                    new CreateItemRelationships(
                        new CreateItemRelationshipsStorage
                        (
                            new CreateItemRelationshipsStorageData
                            (
                                CreateItemRelationshipsStorageData.TypeEnum.Objects,
                                objectId
                            )
                        )
                    )
                )
            }
        );


        try
        {
            DynamicJsonResponse postItemJsonResponse =
                await itemsApi.PostItemAsync(projectId, itemBody).ConfigureAwait(false);
            var uploadItem = postItemJsonResponse.ToObject<ItemCreated>();
            Console.WriteLine("Attributes of uploaded BIM 360 file");
            Console.WriteLine($"\n\t{uploadItem.Data.Attributes.ToJson()}");
            itemId = uploadItem.Data.Id;
            versionId = uploadItem.Data.Relationships.Tip.Data.Id;
        }
        catch (ApiException ex)
        {
            //we met a conflict
            dynamic? errorContent = JsonConvert.DeserializeObject<JObject>(ex.ErrorContent);
            if (errorContent.Errors?[0].Status == "409") //Conflict
            {
                try
                {
                    //Get ItemId of our file
                    itemId = await GetItemIdAsync(projectId, folderUrn, filename, accessToken).ConfigureAwait(false);

                    //Lets create a new version
                    versionId = await UpdateVersionAsync(projectId, itemId, objectId, filename, accessToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Trace.WriteLine("Failed to append new file version", ex2.Message);
                }
            }
        }

        if (string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(versionId))
        {
            throw new InvalidOperationException("Failed to Create/Append file version");
        }

        fileInfo = new FileInfoInDocs
        {
            ProjectId = projectId,
            FolderUrn = folderUrn,
            ItemId = itemId,
            VersionId = versionId
        };

        return fileInfo;
    }

    private static async Task<string> UpdateVersionAsync(string projectId, string? itemId, string objectId,
        string filename, string accessToken)
    {
        var versionsApi = new VersionsApi();
        versionsApi.Configuration.AccessToken = accessToken;

        var relationships = new CreateVersionDataRelationships
        (
            new CreateVersionDataRelationshipsItem
            (
                new CreateVersionDataRelationshipsItemData
                (
                    CreateVersionDataRelationshipsItemData.TypeEnum.Items,
                    itemId
                )
            ),
            new CreateItemRelationshipsStorage
            (
                new CreateItemRelationshipsStorageData
                (
                    CreateItemRelationshipsStorageData.TypeEnum.Objects,
                    objectId
                )
            )
        );
        var createVersion = new CreateVersion
        (
            new JsonApiVersionJsonapi
            (
                JsonApiVersionJsonapi.VersionEnum._0
            ),
            new CreateVersionData
            (
                CreateVersionData.TypeEnum.Versions,
                new CreateStorageDataAttributes
                (
                    filename,
                    new BaseAttributesExtensionObject
                    (
                        "versions:autodesk.bim360:File",
                        "1.0",
                        new JsonApiLink(string.Empty),
                        null
                    )
                ),
                relationships
            )
        );

        dynamic versionResponse = await versionsApi.PostVersionAsync(projectId, createVersion).ConfigureAwait(false);
        var versionId = versionResponse.data.id;
        return versionId;
    }

    /// <summary>
    /// Asynchronously retrieves the unique identifier (ID) of an item in Autodesk BIM 360.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="folderUrn">The unique identifier (URN) of the folder containing the item.</param>
    /// <param name="filename">The name of the item's file.</param>
    /// <param name="accessToken">The access token for authentication.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is a nullable string
    /// representing the unique identifier (ID) of the specified item, or null if the item is not found.
    /// </returns>
    /// <remarks>
    /// This asynchronous method utilizes the FoldersApi to retrieve folder items from Autodesk BIM 360.
    /// It then searches for an item with a matching filename and returns its unique identifier (ID).
    /// The comparison is case-insensitive.
    /// </remarks>
    /// <seealso cref="FoldersApi"/>
    /// <seealso cref="DynamicDictionaryItems"/>
    private static async Task<string?> GetItemIdAsync(string projectId, string folderUrn, string filename,
        string accessToken)
    {
        FoldersApi foldersApi = new FoldersApi();
        foldersApi.Configuration.AccessToken = accessToken;
        DynamicDictionaryItems itemList = await GetFolderItems(projectId, folderUrn, accessToken).ConfigureAwait(false);
        var item = itemList.Cast<KeyValuePair<string, dynamic>>().FirstOrDefault(item =>
            item.Value.attributes.displayName.Equals(filename, StringComparison.OrdinalIgnoreCase));
        return item.Value?.Id;
    }
    /// <summary>
    /// Retrieves folder items from Autodesk BIM 360.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="folderId">The unique identifier of the folder.</param>
    /// <param name="accessToken">The access token for authentication.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is a DynamicDictionaryItems
    /// containing the items within the specified folder.
    /// </returns>
    /// <remarks>
    /// This asynchronous method utilizes the FoldersApi to retrieve folder contents from Autodesk BIM 360.
    /// It specifies the filterType to include only items in the response.
    /// The result is wrapped in a DynamicDictionaryItems for convenient access to the folder data.
    /// </remarks>
    /// <seealso cref="FoldersApi"/>
    /// <seealso cref="DynamicDictionaryItems"/>
    private static async Task<DynamicDictionaryItems> GetFolderItems(string projectId, string folderId,
        string accessToken)
    {
        var foldersApi = new FoldersApi();
        foldersApi.Configuration.AccessToken = accessToken;
        dynamic folderContents = await foldersApi.GetFolderContentsAsync(projectId,
            folderId,
            filterType: new List<string>() { "items" }
            // filterExtensionType: new List<string>() {"items:autodesk.bim360:File"}
        ).ConfigureAwait(false);

        var folderData = new DynamicDictionaryItems(folderContents.data);

        return folderData;
    }
    /// <summary>
    /// Publishes a model in Autodesk BIM 360.
    /// </summary>
    /// <param name="token3Legged">The OAuth 3-legged token for authentication.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="itemId">The unique identifier of the item to be published.</param>
    /// <returns>
    /// A RestResponse containing the result of the model publish operation.
    /// </returns>
    /// <remarks>
    /// This method performs a REST API call to publish a model in Autodesk BIM 360.
    /// It constructs the API request body with the necessary attributes and relationships.
    /// The result of the operation is returned as a RestResponse.
    /// </remarks>
    public RestResponse PublishModel(string token3Legged, string projectId, string itemId)
    {
        string url = $"https://developer.api.autodesk.com/data/v1/projects/{projectId}/commands";
        var client = new RestClient(url);
        var request = new RestRequest();
        request.Method = Method.Post;
        request.AddHeader("Content-Type", "application/vnd.api+json");
        request.AddHeader("Authorization", "Bearer " + token3Legged);
        request.AddHeader("Accept", "application/vnd.api+json");
        var body = new
        {
            jsonapi = new
            {
                version = "1.0"
            },
            data = new
            {
                type = "commands",
                attributes = new
                {
                    extension = new
                    {
                        type = "commands:autodesk.bim360:C4RModelPublish",
                        version = "1.0.0"
                    },
                },
                relationships = new
                {
                    resources = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "items",
                                id = $"{itemId}"
                            }
                        }
                    }
                }
            }
        };
        request.AddJsonBody(body);
        var response = client.Execute(request);
        return response;
    }
    /// <summary>
    /// Publishes a model without creating a link in Autodesk BIM 360.
    /// </summary>
    /// <param name="token3Legged">The OAuth 3-legged token for authentication.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="itemId">The unique identifier of the item to be published.</param>
    /// <returns>
    /// A RestResponse containing the result of the publish operation.
    /// </returns>
    /// <remarks>
    /// This method performs a REST API call to publish a model without creating a link in Autodesk BIM 360.
    /// It constructs the API request body with the necessary attributes and relationships.
    /// The result of the operation is returned as a RestResponse.
    /// </remarks>
    public RestResponse PublishModelWithoutLink(string token3Legged, string projectId, string itemId)
    {
        string url = $"https://developer.api.autodesk.com/data/v1/projects/{projectId}/commands";
        var client = new RestClient(url);
        var request = new RestRequest();
        request.Method = Method.Post;
        request.AddHeader("Content-Type", "application/vnd.api+json");
        request.AddHeader("Authorization", "Bearer " + token3Legged);
        request.AddHeader("Accept", "application/vnd.api+json");
        var body = new
        {
            jsonapi = new
            {
                version = "1.0"
            },
            data = new
            {
                type = "commands",
                attributes = new
                {
                    extension = new
                    {
                        type = "commands:autodesk.bim360:C4RPublishWithoutLinks",
                        version = "1.0.0"
                    },
                },
                relationships = new
                {
                    resources = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "items",
                                id = $"{itemId}"
                            }
                        }
                    }
                }
            }
        };
        request.AddJsonBody(body);
        var response = client.Execute(request);
        return response;
    }
    /// <summary>
    /// Retrieves original BIM data properties for a given project and index version.
    /// </summary>
    /// <param name="token3Leg">The OAuth 3-legged token for authentication.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="indexVersionId">The unique identifier of the index version.</param>
    /// <returns>
    /// A list of BIMObject instances containing the original properties based on the specified project and index version.
    /// </returns>
    /// <remarks>
    /// This method performs REST API calls to obtain manifest, fields, and properties URLs for a given project and index version.
    /// It then downloads and processes the fields and properties data, mapping them to the corresponding BIMField and BIMObject properties.
    /// The resulting list of BIMObject instances includes the original properties, updating property keys based on the associated fields.
    /// </remarks>
    /// <seealso cref="BIMField"/>
    /// <seealso cref="BIMObject"/>
    public List<BIMObject?> GetAllDataOriginalProperties(string token3Leg, string projectId, string indexVersionId)
    {
        if (projectId.StartsWith("b."))
        {
            projectId = projectId.Substring(2);
        }

        string versionUrl = $"{Host}/construction/index/v2/projects/{projectId}/indexes/{indexVersionId}";

        // rest api get to get manifestUrl, fieldsUrl,propertiesUrl
        var client = new RestClient(versionUrl);
        var request = new RestRequest();
        request.Method = Method.Get;
        request.AddHeader("Authorization", $"Bearer {token3Leg}");
        var response = client.Execute(request);
        dynamic version = JsonConvert.DeserializeObject(response.Content);
        string manifestUrl = version.manifestUrl;
        string fieldsUrl = version.fieldsUrl;
        string propertiesUrl = version.propertiesUrl;
        Console.WriteLine(manifestUrl);

        // download the fieldsUrl
        var client2 = new RestClient(fieldsUrl);
        var request2 = new RestRequest();
        request2.Method = Method.Get;
        request2.AddHeader("Authorization", $"Bearer {token3Leg}");
        var response2 = client2.Execute(request2);
        //DeserializeObject to list feilds
        string? content = response2.Content;
        // read content by \n to fix to json format before deserialize to dictionary
        string[] contentArray = content.Split("\n");
        Dictionary<string, BIMField?> fields = new Dictionary<string, BIMField?>();
        foreach (var item in contentArray)
        {
            if (item != "")
            {
                BIMField? bimField = JsonConvert.DeserializeObject<BIMField>(item);
                fields.Add(bimField.key, bimField);
            }
        }

        // download the propertiesUrl
        var client3 = new RestClient(propertiesUrl);
        var request3 = new RestRequest();
        request3.Method = Method.Get;
        request3.AddHeader("Authorization", $"Bearer {token3Leg}");
        var response3 = client3.Execute(request3);
        string? content3 = response3.Content;
        //read content3 by \n to fix to json format before deserialize
        List<BIMObject?> properties = new List<BIMObject?>();
        string[] contentArray3 = content3.Split("\n");
        foreach (var item in contentArray3)
        {
            if (item != "")
            {
                Dictionary<string, object> updatedProps = new Dictionary<string, object>();
                BIMObject? bimProperty = JsonConvert.DeserializeObject<BIMObject>(item);
                foreach (KeyValuePair<string, object> prop in bimProperty.props)
                {
                    var field = fields[prop.Key];
                    // set override prob key to field name
                    if (field != null)
                    {
                        updatedProps[field.name] = prop.Value;
                    }
                }

                bimProperty.props = updatedProps;
                properties.Add(bimProperty);
            }
        }

        return properties;
    }
    public void ExportRevitDataToParquet(string token3Leg, string projectId, string indexVersionId,string filePath)
    {
        if (projectId.StartsWith("b."))
        {
            projectId = projectId.Substring(2);
        }

        string versionUrl = $"{Host}/construction/index/v2/projects/{projectId}/indexes/{indexVersionId}";

        // rest api get to get manifestUrl, fieldsUrl,propertiesUrl
        var client = new RestClient(versionUrl);
        var request = new RestRequest();
        request.Method = Method.Get;
        request.AddHeader("Authorization", $"Bearer {token3Leg}");
        var response = client.Execute(request);
        dynamic version = JsonConvert.DeserializeObject(response.Content);
        string manifestUrl = version.manifestUrl;
        string fieldsUrl = version.fieldsUrl;
        string propertiesUrl = version.propertiesUrl;
        Console.WriteLine(manifestUrl);

        // download the fieldsUrl
        var client2 = new RestClient(fieldsUrl);
        var request2 = new RestRequest();
        request2.Method = Method.Get;
        request2.AddHeader("Authorization", $"Bearer {token3Leg}");
        var response2 = client2.Execute(request2);
        //DeserializeObject to list feilds
        string? content = response2.Content;
        // read content by \n to fix to json format before deserialize to dictionary
        string[] contentArray = content.Split("\n");
        Dictionary<string, BIMField?> fields = new Dictionary<string, BIMField?>();
        foreach (var item in contentArray)
        {
            if (item != "")
            {
                BIMField? bimField = JsonConvert.DeserializeObject<BIMField>(item);
                fields.Add(bimField.key, bimField);
            }
        }
        // download the propertiesUrl
        var client3 = new RestClient(propertiesUrl);
        var request3 = new RestRequest();
        request3.Method = Method.Get;
        request3.AddHeader("Authorization", $"Bearer {token3Leg}");
        var response3 = client3.Execute(request3);
        string? content3 = response3.Content;
        //read content3 by \n to fix to json format before deserialize
        string[] contentArray3 = content3.Split("\n");
        BIMObject[] properties = new BIMObject[contentArray3.Length];
        for( int i=0 ; i<contentArray3.Length ; i++)
        {
            if (contentArray3[i] != "")
            {
                BIMObject? bimProperty = JsonConvert.DeserializeObject<BIMObject>(contentArray3[i]);
                properties[i] = bimProperty;
            }
        }
        ParquetExport parquetExport = new ParquetExport(fields,properties);
        parquetExport.WriteToParquet(filePath);
    }
    /// <summary>
    /// Asynchronously initiates the building of property indexes for a given project and a list of version IDs.
    /// </summary>
    /// <param name="token3Leg">The OAuth 3-legged token for authentication.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="versionIds">A list of version IDs for which property indexes are to be built.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is a dynamic object
    /// representing the deserialized content of the batch status for building property indexes.
    /// </returns>
    /// <remarks>
    /// This asynchronous method performs a REST API call to initiate the building of property indexes
    /// for the specified project and a list of version IDs.
    /// If the project ID starts with "b.", it is adjusted accordingly.
    /// The version IDs are provided in a list, and a JSON body is constructed for the API request.
    /// It deserializes the JSON content of the API response using JsonConvert into a dynamic object.
    /// If the response content is not null, it returns the deserialized dynamic object; otherwise, it returns null.
    /// </remarks>
    /// <seealso cref="JsonConvert"/>
    /// <seealso cref="RestClient"/>
    /// <seealso cref="RestRequest"/>
    /// <seealso cref="RestResponse"/>
    public async Task<dynamic?> BuildPropertyIndexesAsync(string token3Leg, string projectId, List<string> versionIds)
    {
        if (projectId.StartsWith("b."))
        {
            projectId = projectId.Substring(2);
        }

        RestClient client = new RestClient(Host);
        RestRequest request = new RestRequest($"/construction/index/v2/projects/{projectId}/indexes:batchStatus",
            RestSharp.Method.Post);
        request.AddHeader("Authorization", "Bearer " + token3Leg);

        var data = versionIds.Select(versionId => new
        {
            versionUrn = versionId
        });

        request.AddJsonBody(new
        {
            versions = data
        });

        RestResponse indexingResponse = await client.ExecuteAsync(request).ConfigureAwait(false);
        if (indexingResponse.Content != null)
        {
            var result = JsonConvert.DeserializeObject<dynamic>(indexingResponse.Content);
            return result;
        }

        return null;
    }

    /// <summary>
    /// Asynchronously retrieves the status of property indexes for a given project and index.
    /// </summary>
    /// <param name="token3Leg">The authentication credentials, such as an access token.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="indexId">The unique identifier of the index.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is a dynamic object
    /// representing the deserialized content of the property indexes status.
    /// </returns>
    /// <remarks>
    /// This asynchronous method performs a REST API call to obtain the status of property indexes
    /// for the specified project and index.
    /// If the project ID starts with "b.", it is adjusted accordingly.
    /// It deserializes the JSON content using JsonConvert into a dynamic object.
    /// If the response content is not null, it returns the deserialized dynamic object; otherwise, it returns null.
    /// </remarks>
    /// <seealso cref="JsonConvert"/>
    /// <seealso cref="RestClient"/>
    /// <seealso cref="RestRequest"/>
    /// <seealso cref="RestResponse"/>
    public async Task<dynamic?> GetPropertyIndexesStatusAsync(string token3Leg, string projectId, string indexId)
    {
        if (projectId.StartsWith("b."))
        { projectId = projectId.Substring(2);
        }
        RestClient client = new RestClient(Host);
        RestRequest request = new RestRequest($"/construction/index/v2/projects/{projectId}/indexes/{indexId}",
            RestSharp.Method.Get);
        request.AddHeader("Authorization", "Bearer " + token3Leg);

        RestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);
        if (response.Content != null)
        {
            var result = JsonConvert.DeserializeObject<dynamic>(response.Content);
            return result;
        }

        return null;
    }
    /// <summary>
    /// Asynchronously retrieves the property indexes manifest for a given project and index.
    /// </summary>
    /// <param name="token3Leg">The OAuth 3-legged token for authentication.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="indexId">The unique identifier of the index.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is a dynamic object
    /// representing the deserialized content of the property indexes manifest.
    /// </returns>
    /// <remarks>
    /// This asynchronous method performs a REST API call to obtain the property indexes manifest
    /// for the specified project and index.
    /// If the project ID starts with "b.", it is adjusted accordingly.
    /// It deserializes the JSON content using JsonConvert into a dynamic object.
    /// If the response content is not null, it returns the deserialized dynamic object; otherwise, it returns null.
    /// </remarks>
    /// <seealso cref="JsonConvert"/>
    /// <seealso cref="RestClient"/>
    /// <seealso cref="RestRequest"/>
    /// <seealso cref="RestResponse"/>
    public async Task<dynamic?> GetPropertyIndexesManifestAsync(string token3Leg, string projectId, string indexId)
    {
        if (projectId.StartsWith("b."))
        {
            projectId = projectId.Substring(2);
        }

        RestClient client = new RestClient(Host);
        RestRequest request = new RestRequest($"/construction/index/v2/projects/{projectId}/indexes/{indexId}/manifest",
            RestSharp.Method.Get);
        request.AddHeader("Authorization", "Bearer " + token3Leg);

        RestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);
        if (response.Content != null)
        {
            var result = JsonConvert.DeserializeObject<dynamic>(response.Content);
            return result;
        }

        return null;
    }
    /// <summary>
    /// Asynchronously retrieves property fields for a given project and index.
    /// </summary>
    /// <param name="credentials">The authentication credentials, such as an access token.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="indexId">The unique identifier of the index.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is a list of JObject instances
    /// containing the parsed line-delimited JSON content of the retrieved property fields.
    /// </returns>
    /// <remarks>
    /// This asynchronous method performs a REST API call to obtain property fields for the specified project and index.
    /// If the project ID starts with "b.", it is adjusted accordingly.
    /// It utilizes the ParseLineDelimitedJson method to parse the line-delimited JSON content from the API response.
    /// If the response content is not null, it returns a list of JObject instances; otherwise, it returns null.
    /// </remarks>
    /// <seealso cref="JObject"/>
    /// <seealso cref="ParseLineDelimitedJson"/>
    /// <seealso cref="RestClient"/>
    /// <seealso cref="RestRequest"/>
    /// <seealso cref="RestResponse"/>
    public async Task<List<JObject>?> GetPropertyFieldsAsync(string credentials, string projectId, string indexId)
    {
        if (projectId.StartsWith("b."))
        {
            projectId = projectId.Substring(2);
        }

        RestClient client = new RestClient(Host);
        RestRequest request = new RestRequest($"/construction/index/v2/projects/{projectId}/indexes/{indexId}/fields",
            RestSharp.Method.Get);
        request.AddHeader("Authorization", "Bearer " + credentials);

        RestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);
        if (response.Content != null)
        {
            var data = ParseLineDelimitedJson(response.Content);
            return data;
        }

        return null;
    }
    /// <summary>
    /// Asynchronously retrieves levels from AEC model data using the specified access token and URN.
    /// </summary>
    /// <param name="token3Leg">The OAuth 3-legged token for authentication.</param>
    /// <param name="urn">The URN (Unique Resource Name) of the AEC model data.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is a list of Level instances
    /// containing information about the levels extracted from the AEC model data.
    /// </returns>
    /// <exception cref="InvalidDataException">Thrown when no AEC model data is found for the provided URN.</exception>
    /// <exception cref="InvalidOperationException">Thrown when there is a failure in downloading or processing AEC model data.</exception>
    /// <remarks>
    /// This method uses the Autodesk Forge Derivatives API to obtain the manifest and download AEC model data.
    /// It processes the AEC model data to extract information about levels, including index, GUID, name, and elevation range.
    /// The resulting list of Level instances is based on the filtered and processed AEC model data.
    /// </remarks>
    /// <seealso cref="Level"/>
    /// <seealso cref="AecLevel"/>
    /// <seealso cref="DerivativesApi"/>
    /// <seealso cref="JsonSerializer"/>
    /// <seealso cref="JObject"/>
    public async Task<List<Level>> GetLevelsFromAecModelData(string token3Leg, string urn)
    {
        var derivativeApi = new DerivativesApi();
        derivativeApi.Configuration.AccessToken = token3Leg;
        string aecModelDataUrn = string.Empty;
        var data = await derivativeApi.GetManifestAsync(urn).ConfigureAwait(false);
        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(data.ToString());

        foreach (var derivative in result.derivatives)
        {
            if ((((dynamic)derivative).outputType != "svf") && (((dynamic)derivative).outputType != "svf2")) continue;

            foreach (var derivativeChild in ((dynamic)derivative).children)
            {
                if (((dynamic)derivativeChild).role != "Autodesk.AEC.ModelData") continue;

                aecModelDataUrn = ((dynamic)derivativeChild).urn;
                break;
            }

            if (!string.IsNullOrWhiteSpace(aecModelDataUrn))
                break;
        }

        if (string.IsNullOrWhiteSpace(aecModelDataUrn))
            throw new InvalidDataException($"No AEC model data found for this urn `{urn}`");

        System.IO.MemoryStream stream =
            await derivativeApi.GetDerivativeManifestAsync(urn, aecModelDataUrn).ConfigureAwait(false);
        if (stream == null)
            throw new InvalidOperationException("Failed to download AecModelData");

        stream.Seek(0, SeekOrigin.Begin);

        JObject? aecdata;
        var serializer = new JsonSerializer();
        using (var sr = new StreamReader(stream))
        using (var jsonTextReader = new JsonTextReader(sr))
        {
            aecdata = serializer.Deserialize<JObject>(jsonTextReader);
        }

        if (aecdata == null)
            throw new InvalidOperationException("Failed to process AecModelData");

        var levelJsonToken = aecdata.GetValue("levels");
        var levelData = levelJsonToken.ToObject<List<AecLevel>>();

        var filteredLevels = levelData.Where(lvl => lvl.Extension != null)
            .Where(lvl => lvl.Extension.BuildingStory == true)
            .ToList();

        Func<AecLevel, double>? getProjectElevation = default(Func<AecLevel, double>);
        getProjectElevation = level => level.Extension.ProjectElevation.HasValue
            ? level.Extension.ProjectElevation.Value
            : level.Elevation;

        var levels = new List<Level>();
        double zOffsetHack = 1.0 / 12.0;
        for (var i = 0; i < filteredLevels.Count; i++)
        {
            var level = filteredLevels[i];
            double nextElevation;
            if (i + 1 < filteredLevels.Count)
            {
                var nextLevel = filteredLevels[i + 1];
                nextElevation = getProjectElevation(nextLevel);
            }
            else
            {
                var topLevel = filteredLevels[filteredLevels.Count - 1];
                var topElevation = getProjectElevation(topLevel);
                nextElevation = topElevation + topLevel.Height;
            }

            levels.Add(new Level
            {
                Index = i + 1,
                Guid = level.Guid,
                Name = level.Name,
                ZMin = getProjectElevation(level) - zOffsetHack,
                ZMax = nextElevation,
            });
        }

        return levels;
    }

    /// <summary>
    /// Asynchronously retrieves property results for a given project and index.
    /// </summary>
    /// <param name="token3Leg">The OAuth 3-legged token for authentication.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="indexId">The unique identifier of the index.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is a list of JObject instances
    /// containing the parsed line-delimited JSON content of the retrieved property results.
    /// </returns>
    /// <remarks>
    /// This asynchronous method performs a REST API call to obtain property results for the specified project and index.
    /// If the project ID starts with "b.", it is adjusted accordingly.
    /// It utilizes the ParseLineDelimitedJson method to parse the line-delimited JSON content from the API response.
    /// If the response content is not null, it returns a list of JObject instances; otherwise, it returns null.
    /// </remarks>
    /// <seealso cref="JObject"/>
    /// <seealso cref="ParseLineDelimitedJson"/>
    /// <seealso cref="RestClient"/>
    /// <seealso cref="RestRequest"/>
    /// <seealso cref="RestResponse"/>
    public async Task<List<JObject>?> GetPropertiesResultsAsync(string token3Leg, string projectId, string indexId)
    {
        if (projectId.StartsWith("b."))
        {
            projectId = projectId.Substring(2);
        }

        RestClient client = new RestClient(Host);
        RestRequest request =
            new RestRequest($"/construction/index/v2/projects/{projectId}/indexes/{indexId}/properties",
                RestSharp.Method.Get);
        request.AddHeader("Authorization", "Bearer " + token3Leg);
        RestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);
        if (response.Content != null)
        {
            var data = ParseLineDelimitedJson(response.Content);
            return data;
        }

        return null;
    }

    public async Task<List<JObject>?> GetPropertyQueryResultsAsync(string token3Leg, string projectId, string indexId,
        string queryId)
    {
        if (projectId.StartsWith("b."))
        {
            projectId = projectId.Substring(2);
        }

        RestClient client = new RestClient(Host);
        RestRequest request =
            new RestRequest(
                $"/construction/index/v2/projects/{projectId}/indexes/{indexId}/queries/{queryId}/properties",
                RestSharp.Method.Get);
        request.AddHeader("Authorization", "Bearer " + token3Leg);
        RestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);
        if (response.Content != null)
        {
            var data = ParseLineDelimitedJson(response.Content);
            return data;
        }

        return null;
    }

    /// <summary>
    /// Parses line-delimited JSON content into a list of JObject instances.
    /// </summary>
    /// <param name="content">The line-delimited JSON content to be parsed.</param>
    /// <returns>A list of JObject instances representing the parsed JSON data.</returns>
    /// <remarks>
    /// This method uses a JsonTextReader with support for multiple content to read line-delimited JSON.
    /// It deserializes each JSON item into a JObject and adds it to the resulting list.
    /// </remarks>
    /// <seealso cref="JObject"/>
    /// <seealso cref="JsonTextReader"/>
    /// <seealso cref="JsonSerializer"/>
    private List<JObject> ParseLineDelimitedJson(string content)
    {
        var data = new List<JObject>();
        using (var jsonTextReader = new JsonTextReader(new StringReader(content)))
        {
            jsonTextReader.SupportMultipleContent = true;
            var jsonSerializer = new JsonSerializer();
            while (jsonTextReader.Read())
            {
                var item = jsonSerializer.Deserialize<JObject>(jsonTextReader);
                data.Add(item);
            }
        }

        return data;
    }

    /// <summary>
    /// Retrieves all BIM (Building Information Modeling) data associated with a specific version of a project using the provided authentication token and project identifiers.
    /// </summary>
    /// <param name="token3Leg">The authentication token required to access the project data.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="versionId">The identifier of the specific version of the project.</param>
    /// <returns>An array of BIMData objects containing the requested data.</returns>
    public BIMData[] GetAllDataByVersionId(string token3Leg, string projectId, string versionId)
    {
        dynamic? result = BuildPropertyIndexesAsync(token3Leg, projectId, new List<string>() { versionId }).Result;
        string? indexId;
        string? state = result?.indexes[0]?.state?.ToString() ?? result?.state?.ToString()?? null;
        while (!string.Equals(state, "FINISHED", StringComparison.Ordinal))
        {
            Thread.Sleep(1000);
            result = BuildPropertyIndexesAsync(token3Leg, projectId, new List<string>() { versionId }).Result;
            state = result?.indexes[0]?.state?.ToString() ?? result?.state?.ToString()?? null;
        }
        indexId = result?.indexes[0]?.indexId?.ToString() ?? result?.indexId?.ToString()?? null;
        return GetAllDataByIndexVersionId(token3Leg, projectId, indexId);
    }
    /// <summary>
    /// Retrieves BIM data properties for a given project and index version.
    /// </summary>
    /// <param name="token3Leg">The OAuth 3-legged token for authentication.</param>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="indexVersionId">The unique identifier of the index version.</param>
    /// <returns>An array of BIMData containing information about the properties, bounding box, and external ID.</returns>
    /// <remarks>
    /// This method performs a series of REST API calls to obtain manifest, fields, and properties URLs.
    /// It then downloads and processes the fields and properties data, mapping them to the corresponding BIMObject properties.
    /// The resulting BIMData array includes information such as category, name, type, value, unit, bounding box, and external ID.
    /// </remarks>
    public BIMData[] GetAllDataByIndexVersionId(string token3Leg, string projectId, string indexVersionId)
    {
        string versionUrl = $"{Host}/construction/index/v2/projects/{projectId}/indexes/{indexVersionId}";

        // rest api get to get manifestUrl, fieldsUrl,propertiesUrl
        var client = new RestClient(versionUrl);
        var request = new RestRequest();
        request.Method = Method.Get;
        request.AddHeader("Authorization", $"Bearer {token3Leg}");
        var response = client.Execute(request);
        dynamic version = JsonConvert.DeserializeObject(response.Content);
        string manifestUrl = version.manifestUrl;
        string fieldsUrl = version.fieldsUrl;
        string propertiesUrl = version.propertiesUrl;
        // download the fieldsUrl
        var client2 = new RestClient(fieldsUrl);
        var request2 = new RestRequest();
        request2.Method = Method.Get;
        request2.AddHeader("Authorization", $"Bearer {token3Leg}");
        var response2 = client2.Execute(request2);
        //DeserializeObject to list feilds
        string? content = response2.Content;
        // read content by \n to fix to json format before deserialize to dictionary
        string[] contentArray = content.Split("\n");
        Dictionary<string, BIMField?> fields = new Dictionary<string, BIMField?>();
        foreach (var item in contentArray)
        {
            if (item != "")
            {
                BIMField? bimField = JsonConvert.DeserializeObject<BIMField>(item);
                fields.Add(bimField.key, bimField);
            }
        }

        // download the propertiesUrl
        var client3 = new RestClient(propertiesUrl);
        var request3 = new RestRequest();
        request3.Method = Method.Get;
        request3.AddHeader("Authorization", $"Bearer {token3Leg}");
        var response3 = client3.Execute(request3);
        string? content3 = response3.Content;
        //read content3 by \n to fix to json format before deserialize
        string[] contentArray3 = content3.Split("\n");
        BIMData[] properties = new BIMData[contentArray3.Length];
        Dictionary<string, string> dictUnits = UnitUtils.GetAllDictUnits();
        for (int i = 0; i < contentArray3.Length; i++)
        {
            if (contentArray3[i] != "")
            {
                // bim properties length = bimobject.props length
                BIMObject? bimObject = JsonConvert.DeserializeObject<BIMObject>(contentArray3[i]);
                BIMProperty[] bimProperties = new BIMProperty[bimObject!.props.Count];
                int j = 0;
                foreach (KeyValuePair<string, object> prop in bimObject.props)
                {
                    var field = fields[prop.Key];
                    // set override prob key to field name
                    if (field != null)
                    {
                        BIMProperty bimProperty = new BIMProperty();
                        bimProperty.Category = field.category;
                        bimProperty.Name = field.name;
                        bimProperty.Type = field.type;
                        bimProperty.Value = prop.Value.ToString();
                        UnitsData? unitsData = UnitUtils.ParseUnitsData(field.uom);
                        if (unitsData != null)
                        {
                            bimProperty.Unit = dictUnits[unitsData.TypeId];
                        }

                        bimProperties[j] = bimProperty;
                    }

                    j++;
                }

                var max = new System.Numerics.Vector3();
                var min = new System.Numerics.Vector3();
                if (bimObject.bboxMax != null || bimObject.bboxMin != null)
                {
                    max = new System.Numerics.Vector3
                    {
                        X = (float)bimObject.bboxMax["x"]!,
                        Y = (float)bimObject.bboxMax["y"]!,
                        Z = (float)bimObject.bboxMax["z"]!
                    };
                    min = new System.Numerics.Vector3
                    {
                        X = (float)bimObject.bboxMin["x"]!,
                        Y = (float)bimObject.bboxMin["y"]!,
                        Z = (float)bimObject.bboxMin["z"]!
                    };
                }

                BIMProperty[] props = bimProperties.Length > 0
                    ? bimProperties.OrderBy(x => x.Name).ToArray()
                    : bimProperties.ToArray();
                BIMData bimData = new BIMData()
                {
                    properties = props,
                    bboxMax = max,
                    bboxMin = min,
                    externalId = bimObject.externalId,
                    DbId = bimObject.lmvId,
                };
                properties[i] = bimData;
            }
        }

        return properties;
    }

    /// <summary>
    /// Convert BIMData array to DataTable
    /// </summary>
    /// <param name="bimDatas">bim data </param>
    /// <returns name="datatable">datatable</returns>
    public DataTable BimDataToDataTable(BIMData[] bimDatas)
    {
        var dt = new DataTable();
        dt.Columns.Add("DbId", typeof(int));
        dt.Columns.Add("ExternalId", typeof(string));
        dt.Columns.Add("Bbox", typeof(string));
        for(int i =0; i< bimDatas.Length;i++)
        {
            DataRow row = dt.NewRow();
            var data = bimDatas[i];
            if(data==null) continue;
            row["DbId"] = data.DbId;
            row["ExternalId"] = data.externalId!;
            row["Bbox"] = $"({data.bboxMin.Value.X},{data.bboxMin.Value.Y},{data.bboxMin.Value.Z},{data.bboxMax.Value.X},{data.bboxMax.Value.Y},{data.bboxMax.Value.Z}";
            // dt.Rows.Add(row);
            foreach (BIMProperty bimProperty in data.properties)
            {
                if (!dt.Columns.Contains(bimProperty.Name!))
                {
                    dt.Columns.Add(bimProperty.Name, typeof(string));
                }

                row[bimProperty.Name] = bimProperty.Value ?? string.Empty;
            }
            dt.Rows.Add(row);
        }
        return dt;
    }
    public PropDbReader GetPropDbReader(string token3Leg,string versionId)
    {
        string derivativeUrn = Base64Convert.ToBase64String(versionId);
        PropDbReader propDbReader = new PropDbReader(derivativeUrn, token3Leg);
        return propDbReader;
    }
    public PropDbReaderRevit GetPropDbReaderRevit(string token3Leg,string versionId)
    {
        string derivativeUrn = Base64Convert.ToBase64String(versionId);
        PropDbReaderRevit propDbReader = new PropDbReaderRevit(derivativeUrn, token3Leg);
        return propDbReader;
    }

    public void BatchExportAllRevitToExcelByFolder(string directory, string projectId, string folderId,
        bool isRecursive)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(directory);
        if(!directoryInfo.Exists) directoryInfo.Create();
        ExportRevitExcelRecursive(directory, projectId, folderId,isRecursive);
    }

    /// <summary>
    /// Generates a report of item versions in a specified folder within a project.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="folderId">The unique identifier of the folder within the project.</param>
    /// <param name="extenstion">The file extension to filter items by. Default is ".rvt".</param>
    /// <param name="isRecursive">A boolean value indicating whether to recursively search subfolders. Default is false.</param>
    /// <returns>
    /// A DataTable containing the report data. Each row represents an item in the folder (and subfolders if isRecursive is true),
    /// and includes the project ID, folder ID, item ID, item name, and latest version number.
    /// </returns>
    public DataTable BatchReportItemVersion(string projectId, string folderId,string extenstion=".rvt",bool isRecursive =false)
    {
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("ProjectId", typeof(string));
        dataTable.Columns.Add("FolderId", typeof(string));
        dataTable.Columns.Add("ItemName", typeof(string));
        dataTable.Columns.Add("ItemId", typeof(string));
        dataTable.Columns.Add("LatestVersion", typeof(long));
        BatchReportItemVersionRecursive(projectId, folderId,extenstion,ref dataTable,isRecursive);
        return dataTable;
    }
    private void BatchReportItemVersionRecursive(string projectId,string folderId,string extension, ref DataTable dt,bool isRecursive)
    {
        var foldersApi = new FoldersApi();
        // refresh token
        string get2LeggedToken = Auth.Authentication.Get2LeggedToken().Result;
        foldersApi.Configuration.AccessToken = get2LeggedToken;
        dynamic result = foldersApi.GetFolderContentsAsync(projectId, folderId).Result;
        get2LeggedToken = Auth.Authentication.Get2LeggedToken().Result;
        foreach (KeyValuePair<string, dynamic> itemInfo in new DynamicDictionaryItems(result.data))
        {
            string name = (string)itemInfo.Value.attributes.displayName;
            string id = (string)itemInfo.Value.id;
            if (itemInfo.Value.type == "items" && name.EndsWith(extension))
            {
                dynamic? item = GetLatestVersionItem(get2LeggedToken, projectId, id);
                string fileName = item?.attributes.displayName;
                long versionNumber = item?.attributes.versionNumber;
                string itemId = item?.relationships.item.data.id;
                DataRow row = dt.NewRow();
                row["ProjectId"] = projectId;
                row["FolderId"] = folderId;
                row["ItemName"] = fileName??string.Empty;
                row["ItemId"] = itemId??string.Empty;
                row["LatestVersion"] = versionNumber;
                dt.Rows.Add(row);
            }
            else if (itemInfo.Value.type == "folders" && isRecursive)
            {
                BatchReportItemVersionRecursive(projectId,id, extension,ref dt,isRecursive);
            }
        }
    }
    public void BatchExportAllRevitToExcel(string token2Leg,string directory,string hubId,string projectId,bool isRecursive)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(directory);
        if(!directoryInfo.Exists) directoryInfo.Create();
        (string, string) projectFilesFolder = GetTopProjectFilesFolder(token2Leg, hubId, projectId);
        string TopFolderId = projectFilesFolder.Item1;
        var foldersApi = new FoldersApi();
        foldersApi.Configuration.AccessToken = token2Leg;
        dynamic result = foldersApi.GetFolderContentsAsync(projectId, TopFolderId).Result;
        foreach (KeyValuePair<string, dynamic> itemInfo in new DynamicDictionaryItems(result.data))
        {
            string name = (string)itemInfo.Value.attributes.displayName;
            string id = (string)itemInfo.Value.id;
            // if type folder, recursive
            if (itemInfo.Value.type == "folders")
            {
                ExportRevitExcelRecursive(directory,projectId, id,isRecursive);
            }
        }

    }

    /// <summary>
    /// Exports data from a Revit project to an Excel file.
    /// </summary>
    /// <param name="token3Leg">The authentication token for accessing Revit data.</param>
    /// <param name="filePath">The file path where the Excel file will be saved.</param>
    /// <param name="versionId">The version identifier of the Revit project.</param>
    public void ExportRevitDataToExcel(string token3Leg,string filePath,string versionId)
    {
        PropDbReaderRevit propDbReaderRevit = GetPropDbReaderRevit(token3Leg, versionId);
        propDbReaderRevit.ExportAllDataToExcel(filePath);
    }
    private void ExportRevitExcelRecursive(string directory, string projectId, string folderId,bool isRecursive)
    {
        var foldersApi = new FoldersApi();
        // refresh token
        string get2LeggedToken = Auth.Authentication.Get2LeggedToken().Result;
        foldersApi.Configuration.AccessToken = get2LeggedToken;
        dynamic result = foldersApi.GetFolderContentsAsync(projectId, folderId).Result;
        foreach (KeyValuePair<string, dynamic> itemInfo in new DynamicDictionaryItems(result.data))
        {
            string name = (string)itemInfo.Value.attributes.displayName;
            string id = (string)itemInfo.Value.id;
            if (itemInfo.Value.type == "items" && name.EndsWith(".rvt"))
            {
                get2LeggedToken = Auth.Authentication.Get2LeggedToken().Result;
                dynamic? item = GetLatestVersionItem(get2LeggedToken, projectId, id);
                string versionId = item.id;
                string fileName = item.attributes.displayName;
                // remove .rvt
                fileName = fileName.Substring(0, fileName.Length - 4)+".xlsx";
                string filePath = Path.Combine(directory, fileName);
                // start write log
                LogUtils.Info("Start export " + fileName);
                var startTime = DateTime.Now;
                try
                {
                    PropDbReaderRevit propDbReaderRevit = GetPropDbReaderRevit(get2LeggedToken, versionId);

                    propDbReaderRevit.ExportAllDataToExcel(filePath);
                }
                catch (Exception e)
                {
                    LogUtils.Error("Error export " + fileName + " : " + e.Message);
                }
                var endtimestamp = DateTime.Now;
                var TotalTime = endtimestamp - startTime;
                LogUtils.Info("Export " + fileName + " done in " + TotalTime.TotalMinutes + " minutes");

            }
            else if (itemInfo.Value.type == "folders" && isRecursive)
            {
                ExportRevitExcelRecursive(directory, projectId, id,isRecursive);
            }
        }
    }
}