using System.ComponentModel;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;
using Infrastructure.Rest.Client.Zoom.Models;

namespace VibraHeka.Infrastructure.Rest.Client.UnitTests.Zoom.ZoomApiClientTest;

[TestFixture]
public sealed class GetAuthTokenTest : GenericZoomApiClientTest
{
    [Test]
    [DisplayName("Should return success with token when HTTP response is OK and body is valid")]
    public async Task ShouldReturnSuccessWhenHttpResponseIsOkAndBodyIsValid()
    {
        // Given: HTTP responds with 200 OK and a valid token JSON body
        FakeHandler.EnqueueResponse(BuildAuthSuccessResponse(accessToken: "my-access-token", expiresIn: 3600));

        // When: GetAuthToken is called
        Result<ZoomAuthTokenResponse> result = await ApiClient.GetAuthToken(
            ValidClientId, ValidClientSecret, ValidAccountId, CancellationToken.None);

        // Then: result is success with expected token
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(result.Value.AccessToken, Is.EqualTo("my-access-token"),
            $"Expected token 'my-access-token' but got '{result.Value.AccessToken}'");
        Assert.That(result.Value.ExpiresIn, Is.EqualTo(3600),
            $"Expected expiresIn 3600 but got '{result.Value.ExpiresIn}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return Z-001 failure when HTTP response is not success")]
    public async Task ShouldReturnFailureWhenHttpResponseIsNotSuccess()
    {
        // Given: HTTP responds with 401 Unauthorized
        FakeHandler.EnqueueResponse(BuildAuthFailureResponse());

        // When: GetAuthToken is called
        Result<ZoomAuthTokenResponse> result = await ApiClient.GetAuthToken(
            ValidClientId, ValidClientSecret, ValidAccountId, CancellationToken.None);

        // Then: result is failure with Z-001 error code
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with value: '{(result.IsSuccess ? result.Value.AccessToken : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToRetrieveToken),
            $"Expected error code '{ZoomErrors.FailedToRetrieveToken}' but got '{result.Error}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return Z-001 failure when response body deserializes to null")]
    public async Task ShouldReturnFailureWhenDeserializationYieldsNull()
    {
        // Given: HTTP responds with 200 OK but body is JSON null
        FakeHandler.EnqueueResponse(BuildAuthNullBodyResponse());

        // When: GetAuthToken is called
        Result<ZoomAuthTokenResponse> result = await ApiClient.GetAuthToken(
            ValidClientId, ValidClientSecret, ValidAccountId, CancellationToken.None);

        // Then: result is failure with Z-001 error code
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with value: '{(result.IsSuccess ? result.Value.AccessToken : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToRetrieveToken),
            $"Expected error code '{ZoomErrors.FailedToRetrieveToken}' but got '{result.Error}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request but found {FakeHandler.RequestCount}");
    }
}


