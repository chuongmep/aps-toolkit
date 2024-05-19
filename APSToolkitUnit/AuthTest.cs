using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using APSToolkit;
using Autodesk.Authentication.Model;
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
    // [Test]
    // public Task TestAuthentication3LegPkce()
    // {
    //     string clientId = Environment.GetEnvironmentVariable("APS_CLIENT_ID_PKCE", EnvironmentVariableTarget.User);
    //     Auth = new Auth(clientId);
    //     Token = Auth.Get3LeggedTokenPkce().Result;
    //     Assert.IsNotNull(Token.AccessToken);
    //     Assert.IsNotEmpty(Token.RefreshToken);
    //     return Task.CompletedTask;
    // }
    [Test]
    public Task TestRefresh3LegToken()
    {
        var refreshToken = Environment.GetEnvironmentVariable("APS_REFRESH_TOKEN", EnvironmentVariableTarget.User);
        if (string.IsNullOrEmpty(refreshToken)) Assert.Fail("Missing APS_REFRESH_TOKEN environment variable.");
        var scopes = new List<Scopes>
        {
            Scopes.DataRead, Scopes.DataWrite, Scopes.DataCreate, Scopes.DataSearch, Scopes.BucketCreate,
            Scopes.BucketRead, Scopes.CodeAll,
            Scopes.BucketUpdate, Scopes.BucketDelete
        };
        var Leg3Token = Auth.Refresh3LeggedToken(refreshToken, scopes).Result;
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