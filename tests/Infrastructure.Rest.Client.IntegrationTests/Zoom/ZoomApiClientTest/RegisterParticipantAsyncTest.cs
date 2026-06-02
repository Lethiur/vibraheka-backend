using System.ComponentModel;
using System.Net;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;
using Infrastructure.Rest.Client.Zoom.Models;

namespace VibraHeka.Infrastructure.Rest.Client.IntegrationTests.Zoom.ZoomApiClientTest;

[TestFixture]
public sealed class RegisterParticipantAsyncTest : GenericZoomApiClientTest
{
    [Test]
    [DisplayName("Should return success with registrant details when Zoom returns 200 OK and valid body")]
    public async Task ShouldReturnSuccessWhenZoomReturnsOkWithRegistrantBody()
    {
        // Given: Zoom register-participant endpoint returns 200 OK with valid registrant JSON
        FakeHandler.EnqueueJson(HttpStatusCode.Created, BuildValidRegistrantResponseJson());
        ZoomRegisterRegistrantRequest request = BuildRegisterRegistrantRequest();

        // When: RegisterParticipantAsync is called
        Result<ZoomCreateRegistrantResposne> result = await ApiClient.RegisterParticipantAsync(
            "stub-bearer-token",
            request,
            CancellationToken.None);

        // Then: result is success with expected registrant data
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(result.Value.RegistrantId, Is.EqualTo("reg-stub-001"),
            $"Expected registrant ID 'reg-stub-001' but got '{result.Value.RegistrantId}'");
        Assert.That(result.Value.JoinUrl, Is.Not.Null.And.Not.Empty,
            "Expected a non-empty join URL from the stub response");
        Assert.That(FakeHandler.SentRequests.Count, Is.EqualTo(1),
            "Expected exactly 1 HTTP request to register the participant");
    }

    [Test]
    [DisplayName("Should return Z-004 failure when Zoom returns non-200 status code")]
    public async Task ShouldReturnFailureWhenZoomReturnsNonOkStatus()
    {
        // Given: Zoom register-participant endpoint returns 400 Bad Request
        FakeHandler.EnqueueStatusOnly(HttpStatusCode.BadRequest);
        ZoomRegisterRegistrantRequest request = BuildRegisterRegistrantRequest();

        // When: RegisterParticipantAsync is called
        Result<ZoomCreateRegistrantResposne> result = await ApiClient.RegisterParticipantAsync(
            "stub-bearer-token",
            request,
            CancellationToken.None);

        // Then: result is failure with Z-004 error code
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when Zoom returns 400, but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToRegisterParticipant),
            $"Expected error '{ZoomErrors.FailedToRegisterParticipant}' but got '{result.Error}'");
    }
}

