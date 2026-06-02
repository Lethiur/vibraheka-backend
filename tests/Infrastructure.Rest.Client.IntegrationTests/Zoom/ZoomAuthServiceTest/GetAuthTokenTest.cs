using System.ComponentModel;
using System.Net;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;

namespace VibraHeka.Infrastructure.Rest.Client.IntegrationTests.Zoom.ZoomAuthServiceTest;

[TestFixture]
public sealed class GetAuthTokenTest : GenericZoomAuthServiceTest
{
    [Test]
    [DisplayName("Should return success with a valid token when Zoom returns 200 and valid JSON")]
    public async Task ShouldReturnSuccessWhenZoomReturnsValidToken()
    {
        // Given: Zoom OAuth endpoint returns 200 OK with a valid token
        FakeHandler.EnqueueJson(HttpStatusCode.OK, BuildValidAuthTokenJson());

        // When: GetAuthTokenAsync is called
        Result<string> result = await AuthService.GetAuthTokenAsync(CancellationToken.None);

        // Then: result is success and token is not empty
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(result.Value, Is.EqualTo("stub-access-token-xyz"),
            $"Expected token 'stub-access-token-xyz' but got '{(result.IsSuccess ? result.Value : "N/A")}'");
        Assert.That(FakeHandler.SentRequests.Count, Is.EqualTo(1),
            "Expected exactly 1 HTTP request to obtain the token");
    }

    [Test]
    [DisplayName("Should return Z-001 failure when Zoom returns non-success status code")]
    public async Task ShouldReturnFailureWhenZoomReturnsNonSuccessStatus()
    {
        // Given: Zoom OAuth endpoint returns 401 Unauthorized
        FakeHandler.EnqueueStatusOnly(HttpStatusCode.Unauthorized);

        // When: GetAuthTokenAsync is called
        Result<string> result = await AuthService.GetAuthTokenAsync(CancellationToken.None);

        // Then: result is failure propagating Z-001
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when Zoom returns 401, but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToRetrieveToken),
            $"Expected error '{ZoomErrors.FailedToRetrieveToken}' but got '{result.Error}'");
    }

    [Test]
    [DisplayName("Should return Z-001 failure when Zoom returns 200 but body is invalid JSON")]
    public async Task ShouldReturnFailureWhenZoomReturnsInvalidJsonBody()
    {
        // Given: Zoom OAuth endpoint returns 200 but with invalid JSON body
        FakeHandler.EnqueueJson(HttpStatusCode.OK, "not-valid-json");

        // When: GetAuthTokenAsync is called
        Result<string> result = await AuthService.GetAuthTokenAsync(CancellationToken.None);

        // Then: result is failure with Z-001
        Assert.That(result.IsFailure, Is.True,
            "Expected failure for invalid JSON body, but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToRetrieveToken),
            $"Expected error '{ZoomErrors.FailedToRetrieveToken}' but got '{result.Error}'");
    }

    [Test]
    [DisplayName("Should return cached token on second call without making a new HTTP request")]
    public async Task ShouldReturnCachedTokenWithoutNewHttpCallWhenTokenIsStillValid()
    {
        // Given: first call obtains a token with 3600s expiry
        FakeHandler.EnqueueJson(HttpStatusCode.OK, BuildValidAuthTokenJson(expiresIn: 3600));
        Result<string> firstResult = await AuthService.GetAuthTokenAsync(CancellationToken.None);
        Assert.That(firstResult.IsSuccess, Is.True,
            $"Setup: first GetAuthTokenAsync must succeed, got: '{(firstResult.IsFailure ? firstResult.Error : "N/A")}'");

        // When: GetAuthTokenAsync is called a second time (no new response queued)
        Result<string> secondResult = await AuthService.GetAuthTokenAsync(CancellationToken.None);

        // Then: returns the same token, no additional HTTP call was made
        Assert.That(secondResult.IsSuccess, Is.True,
            $"Expected cached token success but got failure: '{(secondResult.IsFailure ? secondResult.Error : "N/A")}'");
        Assert.That(secondResult.Value, Is.EqualTo(firstResult.Value),
            $"Expected cached token '{firstResult.Value}' but got '{secondResult.Value}'");
        Assert.That(FakeHandler.SentRequests.Count, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request (cache hit), but got {FakeHandler.SentRequests.Count}");
    }

    [Test]
    [DisplayName("Should renew token via new HTTP call when the cached token has expired")]
    public async Task ShouldRenewTokenWhenCachedTokenHasExpired()
    {
        // Given: first call returns a token that expires in ~1 second (expires_in=61, margin=60 ? 1s TTL)
        FakeHandler.EnqueueJson(HttpStatusCode.OK, BuildValidAuthTokenJson(expiresIn: 61));
        Result<string> firstResult = await AuthService.GetAuthTokenAsync(CancellationToken.None);
        Assert.That(firstResult.IsSuccess, Is.True,
            $"Setup: first GetAuthTokenAsync must succeed, got: '{(firstResult.IsFailure ? firstResult.Error : "N/A")}'");

        // Wait for the token to expire (1s TTL + 500ms safety margin)
        await Task.Delay(1500);

        // Queue a second token response for the renewal request
        FakeHandler.EnqueueJson(HttpStatusCode.OK, BuildValidAuthTokenJson(expiresIn: 3600));

        // When: GetAuthTokenAsync is called after expiry
        Result<string> renewedResult = await AuthService.GetAuthTokenAsync(CancellationToken.None);

        // Then: a new HTTP call was made (2 total) and the renewed token is returned
        Assert.That(renewedResult.IsSuccess, Is.True,
            $"Expected success after token renewal but got: '{(renewedResult.IsFailure ? renewedResult.Error : "N/A")}'");
        Assert.That(FakeHandler.SentRequests.Count, Is.EqualTo(2),
            $"Expected 2 HTTP requests (initial + renewal), but got {FakeHandler.SentRequests.Count}");
    }
}
