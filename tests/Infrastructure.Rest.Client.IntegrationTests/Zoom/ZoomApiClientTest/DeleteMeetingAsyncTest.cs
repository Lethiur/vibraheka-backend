using System.ComponentModel;
using System.Net;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;
using MediatR;

namespace Infrastructure.Rest.Client.IntegrationTests.Zoom.ZoomApiClientTest;

[TestFixture]
public sealed class DeleteMeetingAsyncTest : GenericZoomApiClientTest
{
    [Test]
    [DisplayName("Should return success when Zoom returns 204 No Content")]
    public async Task ShouldReturnSuccessWhenZoomReturnsNoContent()
    {
        // Given: Zoom DELETE meeting endpoint returns 204 No Content
        FakeHandler.EnqueueStatusOnly(HttpStatusCode.NoContent);

        // When: DeleteMeetingAsync is called with a valid meeting ID
        Result<Unit> result = await ApiClient.DeleteMeetingAsync(
            "stub-bearer-token",
            987654321L,
            CancellationToken.None);

        // Then: result is success
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success on 204 but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(FakeHandler.SentRequests.Count, Is.EqualTo(1),
            "Expected exactly 1 HTTP DELETE request");
    }

    [Test]
    [DisplayName("Should return Z-003 failure when Zoom returns status other than 204")]
    public async Task ShouldReturnFailureWhenZoomReturnsNonNoContentStatus()
    {
        // Given: Zoom DELETE meeting endpoint returns 404 Not Found
        FakeHandler.EnqueueStatusOnly(HttpStatusCode.NotFound);

        // When: DeleteMeetingAsync is called
        Result<Unit> result = await ApiClient.DeleteMeetingAsync(
            "stub-bearer-token",
            999999999L,
            CancellationToken.None);

        // Then: result is failure with Z-003 error code
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when Zoom returns 404, but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToDeleteMeeting),
            $"Expected error '{ZoomErrors.FailedToDeleteMeeting}' but got '{result.Error}'");
    }
}

