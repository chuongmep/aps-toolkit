// Copyright (c) chuongmep.com. All rights reserved

using APSToolkit.Auth;
using APSToolkit.Database;
using APSToolkit.Utils;
using Autodesk.Forge;
using Autodesk.Forge.Core;
using Autodesk.Forge.DesignAutomation;
using Autodesk.Forge.DesignAutomation.Model;
using Autodesk.Forge.Model;
using Activity = Autodesk.Forge.DesignAutomation.Model.Activity;
using Parameter = Autodesk.Forge.DesignAutomation.Model.Parameter;

namespace APSToolkit.DesignAutomation;

public class DynamoRevitDesignAutomate
{
    // Design DesignAutomation v3 API
    private DesignAutomationClient _designAutomation;

    // Design DesignAutomation v3 Configuration
    private DesignAutomateConfiguration _configuration;

    public DynamoRevitDesignAutomate()
    {
        DesignAutomateConfiguration configuration = new DesignAutomateConfiguration();
        Autodesk.Forge.Core.ForgeService service =
            new Autodesk.Forge.Core.ForgeService(
                new HttpClient(
                    new ForgeHandler(Microsoft.Extensions.Options.Options.Create(new ForgeConfiguration()
                    {
                        ClientId = configuration.ClientId,
                        ClientSecret = configuration.ClientSecret
                    }))
                    {
                        InnerHandler = new HttpClientHandler()
                    })
            );
        _designAutomation = new DesignAutomationClient(service);
    }

    public DynamoRevitDesignAutomate(DesignAutomateConfiguration configuration) : this()
    {
        Autodesk.Forge.Core.ForgeService service =
            new Autodesk.Forge.Core.ForgeService(
                new HttpClient(
                    new ForgeHandler(Microsoft.Extensions.Options.Options.Create(new ForgeConfiguration()
                    {
                        ClientId = configuration.ClientId,
                        ClientSecret = configuration.ClientSecret
                    }))
                    {
                        InnerHandler = new HttpClientHandler()
                    })
            );
        _designAutomation = new DesignAutomationClient(service);
        configuration.Engine = Engine.Revit;
        _configuration = configuration;
    }

    public async Task<Status> ExecuteJob(string token2Leg, string projectId,
        string versionId, string inputZipPath)
    {
        EnsureConfigurations();
        await EnsureAppBundle().ConfigureAwait(false);
        await EnsureActivity().ConfigureAwait(false);
        Status status = await ExecuteWorkItem(token2Leg, projectId, versionId, inputZipPath)
            .ConfigureAwait(false);
        return status;
    }

    private void EnsureConfigurations()
    {
        if (string.IsNullOrEmpty(_configuration.AppName))
            throw new Exception("Missing AppName");
        if (string.IsNullOrEmpty(_configuration.PackageZipPath))
            throw new Exception("Missing PackageZipPath");
        if (string.IsNullOrEmpty(_configuration.EngineName))
            throw new Exception("Missing EngineName");
        if (string.IsNullOrEmpty(_configuration.AppBundleFullName))
            throw new Exception("Missing AppBundleFullName");
        if (string.IsNullOrEmpty(_configuration.BundleDescription))
            throw new Exception("Missing Description");
        if (string.IsNullOrEmpty(nameof(_configuration.Alias)))
            throw new Exception("Missing Alias");
    }

