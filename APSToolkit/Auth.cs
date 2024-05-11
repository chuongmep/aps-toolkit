// Copyright (c) chuongmep.com. All rights reserved

using System.Diagnostics;
using System.Net;
using System.Text;
using Autodesk.Forge;
using Newtonsoft.Json;

namespace APSToolkit;

public class Auth
{
    private string? ClientId { get; set; }
    private string? ClientSecret { get; set; }

    private Token Token { get; set; }
    public Auth(string? clientId, string? clientSecret)
    {
        this.ClientId = clientId;
        this.ClientSecret = clientSecret;
        this.Token = new Token();
    }

    public Auth()
    {
        this.ClientId = Environment.GetEnvironmentVariable("APS_CLIENT_ID",EnvironmentVariableTarget.User);
        this.ClientSecret = Environment.GetEnvironmentVariable("APS_CLIENT_SECRET", EnvironmentVariableTarget.User);
        this.Token = new Token();
    }

    /// <summary>
    /// Sets the Autodesk Forge Design Automation environment variables required for authentication.
    /// </summary>
    /// <param name="clientId">The client ID for Autodesk Forge.</param>
    /// <param name="clientSecret">The client secret for Autodesk Forge.</param>
    /// <param name="callbackUrl">The callback URL for Autodesk Forge authentication.</param>
    public static void SetEnvironmentVariables(string clientId, string clientSecret, string callbackUrl)
    {
        Environment.SetEnvironmentVariable("APS_CLIENT_ID", clientId);
        Environment.SetEnvironmentVariable("APS_CLIENT_SECRET", clientSecret);
        Environment.SetEnvironmentVariable("APS_CALLBACK_URL", callbackUrl);
    }

    /// <summary>
    /// Retrieves the Autodesk Forge Design Automation client ID from the environment variables.
    /// Throws an exception if the environment variable is missing or empty.
    /// </summary>
    /// <returns>The Autodesk Forge Design Automation client ID.</returns>
    public string GetClientId()
    {
        var clientId = Environment.GetEnvironmentVariable("APS_CLIENT_ID");
        if (string.IsNullOrEmpty(clientId))
        {
            throw new Exception("Missing APS_CLIENT_ID environment variable.");
        }

        return clientId;
    }

    /// <summary>
    /// Retrieves the Autodesk Forge Design Automation client secret from the environment variables.
    /// Throws an exception if the environment variable is missing or empty.
    /// </summary>
    /// <returns>The Autodesk Forge Design Automation client secret.</returns>
    public string GetClientSecret()
    {
        var clientSecret = Environment.GetEnvironmentVariable("APS_CLIENT_SECRET");
        if (string.IsNullOrEmpty(clientSecret))
        {
            throw new Exception("Missing APS_CLIENT_SECRET environment variable.");
        }

        return clientSecret;
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
    public async Task<Token> Get2LeggedToken()
    {
        Autodesk.Forge.TwoLeggedApi twoLeggedApi = new Autodesk.Forge.TwoLeggedApi();
        if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(ClientSecret))
        {
            throw new Exception("Missing APS_CLIENT_ID or APS_CLIENT_SECRET environment variables.");
        }
        dynamic token = await twoLeggedApi.AuthenticateAsync(ClientId, ClientSecret, "client_credentials",
            new Scope[]
            {
                Scope.DataRead, Scope.DataWrite, Scope.DataCreate, Scope.DataSearch, Scope.BucketCreate,
                Scope.BucketRead, Scope.CodeAll,
                Scope.BucketUpdate, Scope.BucketDelete
            }).ConfigureAwait(false);
        this.Token.AccessToken = token.access_token;
        this.Token.TokenType = token.token_type;
        this.Token.ExpiresIn = token.expires_in;
        if (string.IsNullOrEmpty(this.Token.AccessToken))
        {
            throw new Exception("can't get access_token, please check again value APS_CLIENT_ID and APS_CLIENT_SECRET");
        }
        return Token;
    }
    public async Task<Token> Get3LeggedToken(string? callbackUrl = null, string? scopes = null)
    {
        if (string.IsNullOrEmpty(scopes))
        {
            scopes =
                "data:read data:write data:create data:search bucket:create bucket:read bucket:update bucket:delete code:all";
        }

        if (string.IsNullOrEmpty(callbackUrl))
        {
            callbackUrl = "http://localhost:8080/api/auth/callback";
        }

        var authUrl = $"https://developer.api.autodesk.com/authentication/v2/authorize?response_type=code&client_id={ClientId}&redirect_uri={callbackUrl}&scope={scopes}";

        OpenDefaultBrowser(authUrl);

        // Start listening for the callback URL
        using var listener = new HttpListener();
        listener.Prefixes.Add(callbackUrl + "/");
        listener.Start();

        Console.WriteLine($"Listening for callback at: {callbackUrl}");

        while (true)
        {
            var context = await listener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            // Extract code from callback URL
            var query = request.Url?.Query;
            var queryParams = System.Web.HttpUtility.ParseQueryString(query!);
            var code = queryParams["code"];

            var token = await HandleCallback(callbackUrl, code);

            this.Token.AccessToken = token!.AccessToken;
            this.Token.TokenType = token.TokenType;
            this.Token.ExpiresIn = token.ExpiresIn;
            this.Token.RefreshToken = token.RefreshToken;

            var responseString = "Authentication successful. You can close this window now.";
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();

            break;
        }
        listener.Stop();
        Environment.SetEnvironmentVariable("APS_REFRESH_TOKEN", Token.RefreshToken, EnvironmentVariableTarget.User);
        return Token;
    }

