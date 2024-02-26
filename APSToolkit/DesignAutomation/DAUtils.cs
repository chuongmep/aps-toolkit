// Copyright (c) chuongmep.com. All rights reserved

using Autodesk.Forge;

namespace APSToolkit.DesignAutomation;

/// <summary>
/// Design Automation Utils
/// </summary>
public static class DAUtils
{
    /// <summary>
    /// Check if the version is a composite design
    /// </summary>
    /// <param name="token"></param>
    /// <param name="projectId"></param>
    /// <param name="versionId"></param>
    /// <returns></returns>
    public static async Task<bool> IsCompositeDesign(string token, string projectId, string versionId)
    {
        VersionsApi versionApi = new VersionsApi();
        versionApi.Configuration.AccessToken = token;
        dynamic version = await versionApi.GetVersionAsync(projectId, versionId).ConfigureAwait(false);
        bool isDesign = version.data.attributes.extension.data.isCompositeDesign;
        return isDesign;
    }

    /// <summary>
    /// Return the Revit version of the model
    /// </summary>
    /// <param name="token"></param>
    /// <param name="projectId">project id
    /// e.g: b.ca790fb5-141d-4ad5-b411-0461af2e9748</param>
    /// <param name="versionId">e.g: urn:adsk.wipprod:fs.file:vf.N4upIOvMRByQDt1u8va4Bw?version=2</param>
    /// <returns></returns>
    public static async Task<Version> GetRevitVersionByVersionId(string token, string projectId, string versionId)
    {
        VersionsApi versionApi = new VersionsApi();
        versionApi.Configuration.AccessToken = token;
        dynamic version = await versionApi.GetVersionAsync(projectId, versionId).ConfigureAwait(false);
        long revitVersion = version.data.attributes.extension.data.revitProjectVersion;
        // return enum match with revitversion like v2020, v2021, v2022,... . Revitversion is 2020, 2021, 2022,...
        return (Version) Enum.Parse(typeof(Version), $"v{revitVersion}");
    }

    /// <summary>
    /// Get Revit version by itemid
    /// </summary>
    /// <param name="token"></param>
    /// <param name="projectId"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static async Task<Version> GetRevitVersionByItem(string token, string projectId, dynamic item)
    {
        VersionsApi versionApi = new VersionsApi();
        versionApi.Configuration.AccessToken = token;
        string versionId = item.relationships.tip.data.id;
        dynamic version = await versionApi.GetVersionAsync(projectId, versionId).ConfigureAwait(false);
        long revitVersion = version.data.attributes.extension.data.revitProjectVersion;
        // return enum match with revitversion like v2020, v2021, v2022,... . Revitversion is 2020, 2021, 2022,...
        return (Version) Enum.Parse(typeof(Version), $"v{revitVersion}");
    }
}