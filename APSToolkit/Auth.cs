// Copyright (c) chuongmep.com. All rights reserved

using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Autodesk.Authentication;
using Autodesk.Authentication.Model;
using Autodesk.SDKManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace APSToolkit;

public class Auth
{
    private string? ClientId { get; set; }
    private string? ClientSecret { get; set; }

    private Token Token { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Auth"/> class.
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="clientSecret"></param>
    public Auth(string? clientId = null, string? clientSecret = null)
    {
        this.ClientId = clientId;
        this.ClientSecret = clientSecret;
        this.Token = new Token();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Auth"/> class.
    ///  If the client ID and client secret are not provided, the constructor retrieves them from the environment variables.
    /// </summary>
    public Auth()
    {
        this.ClientId = Environment.GetEnvironmentVariable("APS_CLIENT_ID", EnvironmentVariableTarget.User);
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
        var sdkManager = SdkManagerBuilder.Create().Build();
        var authenticationClient = new AuthenticationClient(sdkManager);
        List<Scopes> scopes = new List<Scopes>
        {
            Scopes.DataRead, Scopes.DataWrite, Scopes.DataCreate, Scopes.DataSearch, Scopes.BucketCreate,
            Scopes.BucketRead, Scopes.CodeAll,
            Scopes.BucketUpdate, Scopes.BucketDelete
        };
        TwoLeggedToken? auth = await authenticationClient.GetTwoLeggedTokenAsync(ClientId, ClientSecret, scopes);
        return new Token(auth.AccessToken, auth.TokenType, auth.ExpiresIn!, string.Empty);
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

        var authUrl =
            $"https://developer.api.autodesk.com/authentication/v2/authorize?response_type=code&client_id={ClientId}&redirect_uri={callbackUrl}&scope={scopes}";

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
        var payload =
            $"grant_type=authorization_code&code={code}&client_id={ClientId}&client_secret={ClientSecret}&redirect_uri={callbackUrl}";

        using var client = new HttpClient();
        var content = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded");
        var response = await client.PostAsync(tokenUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to retrieve token: {errorMessage}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var token = new Token();
        token.RefreshToken = JObject.Parse(jsonResponse)["refresh_token"]!.Value<string>();
        token.AccessToken = JObject.Parse(jsonResponse)["access_token"]!.Value<string>();
        token.TokenType = JObject.Parse(jsonResponse)["token_type"]!.Value<string>();
        token.ExpiresIn = JObject.Parse(jsonResponse)["expires_in"]!.Value<int>();
        return token;
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

    public async Task<Token> Get3LeggedTokenPkce(string? callbackUrl = null, string? scopes = null)
    {
        if (string.IsNullOrEmpty(scopes))
        {
            scopes = "data:read";
        }

        if (string.IsNullOrEmpty(callbackUrl))
        {
            callbackUrl = "http://localhost:8080/api/auth/callback";
        }

        string codeVerifier = RandomString(64);
        string codeChallenge = GenerateCodeChallenge(codeVerifier);
        string authUrl =
            $"https://developer.api.autodesk.com/authentication/v2/authorize?response_type=code&client_id={ClientId}&redirect_uri={callbackUrl}&scope={scopes}&prompt=login&code_challenge={codeChallenge}&code_challenge_method=S256";
        OpenDefaultBrowser(authUrl);
        // get prefix from callbackUrl just get http://localhost:8080/api/auth/ from http://localhost:8080/api/auth/callback
        string prefix = callbackUrl.Substring(0, callbackUrl.LastIndexOf('/')) + "/";
        var listenerTask = CallListener(prefix, codeVerifier, callbackUrl, scopes);
        await listenerTask;
        Environment.SetEnvironmentVariable("APS_REFRESH_TOKEN", Token.RefreshToken, EnvironmentVariableTarget.User);
        return Token;
    }

    private async Task CallListener(string prefix, string codeVerifier, string callbackUrl, string scopes)
    {
        if (!HttpListener.IsSupported)
        {
            throw new NotSupportedException("HttpListener is not supported in this context!");
        }

        if (prefix == null || prefix.Length == 0)
            throw new ArgumentException("prefixes");
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(prefix);
        listener.Start();
        HttpListenerContext context = await listener.GetContextAsync();
        HttpListenerRequest request = context.Request;
        // Obtain a response object.
        HttpListenerResponse response = context.Response;

        try
        {
            string? authCode = request.Url?.Query.ToString().Split('=')[1];
            await GetPkceToken(authCode, codeVerifier, callbackUrl, scopes);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        // Construct a response.
        string responseString = "<HTML><BODY> Authentication successful. You can close this window now.</BODY></HTML>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        // Get a response stream and write the response to it.
        response.ContentLength64 = buffer.Length;
        System.IO.Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        // You must close the output stream.
        output.Close();
        listener.Stop();
    }

    private async Task GetPkceToken(string? authCode, string codeVerifier, string callbackUrl, string scopes)
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://developer.api.autodesk.com/authentication/v2/token"),
                Content = new FormUrlEncodedContent(new Dictionary<string, string?>
                {
                    { "client_id", ClientId },
                    { "code_verifier", codeVerifier },
                    { "code", authCode },
                    { "scope", scopes },
                    { "grant_type", "authorization_code" },
                    { "redirect_uri", callbackUrl }
                }),
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                string bodystring = await response.Content.ReadAsStringAsync();
                JObject bodyjson = JObject.Parse(bodystring);
                this.Token.AccessToken = bodyjson["access_token"]!.Value<string>();
                this.Token.TokenType = bodyjson["token_type"]!.Value<string>();
                this.Token.ExpiresIn = bodyjson["expires_in"]!.Value<int>();
                this.Token.RefreshToken = bodyjson["refresh_token"]!.Value<string>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private string RandomString(int length)
    {
        Random random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private string GenerateCodeChallenge(string codeVerifier)
    {
        var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        var b64Hash = Convert.ToBase64String(hash);
        var code = Regex.Replace(b64Hash, "\\+", "-");
        code = Regex.Replace(code, "\\/", "_");
        code = Regex.Replace(code, "=+$", "");
        return code;
    }

    /// <summary>
    /// Refreshes a 3-legged access token from the Autodesk Forge API using the refresh token grant type.
    /// </summary>
    /// <param name="refreshToken">The refresh token obtained during the initial authentication.</param>
    /// <param name="scopes">The array of scopes specifying the access permissions for the refreshed token.
    /// </param>
    /// <returns>The refreshed 3-legged access token.</returns>
    /// <exception cref="Exception">Thrown when clientId, clientSecret, or refreshToken is null or empty.</exception>
    public async Task<Token> Refresh3LeggedToken(string refreshToken,
        List<Scopes>? scopes = null)
    {
        var sdkManager = SdkManagerBuilder.Create().Build();
        var authenticationClient = new AuthenticationClient(sdkManager);
        if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(ClientSecret) || string.IsNullOrEmpty(refreshToken))
        {
            throw new Exception("ClientId, ClientSecret, or RefreshToken is null or empty.");
        }

        if (scopes == null)
        {
            scopes = new List<Scopes>
            {
                Scopes.DataRead, Scopes.DataWrite, Scopes.DataCreate, Scopes.DataSearch, Scopes.BucketCreate,
                Scopes.BucketRead, Scopes.CodeAll,
                Scopes.BucketUpdate, Scopes.BucketDelete
            };
        }

        RefreshToken tokenAsync =
            await authenticationClient.GetRefreshTokenAsync(ClientId, ClientSecret, refreshToken, scopes);
        Environment.SetEnvironmentVariable("APS_REFRESH_TOKEN", tokenAsync._RefreshToken,
            EnvironmentVariableTarget.User);
        this.Token.AccessToken = tokenAsync.AccessToken;
        this.Token.TokenType = tokenAsync.TokenType;
        this.Token.ExpiresIn = tokenAsync.ExpiresIn;
        this.Token.RefreshToken = tokenAsync._RefreshToken;
        return Token;
    }
}