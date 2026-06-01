using System.ComponentModel;
using System.Net;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;
using Infrastructure.Rest.Client.Zoom.Models;

namespace VibraHeka.Infrastructure.Rest.Client.IntegrationTests.Zoom.ZoomApiClientTest;

[TestFixture]
public sealed class GetAuthTokenTest : GenericZoomApiClientTest
{
    [Test]
    [DisplayName("Should return success with token when Zoom returns 200 and valid JSON")]
    public async Task ShouldReturnSuccessWhenResponseIsValidJson()
    {
        // Given: Zoom OAuth endpoint returns 200 with a valid token JSON
        FakeHandler.EnqueueJson(HttpStatusCode.OK, BuildValidAuthTokenJson());

        // When: GetAuthToken is called with stub credentials
        Result<ZoomAuthTokenResponse> result = await ApiClient.GetAuthToken(
            "stub-client-id",
            "stub-client-secret",
            "stub-account-id",
            CancellationToken.None);

        // Then: result is success and contains the expected access token
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(result.Value.AccessToken, Is.EqualTo("stub-access-token-xyz"),
            $"Expected token 'stub-access-token-xyz' but got '{result.Value.AccessToken}'");
        Assert.That(result.Value.ExpiresIn, Is.EqualTo(3600),
            $"Expected ExpiresIn 3600 but got '{result.Value.ExpiresIn}'");
        Assert.That(FakeHandler.SentRequests.Count, Is.EqualTo(1),
            "Expected exactly 1 HTTP request to be sent");
    }

    [Test]
    [DisplayName("Should return Z-001 failure when Zoom returns non-success status code")]
    public async Task ShouldReturnFailureWhenResponseStatusIsNotSuccess()
    {
        // Given: Zoom OAuth endpoint returns 401 Unauthorized
        FakeHandler.EnqueueStatusOnly(HttpStatusCode.Unauthorized);

        // When: GetAuthToken is called
        Result<ZoomAuthTokenResponse> result = await ApiClient.GetAuthToken(
            "bad-client-id",
            "bad-client-secret",
            "stub-account-id",
            CancellationToken.None);

        // Then: result is failure with Z-001 error code
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when Zoom returns 401, but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToRetrieveToken),
            $"Expected error '{ZoomErrors.FailedToRetrieveToken}' but got '{result.Error}'");
        Assert.That(FakeHandler.SentRequests.Count, Is.EqualTo(1),
            "Expected exactly 1 HTTP request");
    }

    [Test]
    [DisplayName("Should return Z-001 failure when Zoom returns 200 but body deserializes to null")]
    public async Task ShouldReturnFailureWhenResponseBodyDeserializesToNull()
    {
        // Given: Zoom OAuth endpoint returns 200 with a JSON null body (deserializes to null reference)
        FakeHandler.EnqueueJson(HttpStatusCode.OK, "null");

        // When: GetAuthToken is called
        Result<ZoomAuthTokenResponse> result = await ApiClient.GetAuthToken(
            "stub-client-id",
            "stub-client-secret",
            "stub-account-id",
            CancellationToken.None);

        // Then: result is failure with Z-001 because deserialization yields null
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when Zoom body deserializes to null, but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToRetrieveToken),
            $"Expected error '{ZoomErrors.FailedToRetrieveToken}' but got '{result.Error}'");
    }
}

