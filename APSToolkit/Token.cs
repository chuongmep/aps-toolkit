using Autodesk.Forge;

namespace APSToolkit;

public class Token
{
    public Token(string? accessToken, string? tokenType, long expiresIn, string refreshToken) :this()
    {
        this.AccessToken = accessToken;
        this.TokenType = tokenType;
        this.ExpiresIn = expiresIn;
        this.RefreshToken = refreshToken;
    }

    public Token()
    {

    }
    // map with access_token
    [System.Text.Json.Serialization.JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
    public long? ExpiresIn { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Retrieves a 2-legged access token from the Autodesk Forge API using client credentials.
    /// The retrieved token's access_token, token_type, and expires_in properties are then set to the corresponding properties of the Token instance.
    /// Returns the Token instance.
    /// </summary>
    public Token Refresh2LegToken()
    {
        var auth = new Auth();
        Token? token = auth.Get2LeggedToken().Result;
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
        Scope[] scopes = new Scope[]
        {
            Scope.DataRead, Scope.DataWrite, Scope.DataCreate, Scope.DataSearch, Scope.BucketCreate,
            Scope.BucketRead, Scope.CodeAll,
            Scope.BucketUpdate, Scope.BucketDelete
        };
        Token token = Auth.Refresh3LeggedToken(scopes).Result;
        this.AccessToken = token.AccessToken;
        this.TokenType = token.TokenType;
        this.ExpiresIn = token.ExpiresIn;
        this.RefreshToken = token.RefreshToken;
        return this;
    }
    /// <summary>
    /// Checks if the token has expired.
    /// </summary>
    /// <param name="bufferMinutes"></param>
    /// <returns></returns>
    public bool IsExpired(double bufferMinutes=0)
    {
        var currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (currentUnixTime + bufferMinutes*60 >= this.ExpiresIn)
        {
            return true;
        }
        return false;
    }
}