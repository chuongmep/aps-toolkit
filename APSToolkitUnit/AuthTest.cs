using System;
using System.Threading.Tasks;
using APSToolkit;
using Autodesk.Forge;
using NUnit.Framework;

namespace ForgeToolkitUnit;

public class AuthTest
{
    private static Token Token { get; set; }
    private Auth Auth { get; set; }
    [SetUp]
    public void Setup()
    {
        Auth = new Auth();
    }

    [Test]
    public void TestAuthentication2Leg()
    {
        Token = Auth.Get2LeggedToken().Result;
        Assert.IsNotEmpty(Token.AccessToken);
    }
    [Test]
    public void TestAuthentication3Leg()
    {
        Token = Auth.Get3LeggedToken().Result;
        Assert.IsNotNull(Token.AccessToken);
        Assert.IsNotEmpty(Token.AccessToken);
    }
    [Test]
    public Task TestAuthentication3LegPkce()
    {
        string clientId = Environment.GetEnvironmentVariable("APS_CLIENT_ID_PKCE", EnvironmentVariableTarget.User);
        Auth = new Auth(clientId);
        Token = Auth.Get3LeggedTokenPkce().Result;
        Assert.IsNotNull(Token.AccessToken);
        Assert.IsNotEmpty(Token.RefreshToken);
        return Task.CompletedTask;
    }
    [Test]
    public Task TestRefresh3LegToken()
    {
        var clientID = Environment.GetEnvironmentVariable("APS_CLIENT_ID", EnvironmentVariableTarget.User);
        var clientSecret = Environment.GetEnvironmentVariable("APS_CLIENT_SECRET", EnvironmentVariableTarget.User);
        var refreshToken = Environment.GetEnvironmentVariable("APS_REFRESH_TOKEN", EnvironmentVariableTarget.User);
        if (string.IsNullOrEmpty(clientID)) Assert.Fail("Missing APS_CLIENT_ID environment variable.");
        if (string.IsNullOrEmpty(clientSecret)) Assert.Fail("Missing APS_CLIENT_SECRET environment variable.");
        if (string.IsNullOrEmpty(refreshToken)) Assert.Fail("Missing APS_REFRESH_TOKEN environment variable.");
        Scope[] scope = new Scope[]
            {Scope.DataRead, Scope.DataWrite, Scope.DataCreate, Scope.BucketRead, Scope.BucketCreate};
        var Leg3Token = Auth.Refresh3LeggedToken(clientID, clientSecret, scope).Result;
        Assert.IsNotNull(Leg3Token);
        return Task.CompletedTask;
    }

    [Test]
    public void TestTokenExpired()
    {
        Token = Auth.Get2LeggedToken().Result;
        Assert.IsFalse(Token.IsExpired());
    }
}