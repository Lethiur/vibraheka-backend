using System.Net;
using System.Text;
using System.Text.Json;
using Infrastructure.Rest.Client.UnitTests.Helpers;
using Infrastructure.Rest.Client.Zoom;
using Infrastructure.Rest.Client.Zoom.Config;
using Infrastructure.Rest.Client.Zoom.Models;
using Infrastructure.Rest.Client.Zoom.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Infrastructure.Rest.Client.UnitTests.Zoom.ZoomAuthServiceTest;

public abstract class GenericZoomAuthServiceTest
{
    protected FakeHttpMessageHandler FakeHandler = default!;
    protected ZoomApiClient ApiClient = default!;
    protected ZoomAuthService AuthService = default!;

    protected static ZoomConfig ValidConfig => new()
    {
        AccountID = "test-account-id",
        ClientID = "test-client-id",
        ClientSecret = "test-client-secret",
        HostEmail = "host@example.com",
    };

    [SetUp]
    public virtual void SetUp()
    {
        FakeHandler = new FakeHttpMessageHandler();
        HttpClient httpClient = new(FakeHandler);
        ApiClient = new ZoomApiClient(NullLogger<ZoomApiClient>.Instance, httpClient);
        AuthService = new ZoomAuthService(ApiClient, Options.Create(ValidConfig));
    }

    [TearDown]
    public virtual void TearDown()
    {
        FakeHandler.Dispose();
    }

    /// <summary>
    /// Builds an auth token HTTP response. ExpiresIn=3600 means token is cached for ~59 minutes.
    /// </summary>
    protected static HttpResponseMessage BuildTokenResponseWithExpiry(
        string accessToken = "cached-token",
        int expiresIn = 3600)
    {
        ZoomAuthTokenResponse tokenResponse = new()
        {
            AccessToken = accessToken,
            ExpiresIn = expiresIn,
        };
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(tokenResponse),
                Encoding.UTF8,
                "application/json"),
        };
    }

    /// <summary>
    /// Builds an auth token response with ExpiresIn=60. This causes TokenExpiration = UtcNow + 0s,
    /// meaning the token is immediately considered expired on any subsequent check.
    /// </summary>
    protected static HttpResponseMessage BuildImmediatelyExpiredTokenResponse(string accessToken = "expired-token")
    {
        return BuildTokenResponseWithExpiry(accessToken, expiresIn: 60);
    }

    protected static HttpResponseMessage BuildAuthFailureResponse()
    {
        return new HttpResponseMessage(HttpStatusCode.Unauthorized);
    }
}


