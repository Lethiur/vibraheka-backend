using System.ComponentModel;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;
using MediatR;

namespace VibraHeka.Infrastructure.Rest.Client.UnitTests.Zoom.ZoomApiClientTest;

[TestFixture]
public sealed class DeleteMeetingAsyncTest : GenericZoomApiClientTest
{
    [Test]
    [DisplayName("Should return success when HTTP response is 204 No Content")]
    public async Task ShouldReturnSuccessWhenHttpResponseIsNoContent()
    {
        // Given: HTTP responds with 204 No Content
        FakeHandler.EnqueueResponse(BuildDeleteMeetingSuccessResponse());

        // When: DeleteMeetingAsync is called
        Result<Unit> result = await ApiClient.DeleteMeetingAsync(
            ValidAuthToken, 123456789L, CancellationToken.None);

        // Then: result is success
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return Z-003 failure when HTTP response is not 204 No Content")]
    public async Task ShouldReturnFailureWhenHttpResponseIsNotNoContent()
    {
        // Given: HTTP responds with 404 Not Found
        FakeHandler.EnqueueResponse(BuildDeleteMeetingFailureResponse());

        // When: DeleteMeetingAsync is called
        Result<Unit> result = await ApiClient.DeleteMeetingAsync(
            ValidAuthToken, 123456789L, CancellationToken.None);

        // Then: result is failure with Z-003 error code
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToDeleteMeeting),
            $"Expected error code '{ZoomErrors.FailedToDeleteMeeting}' but got '{result.Error}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request but found {FakeHandler.RequestCount}");
    }
}


