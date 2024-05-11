// Copyright (c) chuongmep.com. All rights reserved

using Autodesk.Forge;
using Autodesk.Forge.Core;
using Autodesk.Forge.DesignAutomation;
using Autodesk.Forge.DesignAutomation.Model;
using Autodesk.Forge.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Activity = Autodesk.Forge.DesignAutomation.Model.Activity;
using Parameter = Autodesk.Forge.DesignAutomation.Model.Parameter;

namespace APSToolkit.DesignAutomation;

public class RevitDesignAutomate
{
    // Design Automation v3 API
    private DesignAutomationClient _designAutomation;

    // Design Automation v3 Configuration
    private DesignAutomateConfiguration _configuration;

    public RevitDesignAutomate()
    {
        DesignAutomateConfiguration configuration = new DesignAutomateConfiguration();
        ForgeService service =
            new ForgeService(
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

    public RevitDesignAutomate(DesignAutomateConfiguration configuration) : this()
    {
        ForgeService service =
            new ForgeService(
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

    /// <summary>
    /// Execute job design automation
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="versionId"></param>
    /// <param name="callBackUrl"></param>
    /// <returns></returns>

    public async Task<Status> ExecuteJob(string projectId, string versionId, string callBackUrl)
    {
        // access Token
        Token token = new Auth(_configuration.ClientId, _configuration.ClientSecret)
            .Get2LeggedToken().Result;
        string userAccessToken = token.AccessToken;
        bool isCompositeDesign = DAUtils.IsCompositeDesign(userAccessToken, projectId, versionId).Result;
        Console.WriteLine("Is Composite Design: " + isCompositeDesign);
        _configuration.Version = DAUtils.GetRevitVersionByVersionId(userAccessToken, projectId, versionId).Result;
        if (isCompositeDesign)
        {
            _configuration.ActivityName += "Composite";
        }

        // ensure app bundle and activity exist
        EnsureConfigurations();
        await EnsureAppBundle().ConfigureAwait(false);
        await EnsureActivity(isCompositeDesign).ConfigureAwait(false);

        Status status = await ExecuteWorkItem(userAccessToken, projectId, versionId, callBackUrl)
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
                {Id = id, Version = 1};
            Autodesk.Forge.DesignAutomation.Model.Alias newAlias = await _designAutomation
                .CreateAppBundleAliasAsync(_configuration.AppName, aliasSpec).ConfigureAwait(false);
            Console.WriteLine("Created new alias version: " + newAlias.Id + " pointing to " + newAlias.Version);
            // upload the zip with .bundle
            UploadAppBundleBits(newAppVersion.UploadParameters, _configuration.PackageZipPath).Wait();
            // RestClient uploadClient = new RestClient(newAppVersion.UploadParameters.EndpointURL);
            // RestRequest request = new RestRequest(string.Empty);
            // request.Method = Method.Post;
            // request.AlwaysMultipartFormData = true;
            // foreach (KeyValuePair<string, string> x in newAppVersion.UploadParameters.FormData)
            //     request.AddParameter(x.Key, x.Value);
            // request.AddFile("file", _configuration.PackageZipPath);
            // request.AddHeader("Cache-Control", "no-cache");
            // Console.WriteLine("Uploading bundle zip...");
            // await uploadClient.ExecutePostAsync(request).ConfigureAwait(false);
            // Console.WriteLine("Uploaded app bundle: " + _configuration.PackageZipPath);
        }
    }

    public async Task UploadAppBundleBits(UploadAppBundleParameters uploadParameters, string packagePath)
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
                           {Content = formData})
                {
                    request.Options.Set(ForgeConfiguration.TimeoutKey, (int) TimeSpan.FromMinutes(10).TotalSeconds);
                    var response = await _designAutomation.Service.Client.SendAsync(request).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        Console.WriteLine("Uploaded app bundle: " + _configuration.PackageZipPath);
    }

    private async Task EnsureActivity(bool isCompositeDesign)
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

        if (!existActivity && !isCompositeDesign)
        {
            string commandLine =
                string.Format(
                    @"$(engine.path)\\revitcoreconsole.exe /i {0}$(args[inputFile].path){0} /al {0}$(appbundles[{1}].path){0}",
                    "\"", _configuration.AppName);
            Activity activitySpec = new Activity()
            {
                Id = _configuration.ActivityName,
                Appbundles = new List<string>() {_configuration.AppBundleFullName},
                CommandLine = new List<string>() {commandLine},
                Engine = _configuration.EngineName,
                Parameters = new Dictionary<string, Autodesk.Forge.DesignAutomation.Model.Parameter>()
                {
                    {
                        "inputFile",
                        new Autodesk.Forge.DesignAutomation.Model.Parameter()
                        {
                            Description = "Input Revit File", LocalName = "$(inputFile)", Ondemand = false,
                            Required = true, Verb = Verb.Get, Zip = false
                        }
                    },
                    {
                        "inputJson",
                        new Parameter()
                        {
                            Description = "input json", LocalName = "params.json", Ondemand = false, Required = false,
                            Verb = Verb.Get, Zip = false
                        }
                    },
                    {
                        "result",
                        new Autodesk.Forge.DesignAutomation.Model.Parameter()
                        {
                            Description = "Resulting JSON File",
                            LocalName = _configuration.ResultFileName + _configuration.ResultFileExt,
                            Ondemand = false,
                            Required = true,
                            Verb = Verb.Put,
                            Zip = false,
                        }
                    }
                },
                Settings = new Dictionary<string, ISetting>()
                {
                    {"script", new StringSetting() {Value = _configuration.Script}}
                },
                Description = _configuration.ActivityDescription,
            };
            Activity newActivity = await _designAutomation.CreateActivityAsync(activitySpec).ConfigureAwait(false);
            Console.WriteLine("Created new activity: " + newActivity.Id);
            // specify the alias for this Activity
            string id = _configuration.Alias.ToString().ToLower();
            Autodesk.Forge.DesignAutomation.Model.Alias aliasSpec = new Autodesk.Forge.DesignAutomation.Model.Alias()
                {Id = id, Version = 1};
            Autodesk.Forge.DesignAutomation.Model.Alias newAlias =
                await _designAutomation.CreateActivityAliasAsync(_configuration.ActivityName, aliasSpec)
                    .ConfigureAwait(false);
            Console.WriteLine($"Created new alias for activity Version: {newAlias.Version} ID: {newAlias.Id}: ");
        }
        else if (!existActivity && isCompositeDesign)
        {
            string commandLine =
                string.Format(
                    @"$(engine.path)\\revitcoreconsole.exe /i {0}$(args[inputFile].path){0} /al {0}$(appbundles[{1}].path){0}",
                    "\"", _configuration.AppName);
            Activity activitySpec = new Activity()
            {
                Id = _configuration.ActivityName,
                Appbundles = new List<string>() {_configuration.AppBundleFullName},
                CommandLine = new List<string>() {commandLine},
                Engine = _configuration.EngineName,
                Parameters = new Dictionary<string, Autodesk.Forge.DesignAutomation.Model.Parameter>()
                {
                    {
                        "inputFile",
                        new Parameter()
                        {
                            Description = "input file",
                            LocalName = "myzip",
                            Ondemand = false,
                            Required = true,
                            Verb = Verb.Get,
                            Zip = true
                        }
                    },
                    {
                        "inputJson",
                        new Parameter()
                        {
                            Description = "input json", LocalName = "params.json", Ondemand = false, Required = false,
                            Verb = Verb.Get, Zip = false
                        }
                    },
                    {
                        "result",
                        new Autodesk.Forge.DesignAutomation.Model.Parameter()
                        {
                            Description = "Resulting JSON File",
                            LocalName = _configuration.ResultFileName + _configuration.ResultFileExt,
                            Ondemand = false,
                            Required = true,
                            Verb = Verb.Put,
                            Zip = false,
                        }
                    }
                },
                Settings = new Dictionary<string, ISetting>()
                {
                    {"script", new StringSetting() {Value = _configuration.Script}}
                },
                Description = _configuration.ActivityDescription,
            };
            Activity newActivity = await _designAutomation.CreateActivityAsync(activitySpec).ConfigureAwait(false);
            Console.WriteLine("Created new activity: " + newActivity.Id);
            // specify the alias for this Activity
            string id = _configuration.Alias.ToString().ToLower();
            Autodesk.Forge.DesignAutomation.Model.Alias aliasSpec = new Autodesk.Forge.DesignAutomation.Model.Alias()
                {Id = id, Version = 1};
            Autodesk.Forge.DesignAutomation.Model.Alias newAlias =
                await _designAutomation.CreateActivityAliasAsync(_configuration.ActivityName, aliasSpec)
                    .ConfigureAwait(false);
            Console.WriteLine($"Created new alias for activity Version: {newAlias.Version} ID: {newAlias.Id}: ");
        }
    }

    /// <summary>
    /// Build download URL
    /// </summary>
    /// <param name="userAccessToken"></param>
    /// <param name="projectId"></param>
    /// <param name="versionId"></param>
    /// <returns></returns>
    private async Task<(XrefTreeArgument xrefTreeArgument, string modelName)> BuildInputFileURL(string userAccessToken,
        string projectId, string versionId)
    {
        VersionsApi versionApi = new VersionsApi();
        versionApi.Configuration.AccessToken = new Auth(_configuration.ClientId, _configuration.ClientSecret).Get2LeggedToken().Result.AccessToken;
        dynamic version = await versionApi.GetVersionAsync(projectId, versionId).ConfigureAwait(false);
        dynamic versionItem = await versionApi.GetVersionItemAsync(projectId, versionId).ConfigureAwait(false);
        string modelName = version.data.attributes.name;
        bool isCompositeDesign = version.data.attributes.extension.data.isCompositeDesign;
        string[] versionItemParams = ((string) version.data.relationships.storage.data.id).Split('/');
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
                    {"Authorization", "Bearer " + userAccessToken}
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
                {"Authorization", "Bearer " + userAccessToken}
            }
        };
        return (treeArgument, modelName);
    }

    private async Task<XrefTreeArgument> BuildUploadFileURL(string token)
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
            Url = (string) (signedUrl.Data.signedUrl),
            Verb = Verb.Put,
        };
    }

    /// <summary>
    /// Execute work item
    /// </summary>
    /// <param name="forgeToken">token forge</param>
    /// <param name="projectId">id of project(eg: b.xxxx)</param>
    /// <param name="versionId">Urn version id eg:urn:adsk.wipprod:fs.file:vf.rXJhqUE6QamM6ANpG2Qz6w?version=2</param>
    /// <param name="callBackUrl">call back url on complete</param>
    /// <returns>status of execute work item</returns>
    public async Task<Status> ExecuteWorkItem(string forgeToken, string projectId, string versionId,
        string callBackUrl)
    {
        Console.WriteLine("ActivityId: " + _configuration.ActivityFullName);
        (XrefTreeArgument xrefTreeArgument, string modelName) buildInputFileUrl =
            await BuildInputFileURL(forgeToken, projectId, versionId).ConfigureAwait(false);
        Console.WriteLine("Download URL: " + buildInputFileUrl.xrefTreeArgument.Url);
        XrefTreeArgument treeArgument = await BuildUploadFileURL(forgeToken).ConfigureAwait(false);
        Console.WriteLine("TreeArgument: " + treeArgument.Url);
        dynamic inputJson = new JObject();
        inputJson.ProjectId = projectId;
        inputJson.VersionId = versionId;
        inputJson.ModelName = buildInputFileUrl.modelName;
        inputJson.ForgeToken = forgeToken;
        XrefTreeArgument inputJsonArgument = new XrefTreeArgument()
        {
            Url = "data:application/json, " + ((JObject)inputJson).ToString(Formatting.None).Replace("\"", "'")
        };
        WorkItem workItemSpec = new WorkItem()
        {
            ActivityId = _configuration.ActivityFullName,
            Arguments = new Dictionary<string, IArgument>()
            {
                {"inputFile", buildInputFileUrl.xrefTreeArgument},
                {"inputJson", inputJsonArgument},
                {"result", treeArgument},
                {"onComplete", new XrefTreeArgument {Verb = Verb.Post, Url = callBackUrl}}
            },
            LimitProcessingTimeSec = 3600,
        };
        Console.WriteLine("Start Create WorkItem");
        WorkItemStatus workItemStatus = await _designAutomation.CreateWorkItemAsync(workItemSpec).ConfigureAwait(false);
        Console.WriteLine("WorkItemStatus: " + workItemStatus.Status);
        Console.WriteLine("Working Item Id: " + workItemStatus.Id);
        // wait until the work item is finished
        Status status = default;
        while (!workItemStatus.Status.IsDone())
        {
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            workItemStatus = await _designAutomation.GetWorkitemStatusAsync(workItemStatus.Id).ConfigureAwait(false);
            Console.WriteLine("WorkItemStatus: " + workItemStatus.Status);
            Console.WriteLine("Result:" + workItemStatus.ReportUrl);
            if (workItemStatus.ReportUrl != null)
                WriteReportTxt(workItemStatus.ReportUrl);
            status = workItemStatus.Status;
        }
        return status;
    }
    private static void WriteReportTxt(string reportUrl)
    {
        var client = new RestClient(reportUrl);
        var request = new RestRequest() {Method = Method.Get};
        var response = client.Execute(request);
        if (response.IsSuccessful)
        {
            string? report = response.Content;
            Console.WriteLine(report);
        }
        else
        {
            Console.WriteLine("Error: " + response.ErrorMessage);
        }
    }
    /// <summary>
    /// Base64 encode
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    public static string Base64Encode(string plainText)
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