    private async Task<Token?> HandleCallback(string callbackUrl, string? code)
    {
        var tokenUrl = "https://developer.api.autodesk.com/authentication/v2/token";
        var payload = $"grant_type=authorization_code&code={code}&client_id={ClientId}&client_secret={ClientSecret}&redirect_uri={callbackUrl}";

        using var client = new HttpClient();
        var content = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded");
        var response = await client.PostAsync(tokenUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to retrieve token: {errorMessage}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<Token>(jsonResponse);
    }

    private void OpenDefaultBrowser(string url)
    {
        try
        {
            // Use the default browser on the system to open the URL
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            // Handle any exceptions, such as if there's no default browser set
            Console.WriteLine($"Error opening default browser: {ex.Message}");
        }
    }

    /// <summary>
    /// Refreshes a 3-legged access token from the Autodesk Forge API using the refresh token grant type.
    /// </summary>
    /// <param name="refreshToken">The refresh token obtained during the initial authentication.</param>
    /// <param name="scope">The array of scopes specifying the access permissions for the refreshed token.</param>
    /// <returns>The refreshed 3-legged access token.</returns>
    /// <exception cref="Exception">Thrown when clientId, clientSecret, or refreshToken is null or empty.</exception>
    public async Task<Token> Refresh3LeggedToken(string refreshToken,
        Scope[] scope)
    {
        Autodesk.Forge.ThreeLeggedApi threeLeggedApi = new Autodesk.Forge.ThreeLeggedApi();
        if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(ClientSecret) || string.IsNullOrEmpty(refreshToken))
        {
            throw new Exception("Missing required parameters: clientId, clientSecret, or refreshToken.");
        }

        threeLeggedApi.Configuration.AccessToken = Get2LeggedToken().Result.AccessToken;
        dynamic token = await threeLeggedApi
            .RefreshtokenAsync(ClientId, ClientSecret, "refresh_token", refreshToken, scope).ConfigureAwait(false);
        var accessToken = token.access_token;
        // set refresh token
        var newRefreshToken = token.refresh_token;
        // set value to environment variable
        Environment.SetEnvironmentVariable("APS_REFRESH_TOKEN", newRefreshToken, EnvironmentVariableTarget.User);
        this.Token.AccessToken = accessToken;
        this.Token.TokenType = token.token_type;
        this.Token.ExpiresIn = token.expires_in;
        this.Token.RefreshToken = newRefreshToken;
        return Token;
    }

    /// <summary>
    /// Refreshes a 3-legged access token from the Autodesk Forge API using the refresh token grant type.
    /// </summary>
    /// <param name="clientId">The client ID associated with the Forge application.</param>
    /// <param name="clientSecret">The client secret associated with the Forge application.</param>
    /// <param name="scope">The array of scopes specifying the access permissions for the refreshed token.</param>
    /// <returns>The refreshed 3-legged access token.</returns>
    /// <exception cref="Exception">Thrown when clientId, clientSecret, or APS_REFRESH_TOKEN is null or empty.</exception>
    public async Task<Token> Refresh3LeggedToken(string clientId, string clientSecret, Scope[] scope)
    {
        var refreshToken = Environment.GetEnvironmentVariable("APS_REFRESH_TOKEN", EnvironmentVariableTarget.User);
        if (string.IsNullOrEmpty(refreshToken)) throw new Exception("Missing APS_REFRESH_TOKEN environment variable.");
        Autodesk.Forge.ThreeLeggedApi threeLeggedApi = new Autodesk.Forge.ThreeLeggedApi();
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            throw new Exception("Missing required parameters: clientId, clientSecret.");
        threeLeggedApi.Configuration.AccessToken = Get2LeggedToken().Result.AccessToken;
        dynamic token = await threeLeggedApi
            .RefreshtokenAsync(clientId, clientSecret, "refresh_token", refreshToken, scope).ConfigureAwait(false);
        var accessToken = token.access_token;
        // set refresh token
        var newRefreshToken = token.refresh_token;
        // set value to environment variable
        Environment.SetEnvironmentVariable("APS_REFRESH_TOKEN", newRefreshToken, EnvironmentVariableTarget.User);
        Token newToken = new Token()
        {
            AccessToken = accessToken,
            TokenType = token.token_type,
            ExpiresIn = token.expires_in,
            RefreshToken = newRefreshToken
        };
        return newToken;
    }

    public static Task<Token> Refresh3LeggedToken(Scope[] scope)
    {
        var clientID = Environment.GetEnvironmentVariable("APS_CLIENT_ID", EnvironmentVariableTarget.User);
        var clientSecret = Environment.GetEnvironmentVariable("APS_CLIENT_SECRET", EnvironmentVariableTarget.User);
        var Leg3Token = new Auth().Refresh3LeggedToken(clientID, clientSecret, scope).Result;
        return Task.FromResult(Leg3Token);
    }
}