// Copyright (c) chuongmep.com. All rights reserved

using System.IO.Compression;
using System.Text.RegularExpressions;
using APSToolkit.Schema;
using Autodesk.Forge;
using Autodesk.Forge.Model;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace APSToolkit;

/// <summary>
/// Use one part of https://github.com/Autodesk-Forge/forge-bucketsmanager-desktop
/// </summary>
public static class Derivatives
{
    private const string BaseUrl = "https://developer.api.autodesk.com/";
    private const string DerivativePath = "derivativeservice/v2/derivatives/";

    public struct Resource
    {
        /// <summary>
        /// File name (no path)
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Remove path to download (must add developer.api.autodesk.com prefix)
        /// </summary>
        public string RemotePath { get; set; }

        /// <summary>
        /// Path to save file locally
        /// </summary>
        public string LocalPath { get; set; }
    }

    private static readonly string[] Roles =
    {
        "Autodesk.CloudPlatform.DesignDescription",
        "Autodesk.CloudPlatform.PropertyDatabase",
        "Autodesk.CloudPlatform.IndexableContent",
        "leaflet-zip",
        "thumbnail",
        "graphics",
        "preview",
        "raas",
        "pdf",
        "lod",
    };

    /// <summary>
    /// Asynchronously reads geometry metadata from a remote source based on the provided URN and access token.
    /// </summary>
    /// <param name="urn">The URL-safe Base64 encoded URN (Unique Resource Name) of the source design.</param>
    /// <param name="accessToken">The access token for authentication.</param>
    /// <returns>
    /// A dictionary containing geometry metadata parsed from the remote source. The keys are file paths,
    /// and the values are arrays of objects implementing the ISvfGeometryMetadata interface.
    /// </returns>
    /// <remarks>
    /// This method extracts the geometry metadata file (GeometryMetadata.pf) from the specified URN using the
    /// provided access token. It then parses the contents of the file into geometry metadata objects and
    /// organizes them in a dictionary. The keys represent the file paths, and the values are arrays of objects
    /// implementing the ISvfGeometryMetadata interface.
    /// </remarks>
    public static async Task<Dictionary<string, ISvfGeometryMetadata[]>> ReadGeometriesRemoteAsync(string? urn,
        string accessToken)
    {
        List<Resource> resourcesToDownload = await ExtractSvfAsync(urn, accessToken).ConfigureAwait(false);
        List<string> fileNames = new List<string>()
        {
            "GeometryMetadata.pf"
        };
        resourcesToDownload = resourcesToDownload.FindAll(r => fileNames.Contains(r.FileName));
        Dictionary<string, ISvfGeometryMetadata[]> geometries = new Dictionary<string, ISvfGeometryMetadata[]>();
        foreach (Resource resource in resourcesToDownload)
        {
            // prepare the GET to download the file
            RestRequest request = new RestRequest(resource.RemotePath);
            request.Method = Method.Get;
            request.AddHeader("Authorization", "Bearer " + accessToken);
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            await ExecuteRequestData(request, resource).ContinueWith((task) =>
            {
                geometries.Add(resource.RemotePath, Geometries.ParseGeometries(task.Result));
            }).ConfigureAwait(false);
        }

        return geometries;
    }

    /// <summary>
    /// Asynchronously reads fragments from a remote source based on the provided URN and access token.
    /// </summary>
    /// <param name="urn">The URL-safe Base64 encoded URN (Unique Resource Name) of the source design.</param>
    /// <param name="accessToken">The access token for authentication.</param>
    /// <returns>
    /// A dictionary containing fragments parsed from the remote source. The keys are file paths,
    /// and the values are arrays of objects implementing the ISvfFragment interface.
    /// </returns>
    /// <remarks>
    /// This method extracts the fragment file (FragmentList.pack) from the specified URN using the
    /// provided access token. It then parses the contents of the file into fragment objects and
    /// organizes them in a dictionary. The keys represent the file paths, and the values are arrays
    /// of objects implementing the ISvfFragment interface.
    /// </remarks>
    public static async Task<Dictionary<string, ISvfFragment[]>> ReadFragmentsRemoteAsync(string? urn,
        string accessToken)
    {
        List<Resource> resourcesToDownload = await ExtractSvfAsync(urn, accessToken).ConfigureAwait(false);
        List<string> fileNames = new List<string>()
        {
            "FragmentList.pack"
        };
        // filter the list of resources to download
        resourcesToDownload = resourcesToDownload.FindAll(r => fileNames.Contains(r.FileName));
        Dictionary<string, ISvfFragment[]> fragments = new Dictionary<string, ISvfFragment[]>();
        foreach (Resource resource in resourcesToDownload)
        {
            // prepare the GET to download the file
            RestRequest request = new RestRequest(resource.RemotePath);
            request.Method = Method.Get;
            request.AddHeader("Authorization", "Bearer " + accessToken);
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            await ExecuteRequestData(request, resource).ContinueWith((task) =>
            {
                fragments.Add(resource.RemotePath, Fragments.ParseFragments(task.Result));
            }).ConfigureAwait(false);
        }

        return fragments;
    }

