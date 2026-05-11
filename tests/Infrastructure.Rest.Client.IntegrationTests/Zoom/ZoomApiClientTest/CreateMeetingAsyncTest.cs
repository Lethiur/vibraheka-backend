using System.ComponentModel;
using System.Net;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;
using Infrastructure.Rest.Client.Zoom.Models;

namespace Infrastructure.Rest.Client.IntegrationTests.Zoom.ZoomApiClientTest;

[TestFixture]
public sealed class CreateMeetingAsyncTest : GenericZoomApiClientTest
{
    [Test]
    [DisplayName("Should return success with meeting details when Zoom returns 201 Created")]
    public async Task ShouldReturnSuccessWhenZoomReturnsCreated()
    {
        // Given: Zoom create-meeting endpoint returns 201 Created with valid meeting JSON
        FakeHandler.EnqueueJson(HttpStatusCode.Created, BuildValidCreateMeetingResponseJson());
        ZoomCreateMeetingRequest request = BuildCreateMeetingRequest();

        // When: CreateMeetingAsync is called with a valid auth token
        Result<ZoomCreateMeetingResponse> result = await ApiClient.CreateMeetingAsync(
            "stub-bearer-token",
            "host@example.com",
            request,
            CancellationToken.None);

        // Then: result is success with expected meeting data
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(result.Value.Id, Is.EqualTo(987654321L),
            $"Expected meeting ID 987654321 but got '{result.Value.Id}'");
        Assert.That(result.Value.JoinUrl, Is.Not.Null.And.Not.Empty,
            "Expected a non-empty join URL from the stub response");
        Assert.That(FakeHandler.SentRequests.Count, Is.EqualTo(1),
            "Expected exactly 1 HTTP request to create the meeting");
    }

    [Test]
    [DisplayName("Should return Z-002 failure when Zoom returns non-success status code")]
    public async Task ShouldReturnFailureWhenZoomReturnsError()
    {
        // Given: Zoom create-meeting endpoint returns 400 Bad Request
        FakeHandler.EnqueueStatusOnly(HttpStatusCode.BadRequest);
        ZoomCreateMeetingRequest request = BuildCreateMeetingRequest();

        // When: CreateMeetingAsync is called
        Result<ZoomCreateMeetingResponse> result = await ApiClient.CreateMeetingAsync(
            "stub-bearer-token",
            "host@example.com",
            request,
            CancellationToken.None);

        // Then: result is failure with Z-002 error code
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when Zoom returns 400, but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToCreateMeeting),
            $"Expected error '{ZoomErrors.FailedToCreateMeeting}' but got '{result.Error}'");
    }
}

