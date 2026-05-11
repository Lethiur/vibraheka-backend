using System.ComponentModel;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;
using MediatR;
using VibraHeka.Domain.Events.Models;

namespace Infrastructure.Rest.Client.UnitTests.Zoom.MeetingAdapterTest;

[TestFixture]
public sealed class UnRegisterAttendeeAsyncTest : GenericMeetingAdapterTest
{
    [Test]
    [DisplayName("Should return success when auth and unregister participant both succeed")]
    public async Task ShouldReturnSuccessWhenAuthAndUnRegisterParticipantSucceed()
    {
        // Given: auth returns a valid token and unregister returns 204
        EnqueueAuthSuccess();
        EnqueueUnRegisterParticipantSuccess();
        UnRegisterAttendeeModel model = BuildValidUnRegisterAttendeeModel();

        // When: UnRegisterAttendeeAsync is called
        Result<Unit> result = await Adapter.UnRegisterAttendeeAsync(model, CancellationToken.None);

        // Then: result is success
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(2),
            $"Expected 2 HTTP requests (auth + unregister) but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return failure with auth error when authentication fails")]
    public async Task ShouldReturnFailureWhenAuthFails()
    {
        // Given: auth HTTP call returns 401 Unauthorized
        EnqueueAuthFailure();
        UnRegisterAttendeeModel model = BuildValidUnRegisterAttendeeModel();

        // When: UnRegisterAttendeeAsync is called
        Result<Unit> result = await Adapter.UnRegisterAttendeeAsync(model, CancellationToken.None);

        // Then: result is failure with auth error and NO unregister was attempted
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure when auth fails but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToRetrieveToken),
            $"Expected auth error '{ZoomErrors.FailedToRetrieveToken}' but got '{result.Error}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request (auth only — no unregister attempted) but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return Z-005 failure when unregister participant returns error status")]
    public async Task ShouldReturnFailureWhenUnRegisterParticipantFails()
    {
        // Given: auth succeeds but unregister participant returns 400
        EnqueueAuthSuccess();
        EnqueueUnRegisterParticipantFailure();
        UnRegisterAttendeeModel model = BuildValidUnRegisterAttendeeModel();

        // When: UnRegisterAttendeeAsync is called
        Result<Unit> result = await Adapter.UnRegisterAttendeeAsync(model, CancellationToken.None);

        // Then: result is failure with Z-005 error code
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure when unregister fails but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToUnregisterParticipant),
            $"Expected error '{ZoomErrors.FailedToUnregisterParticipant}' but got '{result.Error}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(2),
            $"Expected 2 HTTP requests (auth + unregister) but found {FakeHandler.RequestCount}");
    }
}


