// Copyright (c) chuongmep.com. All rights reserved
using Autodesk.Forge;
namespace APSToolkit.Auth;

public static class Authentication
{
    /// <summary>
    /// Sets the Autodesk Forge Design Automation environment variables required for authentication.
    /// </summary>
    /// <param name="ClientID">The client ID for Autodesk Forge.</param>
    /// <param name="ClientSecret">The client secret for Autodesk Forge.</param>
    /// <param name="CallbackUrl">The callback URL for Autodesk Forge authentication.</param>
    public static void SetEnvironmentVariables(string ClientID, string ClientSecret, string CallbackUrl)
    {
        Environment.SetEnvironmentVariable("APS_CLIENT_ID", ClientID);
        Environment.SetEnvironmentVariable("APS_CLIENT_SECRET", ClientSecret);
        Environment.SetEnvironmentVariable("APS_CALLBACK_URL", CallbackUrl);
    }

    /// <summary>
    /// Retrieves the Autodesk Forge Design Automation client ID from the environment variables.
    /// Throws an exception if the environment variable is missing or empty.
    /// </summary>
    /// <returns>The Autodesk Forge Design Automation client ID.</returns>
    public static string GetClientId()
    {
        var ClientID = Environment.GetEnvironmentVariable("APS_CLIENT_ID");
        if (string.IsNullOrEmpty(ClientID))
        {
            throw new Exception("Missing APS_CLIENT_ID environment variable.");
        }

        return ClientID;
    }

    /// <summary>
    /// Retrieves the Autodesk Forge Design Automation client secret from the environment variables.
    /// Throws an exception if the environment variable is missing or empty.
    /// </summary>
    /// <returns>The Autodesk Forge Design Automation client secret.</returns>
    public static string GetClientSecret()
    {
        var ClientSecret = Environment.GetEnvironmentVariable("APS_CLIENT_SECRET");
        if (string.IsNullOrEmpty(ClientSecret))
        {
            throw new Exception("Missing APS_CLIENT_SECRET environment variable.");
        }

        return ClientSecret;
    }

    /// <summary>
    /// Retrieves the Autodesk Forge Design Automation callback URL from the environment variables.
    /// Throws an exception if the environment variable is missing or empty.
    /// </summary>
    /// <returns>The Autodesk Forge Design Automation callback URL.</returns>
    public static string GetCallbackUrl()
    {
        var CallbackUrl = Environment.GetEnvironmentVariable("APS_CALLBACK_URL");
        if (string.IsNullOrEmpty(CallbackUrl))
        {
            throw new Exception("Missing APS_CALLBACK_URL environment variable.");
        }

        return CallbackUrl;
    }

    /// <summary>
    /// Retrieves a 2-legged access token from the Autodesk Forge API using client credentials.
    /// </summary>
    /// <returns>The 2-legged access token.</returns>
    /// <exception cref="Exception">Thrown when APS_CLIENT_ID or APS_CLIENT_SECRET environment variables are missing.</exception>
    public static async Task<string> Get2LeggedToken()
    {
        Autodesk.Forge.TwoLeggedApi twoLeggedApi = new Autodesk.Forge.TwoLeggedApi();
        var ClientID = Environment.GetEnvironmentVariable("APS_CLIENT_ID");
        var ClientSecret = Environment.GetEnvironmentVariable("APS_CLIENT_SECRET");
        if (string.IsNullOrEmpty(ClientID) || string.IsNullOrEmpty(ClientSecret))
        {
            throw new Exception("Missing APS_CLIENT_ID or APS_CLIENT_SECRET environment variables.");
        }

        dynamic token = await twoLeggedApi.AuthenticateAsync(ClientID, ClientSecret, "client_credentials",
            new Scope[]
            {
                Scope.DataRead, Scope.DataWrite, Scope.DataCreate, Scope.DataSearch, Scope.BucketCreate,
                Scope.BucketRead, Scope.CodeAll,
                Scope.BucketUpdate, Scope.BucketDelete
            }).ConfigureAwait(false);
        var access_token = token.access_token;
        if (string.IsNullOrEmpty(access_token))
        {
            throw new Exception("can't get access_token, please check again value APS_CLIENT_ID and APS_CLIENT_SECRET");
        }

        return access_token;
    }

