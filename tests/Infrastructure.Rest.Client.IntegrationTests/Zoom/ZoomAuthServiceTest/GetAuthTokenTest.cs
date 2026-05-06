using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom;
using Infrastructure.Rest.Client.Zoom.Services;
using Microsoft.Extensions.Options;

namespace Infrastructure.Rest.Client.IntegrationTests.Zoom.ZoomAuthServiceTest;

[TestFixture]
public class GetAuthTokenTest : TestBase
{
    [Test]
    public async Task ShouldGetAuthToken()
    {
        // Given: Auth service
        ZoomAuthService zoomAuthService = new ZoomAuthService(new ZoomApiClient(CreateTestLogger<ZoomApiClient>(), new HttpClient()), Options.Create(ZoomConfig));
        
        // When: Token is requested
        Result<string> authToken = await zoomAuthService.GetAuthTokenAsync(CancellationToken.None);
        
        // Then: Token is returned
        Assert.That(authToken.IsSuccess);
    }
}