    private async Task EnsureAppBundle()
    {
        // get the list and check for the name
        Console.WriteLine("Retrieving app bundles");
        Page<string> appBundles = await _designAutomation.GetAppBundlesAsync().ConfigureAwait(false);
        bool existAppBundle = false;
        foreach (string appName in appBundles.Data)
        {
            if (appName.Contains(_configuration.AppBundleFullName))
            {
                existAppBundle = true;
                Console.WriteLine("Found existing app bundle: " + appName);
                break;
            }
        }

        if (!existAppBundle)
        {
            // check if ZIP with bundle is here
            Console.WriteLine("Start Create new app bundle");
            if (!File.Exists(_configuration.PackageZipPath))
                throw new Exception("zip bundle not found at " + _configuration.PackageZipPath);

            AppBundle appBundleSpec = new AppBundle()
            {
                Package = _configuration.AppName,
                Engine = _configuration.EngineName,
                Id = _configuration.AppName,
                Description = _configuration.BundleDescription
            };
            AppBundle newAppVersion = await _designAutomation.CreateAppBundleAsync(appBundleSpec).ConfigureAwait(false);
            if (newAppVersion == null) throw new Exception("Cannot create new app");
            Console.WriteLine("Created new bundle: " + newAppVersion.Appbundles);
            // create alias pointing to v1
            string id = _configuration.Alias.ToString().ToLower();
            Autodesk.Forge.DesignAutomation.Model.Alias aliasSpec = new Autodesk.Forge.DesignAutomation.Model.Alias()
                { Id = id, Version = 1 };
            Autodesk.Forge.DesignAutomation.Model.Alias newAlias = await _designAutomation
                .CreateAppBundleAliasAsync(_configuration.AppName, aliasSpec).ConfigureAwait(false);
            Console.WriteLine("Created new alias version: " + newAlias.Id + " pointing to " + newAlias.Version);
            // upload the zip with .bundle
            UploadAppBundleBits(newAppVersion.UploadParameters, _configuration.PackageZipPath).Wait();
        }
    }

