using Autodesk.Forge;

namespace APSToolkit.Auth;

public class Token
{
    public string access_token { get; set; }
    public string token_type { get; set; }
    public long expires_in { get; set; }
    public string refresh_token { get; set; }

    /// <summary>
    /// Retrieves a 2-legged access token from the Autodesk Forge API using client credentials.
    /// The retrieved token's access_token, token_type, and expires_in properties are then set to the corresponding properties of the Token instance.
    /// Returns the Token instance.
    /// </summary>
    public Token Refresh2LegToken()
    {
        Token token = Authentication.Get2LeggedToken().Result;
        this.access_token = token.access_token;
        this.token_type = token.token_type;
        this.expires_in = token.expires_in;
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
        Token token = Authentication.Refresh3LeggedToken(scopes).Result;
        this.access_token = token.access_token;
        this.token_type = token.token_type;
        this.expires_in = token.expires_in;
        this.refresh_token = token.refresh_token;
        return this;
    }
    public bool IsExpired()
    {
        bool b = this.expires_in <= 60;
        return b;
    }
}