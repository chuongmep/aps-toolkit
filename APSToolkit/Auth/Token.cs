namespace APSToolkit.Auth;

public class Token
{
    public string access_token { get; set; }
    public string token_type { get; set; }
    public long expires_in { get; set; }
    public string refresh_token { get; set; }
    public void Refresh2LegToken()
    {
        Token token = Authentication.Get2LeggedToken().Result;
        this.access_token = token.access_token;
        this.token_type = token.token_type;
        this.expires_in = token.expires_in;
    }
}