    /// <summary>
    /// Executes an asynchronous GET request to download data from a specified resource.
    /// </summary>
    /// <param name="request">The REST request object containing details of the request.</param>
    /// <param name="resource">The resource information to be downloaded.</param>
    /// <returns>
    /// A byte array representing the raw data downloaded from the specified resource.
    /// Returns null if the request encounters an error.
    /// </returns>
    /// <remarks>
    /// This method sends an asynchronous GET request to the Autodesk Forge API endpoint to download
    /// data from a specific resource. The response is checked for success, and the raw bytes are
    /// returned if the download is successful. If an error occurs during the request, null is
    /// returned, and an error message is printed to the console.
    /// </remarks>
    public static async Task<byte[]?> ExecuteRequestData(RestRequest request, Resource resource)
    {
        var client = new RestClient("https://developer.api.autodesk.com/");
        var response = await client.ExecuteGetAsync(request).ConfigureAwait(false);
        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            // something went wrong with this file...
            Console.WriteLine(string.Format("Error downloading {0}: {1}", resource.FileName,
                response.StatusCode.ToString()));
            // Handle the error as needed
            return null;
        }

        return response.RawBytes;
    }

    /// <summary>
    /// Asynchronously saves files from the SVF (Simple Viewer Format) extraction to the specified local folder.
    /// </summary>
    /// <param name="folderPath">The local folder path where files will be saved.</param>
    /// <param name="urn">The URL-safe Base64 encoded URN (Unique Resource Name) of the source design.</param>
    /// <param name="accessToken">The access token for authentication with the Autodesk Forge API.</param>
    /// <remarks>
    /// This method asynchronously extracts SVF resources associated with the specified design
    /// (identified by the URN) and saves them to the local folder provided. Each resource is downloaded
    /// individually, and the local file structure mirrors the structure in the SVF archive. If an error
    /// occurs during the download, an error message is printed to the console.
    /// </remarks>
    public static async Task SaveFileSvfAsync(string folderPath, string? urn, string accessToken)
    {
        List<Resource> resourcesToDownload = await ExtractSvfAsync(urn, accessToken).ConfigureAwait(false);
        var client = new RestClient("https://developer.api.autodesk.com/");
        foreach (var resource in resourcesToDownload)
        {
            // prepare the GET to download the file
            RestRequest request = new RestRequest(resource.RemotePath);
            request.Method = Method.Get;
            request.AddHeader("Authorization", "Bearer " + accessToken);
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            var response = await client.ExecuteGetAsync(request).ConfigureAwait(false);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                // something went wrong with this file...
                Console.WriteLine(string.Format("Error downloading {0}: {1}",
                    resource.FileName, response.StatusCode.ToString()));

                // any other action?
            }
            else
            {
                // combine with selected local path
                string pathToSave = Path.Combine(folderPath, resource.LocalPath);
                // ensure local dir exists
                Directory.CreateDirectory(Path.GetDirectoryName(pathToSave));
                // save file
                File.WriteAllBytes(pathToSave, response.RawBytes);
            }
        }
    }

    /// <summary>
    /// Asynchronously extracts SVF (Simple Viewer Format) resources for a design identified by the given URN.
    /// </summary>
    /// <param name="urn">The URL-safe Base64 encoded URN (Unique Resource Name) of the source design.</param>
    /// <param name="accessToken">The access token for authentication with the Autodesk Forge API.</param>
    /// <returns>A list of resources representing files associated with the SVF extraction.</returns>
    /// <remarks>
    /// This method fetches the manifest for the specified design URN, identifies different types of
    /// derivatives (SVF, F2D, DB, etc.), and organizes the list of resources for external usage.
    /// Each resource in the list includes file details such as file name, remote path, and local path.
    /// </remarks>
    public static async Task<List<Resource>> ExtractSvfAsync(string? urn, string accessToken)
    {
        DerivativesApi derivativeApi = new DerivativesApi();
        derivativeApi.Configuration.AccessToken = accessToken;

        // get the manifest for the URN
        dynamic manifest = await derivativeApi.GetManifestAsync(urn).ConfigureAwait(false);

        // list items of the manifest file
        List<ManifestItem> urns = ParseManifest(manifest.derivatives);
        // iterate on what's on the file
        foreach (ManifestItem item in urns)
        {
            switch (item.MIME)
            {
                case "application/autodesk-svf":
                    item.Path.Files = await SvfDerivatives(item, accessToken).ConfigureAwait(false);
                    break;
                case "application/autodesk-f2d":
                    item.Path.Files = await F2DDerivatives(item, accessToken);
                    break;
                case "application/autodesk-db":
                    item.Path.Files = new List<string>()
                    {
                        "objects_attrs.json.gz",
                        "objects_vals.json.gz",
                        "objects_offs.json.gz",
                        "objects_ids.json.gz",
                        "objects_avs.json.gz",
                        item.Path.RootFileName
                    };
                    break;
                default:
                    item.Path.Files = new List<string>()
                    {
                        item.Path.RootFileName
                    };
                    break;
            }
        }

        // now organize the list for external usage
        List<Resource> resources = new List<Resource>();
        foreach (ManifestItem item in urns)
        {
            foreach (string file in item.Path.Files)
            {
                Uri myUri = new Uri(new Uri(item.Path.BasePath), file);
                resources.Add(new Resource()
                {
                    FileName = file,
                    RemotePath = DerivativePath + Uri.UnescapeDataString(myUri.AbsoluteUri),
                    LocalPath = Path.Combine(item.Path.LocalPath, file)
                });
            }
        }

        return resources;
    }

    public static async Task<List<Resource>> ExtractProbDbAsync(string? urn, string accessToken)
    {
        DerivativesApi derivativeApi = new DerivativesApi();
        derivativeApi.Configuration.AccessToken = accessToken;

        // get the manifest for the URN
        dynamic manifest = await derivativeApi.GetManifestAsync(urn).ConfigureAwait(false);

        // list items of the manifest file
        List<ManifestItem> urns = ParseManifest(manifest.derivatives);
        urns = urns.Where(x => x.MIME == "application/autodesk-db").ToList();
        // iterate on what's on the file
        List<Resource> resources = new List<Resource>();
        foreach (ManifestItem item in urns)
        {
            item.Path.Files = new List<string>()
            {
                "objects_attrs.json.gz",
                "objects_vals.json.gz",
                "objects_offs.json.gz",
                "objects_ids.json.gz",
                "objects_avs.json.gz",
            };
            foreach (string file in item.Path.Files)
            {
                Uri myUri = new Uri(new Uri(item.Path.BasePath), file);
                resources.Add(new Resource()
                {
                    FileName = file,
                    RemotePath = DerivativePath + Uri.UnescapeDataString(myUri.AbsoluteUri),
                    LocalPath = Path.Combine(item.Path.LocalPath, file)
                });
            }
        }
        return resources;
    }
    public static async Task<List<Resource>> ExtractDataBaseAsync(string? urn, string token)
    {
        DerivativesApi derivativeApi = new DerivativesApi();
        derivativeApi.Configuration.AccessToken = token;

        // get the manifest for the URN
        dynamic manifest = await derivativeApi.GetManifestAsync(urn).ConfigureAwait(false);

        // list items of the manifest file
        List<ManifestItem> urns = ParseManifest(manifest.derivatives);
        urns = urns.Where(x => x.MIME == "application/autodesk-db").ToList();
        // iterate on what's on the file
        List<Resource> resources = new List<Resource>();
        foreach (ManifestItem item in urns)
        {
            item.Path.Files = new List<string>()
            {
                item.Path.RootFileName,
            };
            foreach (string file in item.Path.Files)
            {
                Uri myUri = new Uri(new Uri(item.Path.BasePath), file);
                resources.Add(new Resource()
                {
                    FileName = file,
                    RemotePath = DerivativePath + Uri.UnescapeDataString(myUri.AbsoluteUri),
                    LocalPath = Path.Combine(item.Path.LocalPath, file)
                });
            }
        }
        return resources;
    }



    /// <summary>
    /// Asynchronously retrieves SVF (Simple Viewer Format) derivatives for a specific design identified by the given URN.
    /// </summary>
    /// <param name="item">The manifest item representing the design.</param>
    /// <param name="accessToken">The access token for authentication with the Autodesk Forge API.</param>
    /// <returns>A list of file names associated with the SVF derivatives.</returns>
    /// <remarks>
    /// This method fetches the SVF derivatives for the specified design URN using the provided access token.
    /// The list includes the URN as well as additional files associated with the SVF, such as asset files.
    /// </remarks>
    private static async Task<List<string>> SvfDerivatives(ManifestItem item, string accessToken)
    {
        JObject manifest = await GetDerivativeAsync(item.Path.Urn, accessToken).ConfigureAwait(false);

        List<string> files = new List<string>();
        files.Add(item.Path.Urn.Substring(item.Path.BasePath.Length));

        files.AddRange(GetAssets(manifest));

        return files;
    }

    /// <summary>
    /// Asynchronously retrieves F2D (2D Viewer Format) derivatives for a specific design identified by the given URN.
    /// </summary>
    /// <param name="item">The manifest item representing the design.</param>
    /// <param name="accessToken">The access token for authentication with the Autodesk Forge API.</param>
    /// <returns>A list of file names associated with the F2D derivatives.</returns>
    /// <remarks>
    /// This method fetches the F2D derivatives for the specified design URN using the provided access token.
    /// The list includes the manifest file and additional files associated with the F2D, such as asset files.
    /// </remarks>
    private static async Task<List<string>> F2DDerivatives(ManifestItem item, string accessToken)
    {
        JObject manifest = await GetDerivativeAsync(item.Path.BasePath + "manifest.json.gz", accessToken)
            .ConfigureAwait(false);

        List<string> files = new List<string>();
        files.Add("manifest.json.gz");

        files.AddRange(GetAssets(manifest));

        return files;
    }

    /// <summary>
    /// Extracts asset URIs from the provided JSON manifest, excluding embedded assets.
    /// </summary>
    /// <param name="manifest">The JSON manifest containing information about assets.</param>
    /// <returns>A list of asset file names extracted from the manifest, excluding embedded assets.</returns>
    /// <remarks>
    /// This method iterates through the "assets" section of the JSON manifest and retrieves the URIs of the assets.
    /// Assets with URIs containing "embed:/" are skipped, as they are considered embedded and are not included in the result.
    /// The resulting list represents the names of non-embedded asset files.
    /// </remarks>
    private static List<string> GetAssets(JObject manifest)
    {
        List<string> files = new List<string>();

        // for each "asset" on the manifest, add to the list of files (skip embed)
        foreach (JObject asset in manifest["assets"])
        {
            System.Diagnostics.Debug.WriteLine(asset["URI"].Value<string>());
            if (asset["URI"].Value<string>().Contains("embed:/")) continue;
            files.Add(asset["URI"].Value<string>());
        }

        return files;
    }

    /// <summary>
    /// Downloads the derivative manifest file, extracts its content, and returns it as a JObject.
    /// </summary>
    /// <param name="manifest">The path to the manifest file, including the base URL.</param>
    /// <param name="accessToken">The access token required for authorization.</param>
    /// <returns>A JObject representing the content of the extracted manifest file.</returns>
    /// <remarks>
    /// This method performs a GET request to download the specified manifest file from the Autodesk Forge API.
    /// The file is decompressed if it is in gzip format. The extracted content is then parsed into a JObject.
    /// </remarks>
    /// <exception cref="Exception">Thrown if there is an error during the download or extraction process.</exception>
    private static async Task<JObject> GetDerivativeAsync(string manifest, string accessToken)
    {
        // prepare to download the file
        var client = new RestClient(BaseUrl);
        RestRequest request = new RestRequest(DerivativePath + "{manifest}");
        request.Method = Method.Get;
        request.AddParameter("manifest", manifest, ParameterType.UrlSegment);
        request.AddHeader("Authorization", "Bearer " + accessToken);
        request.AddHeader("Accept-Encoding", "gzip, deflate");
        var response = await client.ExecuteGetAsync(request);

        JObject manifestJson = null;

        // unzip it
        if (manifest.IndexOf(".gz") > -1)
        {
            GZipStream gzip = new GZipStream(new MemoryStream(response.RawBytes), CompressionMode.Decompress);
            using (var fileStream = new StreamReader(gzip))
                manifestJson = JObject.Parse(fileStream.ReadToEnd());
        }
        else
        {
            ZipArchive zip = new ZipArchive(new MemoryStream(response.RawBytes));
            ZipArchiveEntry manifestData = zip.GetEntry("manifest.json");
            using (var stream = manifestData.Open())
            using (var reader = new StreamReader(stream))
                manifestJson = JObject.Parse(reader.ReadToEnd().ToString());
        }

        return manifestJson;
    }

    /// <summary>
    /// Downloads and parses the manifest for SVF (Simple Viewer Format) files.
    /// </summary>
    /// <param name="manifest">The manifest object containing information about SVF files.</param>
    /// <returns>A list of ManifestItem objects representing the parsed information from the manifest.</returns>
    /// <remarks>
    /// This method iterates through the manifest entries and extracts relevant information for SVF files.
    /// The information includes GUID, MIME type, and the decomposed URN (Uniform Resource Name) path.
    /// The method recursively processes child entries in the manifest.
    /// </remarks>
    /// <exception cref="Exception">Thrown if there is an issue during the parsing process.</exception>
    private static List<ManifestItem> ParseManifest(dynamic manifest)
    {
        List<ManifestItem> urns = new List<ManifestItem>();
        foreach (KeyValuePair<string, object> item in manifest.Dictionary)
        {
            DynamicDictionary itemKeys = (DynamicDictionary)item.Value;
            if (itemKeys.Dictionary.ContainsKey("role") && Roles.Contains(itemKeys.Dictionary["role"]))
            {
                urns.Add(new ManifestItem
                {
                    Guid = (string)itemKeys.Dictionary["guid"],
                    MIME = (string)itemKeys.Dictionary["mime"],
                    Path = DecomposeUrn((string)itemKeys.Dictionary["urn"])
                });
            }

            if (itemKeys.Dictionary.TryGetValue("children", out var value))
            {
                urns.AddRange(ParseManifest(value));
            }
        }

        return urns;
    }

    /// <summary>
    /// Decomposes a URN (Uniform Resource Name) into its components.
    /// </summary>
    /// <param name="encodedUrn">The URL-safe Base64 encoded URN of the source design.</param>
    /// <returns>A PathInfo object containing components of the decomposed URN, such as rootFileName, basePath, localPath, and URN.</returns>
    /// <remarks>
    /// This method takes a URL-safe Base64 encoded URN and extracts various components from it.
    /// - The rootFileName represents the last segment of the URN.
    /// - The basePath is the portion of the URN excluding the rootFileName.
    /// - The localPath is derived from the basePath with any unnecessary segments removed.
    /// - The URN is the original decoded URN.
    /// </remarks>
    private static PathInfo DecomposeUrn(string encodedUrn)
    {
        string urn = Uri.UnescapeDataString(encodedUrn);

        string rootFileName = urn.Substring(urn.LastIndexOf('/') + 1);
        string basePath = urn.Substring(0, urn.LastIndexOf('/') + 1);
        string localPath = basePath.Substring(basePath.IndexOf('/') + 1);
        localPath = Regex.Replace(localPath, "[/]?output/", string.Empty);

        return new PathInfo()
        {
            RootFileName = rootFileName,
            BasePath = basePath,
            LocalPath = localPath,
            Urn = urn
        };
    }
}