    /// <summary>
    /// Retrieves a 2-legged access token from the Autodesk Forge API using client credentials.
    /// </summary>
    /// <param name="clientId">The client ID for authentication.</param>
    /// <param name="clientSecret">The client secret for authentication.</param>
    /// <returns>The 2-legged access token.</returns>
    /// <exception cref="Exception">Thrown when clientId or clientSecret is null or empty.</exception>
    public static async Task<string> Get2LeggedToken(string clientId, string clientSecret)
    {
        Autodesk.Forge.TwoLeggedApi twoLeggedApi = new Autodesk.Forge.TwoLeggedApi();
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new Exception("Missing APS_CLIENT_ID or APS_CLIENT_SECRET environment variables.");
        }

        dynamic token = await twoLeggedApi.AuthenticateAsync(clientId, clientSecret, "client_credentials",
            new Scope[]
            {
                Scope.DataRead, Scope.DataWrite, Scope.DataCreate, Scope.DataSearch, Scope.BucketCreate,
                Scope.BucketRead,
                Scope.BucketUpdate, Scope.BucketDelete
            }).ConfigureAwait(false);
        var access_token = token.access_token;
        return access_token;
    }

    /// <summary>
    /// Retrieves a 3-legged access token from the Autodesk Forge API using the authorization code flow.
    /// </summary>
    /// <param name="code">The authorization code received from the Forge authentication callback.</param>
    /// <param name="callbackUrl">The callback URL used during the initial authentication request.</param>
    /// <returns>The 3-legged access token.</returns>
    /// <exception cref="Exception">Thrown when FORGE_CLIENT_ID or FORGE_CLIENT_SECRET environment variables are missing or empty.</exception>
    public static async Task<string> Get3LeggedToken(string code, string callbackUrl)
    {
        Autodesk.Forge.ThreeLeggedApi threeLeggedApi = new Autodesk.Forge.ThreeLeggedApi();
        var ClientID = Environment.GetEnvironmentVariable("APS_CLIENT_ID");
        var ClientSecret = Environment.GetEnvironmentVariable("APS_CLIENT_SECRET");
        if (string.IsNullOrEmpty(ClientID) || string.IsNullOrEmpty(ClientSecret))
        {
            throw new Exception("Missing FORGE_CLIENT_ID or FORGE_CLIENT_SECRET environment variables.");
        }

        dynamic token = await threeLeggedApi
            .GettokenAsync(ClientID, ClientSecret, "authorization_code", code, callbackUrl).ConfigureAwait(false);
        var access_token = token.access_token;
        return access_token;
    }

    /// <summary>
    /// Retrieves a 3-legged access token from the Autodesk Forge API using the authorization code flow.
    /// </summary>
    /// <param name="clientId">The client ID associated with the Forge application.</param>
    /// <param name="clientSecret">The client secret associated with the Forge application.</param>
    /// <param name="code">The authorization code received from the Forge authentication callback.</param>
    /// <param name="callbackUrl">The callback URL used during the initial authentication request.</param>
    /// <returns>The 3-legged access token.</returns>
    /// <exception cref="Exception">Thrown when clientId or clientSecret is null or empty.</exception>
    public static async Task<string> Get3LeggedToken(string clientId, string clientSecret, string code,
        string callbackUrl)
    {
        Autodesk.Forge.ThreeLeggedApi threeLeggedApi = new Autodesk.Forge.ThreeLeggedApi();
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new Exception("Missing FORGE_CLIENT_ID or FORGE_CLIENT_SECRET environment variables.");
        }

        dynamic token = await threeLeggedApi
            .GettokenAsync(clientId, clientSecret, "authorization_code", code, callbackUrl).ConfigureAwait(false);
        var access_token = token.access_token;
        return access_token;
    }

    /// <summary>
    /// Refreshes a 3-legged access token from the Autodesk Forge API using the refresh token grant type.
    /// </summary>
    /// <param name="clientId">The client ID associated with the Forge application.</param>
    /// <param name="clientSecret">The client secret associated with the Forge application.</param>
    /// <param name="refreshToken">The refresh token obtained during the initial authentication.</param>
    /// <param name="scope">The array of scopes specifying the access permissions for the refreshed token.</param>
    /// <returns>The refreshed 3-legged access token.</returns>
    /// <exception cref="Exception">Thrown when clientId, clientSecret, or refreshToken is null or empty.</exception>
    public static async Task<string> Refresh3LeggedToken(string clientId, string clientSecret, string refreshToken,
        Scope[] scope)
    {
        Autodesk.Forge.ThreeLeggedApi threeLeggedApi = new Autodesk.Forge.ThreeLeggedApi();
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(refreshToken))
        {
            throw new Exception("Missing required parameters: clientId, clientSecret, or refreshToken.");
        }

        threeLeggedApi.Configuration.AccessToken = await Get2LeggedToken(clientId, clientSecret).ConfigureAwait(false);
        dynamic token = await threeLeggedApi
            .RefreshtokenAsync(clientId, clientSecret, "refresh_token", refreshToken, scope).ConfigureAwait(false);
        var accessToken = token.access_token;
        // set refresh token
        var newRefreshToken = token.refresh_token;
        // set value to environment variable
        Environment.SetEnvironmentVariable("APS_REFRESH_TOKEN", newRefreshToken, EnvironmentVariableTarget.User);
        return accessToken;
    }

    /// <summary>
    /// Refreshes a 3-legged access token from the Autodesk Forge API using the refresh token grant type.
    /// </summary>
    /// <param name="clientId">The client ID associated with the Forge application.</param>
    /// <param name="clientSecret">The client secret associated with the Forge application.</param>
    /// <param name="scope">The array of scopes specifying the access permissions for the refreshed token.</param>
    /// <returns>The refreshed 3-legged access token.</returns>
    /// <exception cref="Exception">Thrown when clientId, clientSecret, or APS_REFRESH_TOKEN is null or empty.</exception>
    public static async Task<string> Refresh3LeggedToken(string clientId, string clientSecret, Scope[] scope)
    {
        var refreshToken = Environment.GetEnvironmentVariable("APS_REFRESH_TOKEN", EnvironmentVariableTarget.User);
        if (string.IsNullOrEmpty(refreshToken)) throw new Exception("Missing APS_REFRESH_TOKEN environment variable.");
        Autodesk.Forge.ThreeLeggedApi threeLeggedApi = new Autodesk.Forge.ThreeLeggedApi();
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            throw new Exception("Missing required parameters: clientId, clientSecret.");
        threeLeggedApi.Configuration.AccessToken = await Get2LeggedToken(clientId, clientSecret).ConfigureAwait(false);
        dynamic token = await threeLeggedApi
            .RefreshtokenAsync(clientId, clientSecret, "refresh_token", refreshToken, scope).ConfigureAwait(false);
        var accessToken = token.access_token;
        // set refresh token
        var newRefreshToken = token.refresh_token;
        // set value to environment variable
        Environment.SetEnvironmentVariable("APS_REFRESH_TOKEN", newRefreshToken, EnvironmentVariableTarget.User);
        return accessToken;
    }

    public static Task<string> Refresh3LeggedToken(Scope[] scope)
    {
        var clientID = Environment.GetEnvironmentVariable("APS_CLIENT_ID", EnvironmentVariableTarget.User);
        var clientSecret = Environment.GetEnvironmentVariable("APS_CLIENT_SECRET", EnvironmentVariableTarget.User);
        var refreshToken = Environment.GetEnvironmentVariable("APS_REFRESH_TOKEN", EnvironmentVariableTarget.User);
        var Leg3Token = Authentication.Refresh3LeggedToken(clientID, clientSecret, scope).Result;
        return Task.FromResult(Leg3Token);
    }
}