    private async Task UploadAppBundleBits(UploadAppBundleParameters uploadParameters, string packagePath)
    {
        using (var formData = new MultipartFormDataContent())
        {
            foreach (var kv in uploadParameters.FormData)
            {
                if (kv.Value != null)
                {
                    formData.Add(new StringContent(kv.Value), kv.Key);
                }
            }

            using (var content = new StreamContent(new FileStream(packagePath, FileMode.Open)))
            {
                formData.Add(content, "file");

                using (var request = new HttpRequestMessage(HttpMethod.Post, uploadParameters.EndpointURL)
                           { Content = formData })
                {
                    request.Options.Set(ForgeConfiguration.TimeoutKey, (int)TimeSpan.FromMinutes(10).TotalSeconds);
                    var response = await _designAutomation.Service.Client.SendAsync(request).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        Console.WriteLine("Uploaded app bundle: " + _configuration.PackageZipPath);
    }

    private async Task EnsureActivity()
    {
        Console.WriteLine("Start Retrieving activity");
        Page<string> activities = await _designAutomation.GetActivitiesAsync().ConfigureAwait(false);
        bool existActivity = false;
        foreach (string activity in activities.Data)
        {
            if (activity.Contains(_configuration.ActivityFullName))
            {
                existActivity = true;
                Console.WriteLine("Found existing activity, id : " + activity);
                break;
            }
        }

        if (!existActivity)
        {
            string commandLine =
                string.Format(
                    @"$(engine.path)\\revitcoreconsole.exe /i {0}$(args[inputFile].path){0} /al {0}$(appbundles[{1}].path){0}",
                    "\"", _configuration.AppName);
            Activity activitySpec = new Activity()
            {
                Id = _configuration.ActivityName,
                Appbundles = new List<string>() { _configuration.AppBundleFullName },
                CommandLine = new List<string>() { commandLine },
                Engine = _configuration.EngineName,
                Parameters = new Dictionary<string, Autodesk.Forge.DesignAutomation.Model.Parameter>()
                {
                    {
                        "inputFile",
                        new Parameter()
                        {
                            Description = "Input Revit model",
                            Ondemand = false,
                            Required = true,
                            Verb = Verb.Get,
                            Zip = false
                        }
                    },
                    {
                        "input",
                        new Parameter()
                        {
                            Description = "Input Dynamo graph(s) and supporting files",
                            LocalName = "input.zip",
                            Ondemand = false,
                            Required = true,
                            Verb = Verb.Get,
                            Zip = true
                        }
                    },
                    {
                        "result",
                        new Autodesk.Forge.DesignAutomation.Model.Parameter()
                        {
                            Description = "Graph result",
                            LocalName = "result",
                            Ondemand = false,
                            Required = true,
                            Verb = Verb.Put,
                            Zip = true,
                        }
                    }
                },
                Settings = new Dictionary<string, ISetting>()
                {
                    { "script", new StringSetting() { Value = _configuration.Script } }
                },
                Description = _configuration.ActivityDescription,
            };
            Activity newActivity = await _designAutomation.CreateActivityAsync(activitySpec).ConfigureAwait(false);
            Console.WriteLine("Created new activity: " + newActivity.Id);
            // specify the alias for this Activity
            string id = _configuration.Alias.ToString().ToLower();
            Autodesk.Forge.DesignAutomation.Model.Alias aliasSpec = new Autodesk.Forge.DesignAutomation.Model.Alias()
                { Id = id, Version = 1 };
            Autodesk.Forge.DesignAutomation.Model.Alias newAlias =
                await _designAutomation.CreateActivityAliasAsync(_configuration.ActivityName, aliasSpec)
                    .ConfigureAwait(false);
            Console.WriteLine($"Created new alias for activity Version: {newAlias.Version} ID: {newAlias.Id}: ");
        }
    }

    private async Task<XrefTreeArgument> BuildUploadURL(string token)
    {
        string bucketName = _configuration.BucketOutputName;
        // just allow bucketName -_.a-z0-9]{3,128}, remove any other special characters
        var regex = new System.Text.RegularExpressions.Regex("[-_.a-z0-9]{3,128}");
        // remove any other special characters
        bucketName = regex.Match(bucketName.ToLower()).Value;
        string resultFilename = Base64Encode(_configuration.ResultFileName) + _configuration.ResultFileExt;
        BucketsApi buckets = new BucketsApi();
        buckets.Configuration.AccessToken = token;
        PostBucketsPayload bucketPayload =
            new PostBucketsPayload(bucketName, null, PostBucketsPayload.PolicyKeyEnum.Transient);
        try
        {
            string xAdsRegion = "US";
            await buckets.CreateBucketAsync(bucketPayload, xAdsRegion).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // ignore bucket created
        }

        ObjectsApi objects = new ObjectsApi();
        dynamic signedUrl = await objects
            .CreateSignedResourceAsyncWithHttpInfo(bucketName, resultFilename, new PostBucketsSigned(30), "readwrite")
            .ConfigureAwait(false);

        return new XrefTreeArgument()
        {
            Url = (string)(signedUrl.Data.signedUrl),
            Verb = Verb.Put
        };
    }

    private async Task<Status> ExecuteWorkItem(string token2Leg, string projectId,
        string versionId, string inputZipPath)
    {
        Console.WriteLine("ActivityId: " + _configuration.ActivityFullName);
        XrefTreeArgument treeArgument = await BuildUploadURL(token2Leg).ConfigureAwait(false);
        Console.WriteLine("TreeArgument: " + treeArgument.Url);
        XrefTreeArgument inputParamJsonArgument =
            BuildInputRevitFileURL(token2Leg, projectId, versionId).Result.xrefTreeArgument;
        XrefTreeArgument buildInputZipArgument = BuildInputZipArgument(token2Leg,inputZipPath);
        WorkItem workItemSpec = new WorkItem()
        {
            ActivityId = _configuration.ActivityFullName,
            Arguments = new Dictionary<string, IArgument>()
            {
                { "inputFile", inputParamJsonArgument },
                { "input", buildInputZipArgument},
                { "result", treeArgument }
            },
            LimitProcessingTimeSec = 3600,
        };
        Console.WriteLine("Start Create WorkItem");
        WorkItemStatus workItemStatus = await _designAutomation.CreateWorkItemAsync(workItemSpec).ConfigureAwait(false);
        Console.WriteLine("WorkItemStatus: " + workItemStatus.Status);
        Console.WriteLine("Working Item Id: " + workItemStatus.Id);
        // wait until the work item is finished
        while (!workItemStatus.Status.IsDone())
        {
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            workItemStatus = await _designAutomation.GetWorkitemStatusAsync(workItemStatus.Id).ConfigureAwait(false);
            if (workItemStatus.Status != Status.Success && workItemStatus.Status != Status.Inprogress)
            {
                Console.WriteLine("Status: " + workItemStatus.Status);
                Console.WriteLine(workItemStatus.ReportUrl);
            }
        }

        Console.WriteLine("WorkItemStatus: " + workItemStatus.Status);
        Console.WriteLine("Result:" + workItemStatus.ReportUrl);
        // download the report and write console
        LogUtils.WriteConsoleWorkItemReport(workItemStatus.ReportUrl);
        return workItemStatus.Status;
    }

    /// <summary>
    /// Build download URL
    /// </summary>
    /// <param name="userAccessToken"></param>
    /// <param name="projectId"></param>
    /// <param name="versionId"></param>
    /// <returns></returns>
    private async Task<(XrefTreeArgument xrefTreeArgument, string modelName)> BuildInputRevitFileURL(
        string userAccessToken,
        string projectId, string versionId)
    {
        VersionsApi versionApi = new VersionsApi();
        versionApi.Configuration.AccessToken = await Authentication.Get2LeggedToken().ConfigureAwait(false);
        dynamic version = await versionApi.GetVersionAsync(projectId, versionId).ConfigureAwait(false);
        dynamic versionItem = await versionApi.GetVersionItemAsync(projectId, versionId).ConfigureAwait(false);
        string modelName = version.data.attributes.name;
        bool isCompositeDesign = version.data.attributes.extension.data.isCompositeDesign;
        string[] versionItemParams = ((string)version.data.relationships.storage.data.id).Split('/');
        string[] bucketKeyParams = versionItemParams[versionItemParams.Length - 2].Split(':');
        string bucketKey = bucketKeyParams[bucketKeyParams.Length - 1];
        string objectName = versionItemParams[versionItemParams.Length - 1];
        string downloadUrl = $"https://developer.api.autodesk.com/oss/v2/buckets/{bucketKey}/objects/{objectName}";

        if (isCompositeDesign)
        {
            XrefTreeArgument xrefTreeArgument = new XrefTreeArgument()
            {
                Url = downloadUrl,
                Verb = Verb.Get,
                LocalName = "myzip",
                PathInZip = modelName,
                Headers = new Dictionary<string, string>()
                {
                    { "Authorization", "Bearer " + userAccessToken }
                }
            };
            return (xrefTreeArgument, modelName);
        }

        XrefTreeArgument treeArgument = new XrefTreeArgument()
        {
            Url = downloadUrl,
            Verb = Verb.Get,
            Headers = new Dictionary<string, string>()
            {
                { "Authorization", "Bearer " + userAccessToken }
            }
        };
        return (treeArgument, modelName);
    }

    private XrefTreeArgument BuildInputZipArgument(string token,string inputZipPath)
    {
        // post url to upload zip file
        BucketStorage bucketStorage = new BucketStorage();
        string bucketName = "test_data";
        bucketStorage.UploadFileToBucket(token, bucketName,inputZipPath);
        var url = bucketStorage.GetFileSignedUrl(token, bucketName,"input.zip");
        XrefTreeArgument treeArgument = new XrefTreeArgument()
        {
            Url = url,
            PathInZip = "input.zip",
            LocalName = "input.zip",
            Verb = Verb.Get,
            Headers = new Dictionary<string, string>()
            {
                { "Authorization", "Bearer " + token }
            }
        };
        return treeArgument;
    }

    /// <summary>
    /// Base64 encode
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    private static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes).Replace("/", "_");
    }

    public static string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData.Replace("_", "/"));
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }
}