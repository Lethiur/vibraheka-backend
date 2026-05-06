using System.ComponentModel;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;
using VibraHeka.Domain.Events.Models;

namespace Infrastructure.Rest.Client.UnitTests.Zoom.MeetingAdapterTest;

[TestFixture]
public sealed class RegisterAttendeeAsyncTest : GenericMeetingAdapterTest
{
    [Test]
    [DisplayName("Should return success with registrant result when auth and register participant succeed")]
    public async Task ShouldReturnSuccessWhenAuthAndRegisterParticipantSucceed()
    {
        // Given: auth returns a valid token and register participant returns 200 with valid body
        EnqueueAuthSuccess();
        EnqueueRegisterParticipantSuccess(
            meetingId: 123456789L,
            registrantId: "reg-abc-123",
            joinUrl: "https://zoom.us/j/123456789?tk=abc");
        RegisterAttendeeModel model = BuildValidRegisterAttendeeModel();

        // When: RegisterAttendeeAsync is called
        Result<RegisterAttendeeResult> result = await Adapter.RegisterAttendeeAsync(model, CancellationToken.None);

        // Then: result is success with mapped registrant details
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(result.Value.EventID, Is.EqualTo(123456789L),
            $"Expected event ID 123456789 but got '{(result.IsSuccess ? result.Value.EventID : 0L)}'");
        Assert.That(result.Value.RegistrantID, Is.EqualTo("reg-abc-123"),
            $"Expected registrant ID 'reg-abc-123' but got '{(result.IsSuccess ? result.Value.RegistrantID : "N/A")}'");
        Assert.That(result.Value.JoinURL, Is.EqualTo("https://zoom.us/j/123456789?tk=abc"),
            $"Expected join URL but got '{(result.IsSuccess ? result.Value.JoinURL : "N/A")}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(2),
            $"Expected 2 HTTP requests (auth + register) but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return failure with auth error when authentication fails")]
    public async Task ShouldReturnFailureWhenAuthFails()
    {
        // Given: auth HTTP call returns 401 Unauthorized
        EnqueueAuthFailure();
        RegisterAttendeeModel model = BuildValidRegisterAttendeeModel();

        // When: RegisterAttendeeAsync is called
        Result<RegisterAttendeeResult> result = await Adapter.RegisterAttendeeAsync(model, CancellationToken.None);

        // Then: result is failure with auth error and NO register was attempted
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure when auth fails but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToRetrieveToken),
            $"Expected auth error '{ZoomErrors.FailedToRetrieveToken}' but got '{result.Error}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request (auth only — no register attempted) but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return Z-004 failure when register participant returns error status")]
    public async Task ShouldReturnFailureWhenRegisterParticipantFails()
    {
        // Given: auth succeeds but register participant returns 400
        EnqueueAuthSuccess();
        EnqueueRegisterParticipantFailure();
        RegisterAttendeeModel model = BuildValidRegisterAttendeeModel();

        // When: RegisterAttendeeAsync is called
        Result<RegisterAttendeeResult> result = await Adapter.RegisterAttendeeAsync(model, CancellationToken.None);

        // Then: result is failure with Z-004 error code
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure when register participant fails but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToRegisterParticipant),
            $"Expected error '{ZoomErrors.FailedToRegisterParticipant}' but got '{result.Error}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(2),
            $"Expected 2 HTTP requests (auth + register) but found {FakeHandler.RequestCount}");
    }
}


