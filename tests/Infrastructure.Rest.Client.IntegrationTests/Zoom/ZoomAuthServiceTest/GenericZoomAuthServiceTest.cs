using Infrastructure.Rest.Client.IntegrationTests.Helpers;
using Infrastructure.Rest.Client.Zoom;
using Infrastructure.Rest.Client.Zoom.Services;
using Microsoft.Extensions.Options;

namespace Infrastructure.Rest.Client.IntegrationTests.Zoom.ZoomAuthServiceTest;

/// <summary>
/// Base class for ZoomAuthService integration tests using deterministic HTTP stubs.
/// Each test controls Zoom API responses via FakeHttpMessageHandler.
/// </summary>
public abstract class GenericZoomAuthServiceTest : TestBase
{
    protected FakeHttpMessageHandler FakeHandler = default!;
    protected ZoomAuthService AuthService = default!;
    protected ZoomApiClient ApiClient = default!;

    [SetUp]
    public virtual void SetUp()
    {
        FakeHandler = new FakeHttpMessageHandler();
        ApiClient = new ZoomApiClient(CreateTestLogger<ZoomApiClient>(), new HttpClient(FakeHandler));
        AuthService = new ZoomAuthService(ApiClient, Options.Create(ZoomConfig));
    }

    [TearDown]
    public virtual void TearDown()
    {
        FakeHandler.Dispose();
    }

    protected static string BuildValidAuthTokenJson(int expiresIn = 3600) =>
        $$$"""{"access_token":"stub-access-token-xyz","token_type":"bearer","expires_in":{{{expiresIn}}}}""";
}
