using System.ComponentModel;
using System.Net;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;
using MediatR;

namespace VibraHeka.Infrastructure.Rest.Client.IntegrationTests.Zoom.ZoomApiClientTest;

[TestFixture]
public sealed class UnRegisterParticipantAsyncTest : GenericZoomApiClientTest
{
    [Test]
    [DisplayName("Should return success when Zoom returns 204 No Content")]
    public async Task ShouldReturnSuccessWhenZoomReturnsNoContent()
    {
        // Given: Zoom unregister-participant endpoint returns 204 No Content
        FakeHandler.EnqueueStatusOnly(HttpStatusCode.NoContent);
        global::Infrastructure.Rest.Client.Zoom.Models.ZoomUnRegisterRegistrantRequest request =
            BuildUnRegisterRegistrantRequest();

        // When: UnRegisterParticipantAsync is called
        Result<Unit> result = await ApiClient.UnRegisterParticipantAsync(
            "stub-bearer-token",
            request,
            CancellationToken.None);

        // Then: result is success
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success on 204 but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(FakeHandler.SentRequests.Count, Is.EqualTo(1),
            "Expected exactly 1 HTTP DELETE request for unregister");
    }

    [Test]
    [DisplayName("Should return Z-005 failure when Zoom returns status other than 204")]
    public async Task ShouldReturnFailureWhenZoomReturnsNonNoContentStatus()
    {
        // Given: Zoom unregister-participant endpoint returns 404 Not Found
        FakeHandler.EnqueueStatusOnly(HttpStatusCode.NotFound);
        global::Infrastructure.Rest.Client.Zoom.Models.ZoomUnRegisterRegistrantRequest request =
            BuildUnRegisterRegistrantRequest(meetingId: 111111111L, registrantId: "unknown-reg");

        // When: UnRegisterParticipantAsync is called
        Result<Unit> result = await ApiClient.UnRegisterParticipantAsync(
            "stub-bearer-token",
            request,
            CancellationToken.None);

        // Then: result is failure with Z-005 error code
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when Zoom returns 404, but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToUnregisterParticipant),
            $"Expected error '{ZoomErrors.FailedToUnregisterParticipant}' but got '{result.Error}'");
    }
}

