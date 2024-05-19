using System.Runtime.Serialization;
using Autodesk.Authentication.Model;
using Autodesk.Forge;

namespace APSToolkit;

public class Token
{
    public Token(string? accessToken, string? tokenType, int? expiresIn, string refreshToken) :this()
    {
        this.AccessToken = accessToken;
        this.TokenType = tokenType;
        this.ExpiresIn = expiresIn;
        this.RefreshToken = refreshToken;
    }

    public Token()
    {

    }

    [DataMember(Name = "access_token", EmitDefaultValue = false)]
    public string? AccessToken { get; set; }

    [DataMember(Name = "token_type", EmitDefaultValue = false)]
    public string? TokenType { get; set; }
    [DataMember(Name = "expires_in", EmitDefaultValue = false)]
    public long? ExpiresIn { get; set; }
    [DataMember(Name = "refresh_token", EmitDefaultValue = false)]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Retrieves a 2-legged access token from the Autodesk Forge API using client credentials.
    /// The retrieved token's access_token, token_type, and expires_in properties are then set to the corresponding properties of the Token instance.
    /// Returns the Token instance.
    /// </summary>
    public Token Refresh2LegToken()
    {
        var auth = new Auth();
        Token token = auth.Get2LeggedToken().Result;
        this.AccessToken = token.AccessToken;
        this.TokenType = token.TokenType;
        this.ExpiresIn = token.ExpiresIn;
        return this;
    }

    /// <summary>
    /// Refreshes a 3-legged access token from the Autodesk Forge API using the refresh token grant type.
    /// The method specifies an array of scopes for the refreshed token.
    /// The retrieved token's access_token, token_type, expires_in, and refresh_token properties are then set to the corresponding properties of the Token instance.
    /// Returns the Token instance.
    /// </summary>
    public Token Refresh3LegToken()
    {
        var scopes = new List<Scopes>
        {
            Scopes.DataRead, Scopes.DataWrite, Scopes.DataCreate, Scopes.DataSearch, Scopes.BucketCreate,
            Scopes.BucketRead, Scopes.CodeAll,
            Scopes.BucketUpdate, Scopes.BucketDelete
        };
        var refreshToken = Environment.GetEnvironmentVariable("APS_REFRESH_TOKEN", EnvironmentVariableTarget.User);
        var auth = new Auth();
        Token token = auth.Refresh3LeggedToken(refreshToken,scopes).Result;
        this.AccessToken = token.AccessToken;
        this.TokenType = token.TokenType;
        this.ExpiresIn = token.ExpiresIn;
        this.RefreshToken = token.RefreshToken;
        return this;
    }
    public bool IsExpired()
    {
        bool b = this.ExpiresIn <= 60;
        return b;
    }
}