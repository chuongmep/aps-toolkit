// Copyright (c) chuongmep.com. All rights reserved

using APSToolkit.Auth;
using Autodesk.Forge;

namespace APSToolkit.DesignAutomation;

public class DesignAutomateConfiguration
{
    /// <summary>
    /// Name of Application
    /// </summary>
    public string AppName { get; set; } = "AppNameDemo";

    // public string AppBundleName { get; set; } = "AppNameDemo.zip";
    public string PackageZipPath { get; set; }
    public string Script { get; set; }
    public Engine Engine { get; set; }
    public Version Version { get; set; } = Version.v2022;
    public string EngineName => GetEngineName();

    /// <summary>
    /// Default NickName is admin
    /// </summary>
    public string NickName { get; set; } = "admin";

    public string BundleDescription { get; set; } = "Description for Bundle Name";
    public string ActivityDescription { get; set; } = "Description for Activity Name";
    public string AppBundleFullName => $"{NickName}.{AppName}+{Alias.ToString().ToLower()}";

    public string ActivityName { get; set; } = "ActivityDemo";
    public string ActivityFullName => $"{NickName}.{ActivityName}+{Alias.ToString().ToLower()}";
    public Alias Alias { get; set; } = Alias.DEV;
    public string BucketOutputName => $"{AppName}_output";
    public string ResultFileName { get; set; } = "result";
    public string ResultFileExt { get; set; } = ".json";
    public string ClientId { get; }
    public string ClientSecret { get; }

    public DesignAutomateConfiguration()
    {
        // set client id and client secret from environment variables
        ClientId = Authentication.GetClientId();
        ClientSecret = Authentication.GetClientSecret();
    }

    public DesignAutomateConfiguration(string clientId, string clientSecret)
    {
        ClientId = clientId;
        ClientSecret = clientSecret;
    }

    private string GetEngineName()
    {
        string version = ((int) Version).ToString();
        switch (Engine)
        {
            case Engine.AutoCAD:
                return $"Autodesk.AutoCAD+{version}";
            case Engine.Inventor:
                return $"Autodesk.Inventor+{version}";
            case Engine.Revit:
                return $"Autodesk.Revit+{version}";
        }

        return string.Empty;
    }

    public string Get2LeggedToken()
    {
        return Authentication.Get2LeggedToken(ClientId, ClientSecret).Result;
    }
    public string Get3LeggedToken(string code,string callback)
    {
        return Authentication.Get3LeggedToken(ClientId, ClientSecret,code,callback).Result;
    }
    public string Refresh3LeggedToken(string refreshToken,Scope[] scope)
    {
        return Authentication.Refresh3LeggedToken(ClientId, ClientSecret, refreshToken,scope).Result;
    }
}