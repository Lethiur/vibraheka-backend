using System.ComponentModel;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;

namespace VibraHeka.Infrastructure.Rest.Client.UnitTests.Zoom.ZoomAuthServiceTest;

[TestFixture]
public sealed class GetAuthTokenAsyncTest : GenericZoomAuthServiceTest
{
    [Test]
    [DisplayName("Should return success with valid token on first call")]
    public async Task ShouldReturnSuccessOnFirstCall()
    {
        // Given: HTTP responds with a valid token
        FakeHandler.EnqueueResponse(BuildTokenResponseWithExpiry(accessToken: "my-token", expiresIn: 3600));

        // When: GetAuthTokenAsync is called for the first time
        Result<string> result = await AuthService.GetAuthTokenAsync(CancellationToken.None);

        // Then: result is success and token matches
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(result.Value, Is.EqualTo("my-token"),
            $"Expected token 'my-token' but got '{(result.IsSuccess ? result.Value : "N/A")}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request on first call but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should reuse cached token on second call when token has not expired")]
    public async Task ShouldReturnCachedTokenWhenNotExpired()
    {
        // Given: first call succeeds with a long-lived token (3600s expiry)
        FakeHandler.EnqueueResponse(BuildTokenResponseWithExpiry(accessToken: "long-lived-token", expiresIn: 3600));
        Result<string> firstResult = await AuthService.GetAuthTokenAsync(CancellationToken.None);

        // When: second call is made immediately (token not yet expired)
        Result<string> secondResult = await AuthService.GetAuthTokenAsync(CancellationToken.None);

        // Then: second call returns cached token without making a new HTTP request
        Assert.That(firstResult.IsSuccess, Is.True,
            $"Expected first call to succeed but got error: '{(firstResult.IsFailure ? firstResult.Error : "N/A")}'");
        Assert.That(secondResult.IsSuccess, Is.True,
            $"Expected second call to succeed from cache but got error: '{(secondResult.IsFailure ? secondResult.Error : "N/A")}'");
        Assert.That(secondResult.Value, Is.EqualTo("long-lived-token"),
            $"Expected cached token 'long-lived-token' but got '{(secondResult.IsSuccess ? secondResult.Value : "N/A")}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request total (cache for second call) but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should request new token when cached token has expired (ExpiresIn=60 causes immediate expiry)")]
    public async Task ShouldRequestNewTokenWhenCachedTokenIsExpired()
    {
        // Given: first call returns a token that expires immediately (ExpiresIn=60 => UtcNow + 0s)
        FakeHandler.EnqueueResponse(BuildImmediatelyExpiredTokenResponse(accessToken: "short-lived-token"));
        FakeHandler.EnqueueResponse(BuildTokenResponseWithExpiry(accessToken: "refreshed-token", expiresIn: 3600));
        Result<string> firstResult = await AuthService.GetAuthTokenAsync(CancellationToken.None);

        // When: second call is made (token already expired)
        Result<string> secondResult = await AuthService.GetAuthTokenAsync(CancellationToken.None);

        // Then: a new HTTP request was made to refresh the token
        Assert.That(firstResult.IsSuccess, Is.True,
            $"Expected first call to succeed but got: '{(firstResult.IsFailure ? firstResult.Error : "N/A")}'");
        Assert.That(secondResult.IsSuccess, Is.True,
            $"Expected second call to succeed with refreshed token but got: '{(secondResult.IsFailure ? secondResult.Error : "N/A")}'");
        Assert.That(secondResult.Value, Is.EqualTo("refreshed-token"),
            $"Expected refreshed token but got '{(secondResult.IsSuccess ? secondResult.Value : "N/A")}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(2),
            $"Expected 2 HTTP requests (initial + refresh) but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return Z-001 failure when API client returns an error")]
    public async Task ShouldReturnFailureWhenApiClientFails()
    {
        // Given: HTTP responds with 401 Unauthorized
        FakeHandler.EnqueueResponse(BuildAuthFailureResponse());

        // When: GetAuthTokenAsync is called
        Result<string> result = await AuthService.GetAuthTokenAsync(CancellationToken.None);

        // Then: result is failure propagating the error from ZoomApiClient
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with value: '{(result.IsSuccess ? result.Value : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToRetrieveToken),
            $"Expected error code '{ZoomErrors.FailedToRetrieveToken}' but got '{result.Error}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request but found {FakeHandler.RequestCount}");
    }
}


