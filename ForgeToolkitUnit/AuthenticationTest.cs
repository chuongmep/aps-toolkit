using System;
using System.Threading.Tasks;
using APSToolkit.Auth;
using Autodesk.Forge;
using NUnit.Framework;

namespace ForgeToolkitUnit;

public class AuthenticationTest
{
    private static string Token { get; set; }

    [SetUp]
    public void Setup()
    {
        Token = Authentication.Get2LeggedToken().Result;
    }

    [Test]
    public void TestAuthentication2Leg()
    {
        Assert.IsNotNull(Token);
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
        var Leg3Token = Authentication.Refresh3LeggedToken(clientID, clientSecret, scope).Result;
        Assert.IsNotNull(Leg3Token);
        return Task.CompletedTask;
    }
}