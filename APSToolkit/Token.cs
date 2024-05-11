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

    public string? AccessToken { get; set; }
    public string? TokenType { get; set; }
    public long? ExpiresIn { get; set; }
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
    public bool IsExpired()
    {
        bool b = this.ExpiresIn <= 60;
        return b;
    }
}