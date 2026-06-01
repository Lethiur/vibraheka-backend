using System.ComponentModel;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;
using VibraHeka.Domain.Events.Models;

namespace VibraHeka.Infrastructure.Rest.Client.UnitTests.Zoom.MeetingAdapterTest;

[TestFixture]
public sealed class ScheduleMeetingAsyncTest : GenericMeetingAdapterTest
{
    [Test]
    [DisplayName("Should return success with event result when auth and create meeting succeed")]
    public async Task ShouldReturnSuccessWhenAuthAndCreateMeetingSucceed()
    {
        // Given: auth returns a valid token and create meeting returns 201
        EnqueueAuthSuccess();
        EnqueueCreateMeetingSuccess(
            meetingId: 987654321L,
            joinUrl: "https://zoom.us/j/987654321",
            startUrl: "https://zoom.us/s/987654321",
            password: "Secret123",
            registrationUrl: "https://zoom.us/meeting/register/xyz");
        CreateEventModel model = BuildValidCreateEventModel();

        // When: ScheduleMeetingAsync is called
        Result<CreateEventResult> result = await Adapter.ScheduleMeetingAsync(model, CancellationToken.None);

        // Then: result is success with mapped event details
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(result.Value.EventID, Is.EqualTo(987654321L),
            $"Expected event ID 987654321 but got '{(result.IsSuccess ? result.Value.EventID : 0L)}'");
        Assert.That(result.Value.JoinURL, Is.Not.Empty,
            "Expected non-empty join URL");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(2),
            $"Expected 2 HTTP requests (auth + create) but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return failure with auth error when authentication fails")]
    public async Task ShouldReturnFailureWhenAuthFails()
    {
        // Given: auth HTTP call returns 401 Unauthorized
        EnqueueAuthFailure();
        CreateEventModel model = BuildValidCreateEventModel();

        // When: ScheduleMeetingAsync is called
        Result<CreateEventResult> result = await Adapter.ScheduleMeetingAsync(model, CancellationToken.None);

        // Then: result is failure with auth error and NO meeting creation was attempted
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure when auth fails but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToRetrieveToken),
            $"Expected auth error '{ZoomErrors.FailedToRetrieveToken}' but got '{result.Error}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request (auth only — no meeting creation) but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return Z-002 failure when create meeting returns error status")]
    public async Task ShouldReturnFailureWhenCreateMeetingFails()
    {
        // Given: auth succeeds but create meeting returns 400
        EnqueueAuthSuccess();
        EnqueueCreateMeetingFailure();
        CreateEventModel model = BuildValidCreateEventModel();

        // When: ScheduleMeetingAsync is called
        Result<CreateEventResult> result = await Adapter.ScheduleMeetingAsync(model, CancellationToken.None);

        // Then: result is failure with Z-002 error code
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure when create meeting fails but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToCreateMeeting),
            $"Expected error '{ZoomErrors.FailedToCreateMeeting}' but got '{result.Error}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(2),
            $"Expected 2 HTTP requests (auth + create) but found {FakeHandler.RequestCount}");
    }
}
