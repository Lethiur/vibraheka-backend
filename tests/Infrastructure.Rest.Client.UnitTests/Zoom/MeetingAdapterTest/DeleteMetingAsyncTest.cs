using System.ComponentModel;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;
using MediatR;

namespace VibraHeka.Infrastructure.Rest.Client.UnitTests.Zoom.MeetingAdapterTest;

[TestFixture]
public sealed class DeleteMetingAsyncTest : GenericMeetingAdapterTest
{
    [Test]
    [DisplayName("Should return success when auth and delete meeting both succeed")]
    public async Task ShouldReturnSuccessWhenAuthAndDeleteMeetingSucceed()
    {
        // Given: auth returns a valid token and delete meeting returns 204
        EnqueueAuthSuccess();
        EnqueueDeleteMeetingSuccess();

        // When: DeleteMetingAsync is called
        Result<Unit> result = await Adapter.DeleteMetingAsync(123456789L, CancellationToken.None);

        // Then: result is success
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(2),
            $"Expected 2 HTTP requests (auth + delete) but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return failure with auth error when authentication fails")]
    public async Task ShouldReturnFailureWhenAuthFails()
    {
        // Given: auth HTTP call returns 401 Unauthorized
        EnqueueAuthFailure();

        // When: DeleteMetingAsync is called
        Result<Unit> result = await Adapter.DeleteMetingAsync(123456789L, CancellationToken.None);

        // Then: result is failure with auth error and NO delete was attempted
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure when auth fails but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToRetrieveToken),
            $"Expected auth error '{ZoomErrors.FailedToRetrieveToken}' but got '{result.Error}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request (auth only — no delete attempted) but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return Z-003 failure when delete meeting returns error status")]
    public async Task ShouldReturnFailureWhenDeleteMeetingFails()
    {
        // Given: auth succeeds but delete meeting returns 404
        EnqueueAuthSuccess();
        EnqueueDeleteMeetingFailure();

        // When: DeleteMetingAsync is called
        Result<Unit> result = await Adapter.DeleteMetingAsync(123456789L, CancellationToken.None);

        // Then: result is failure with Z-003 error code
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure when delete meeting returns error but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToDeleteMeeting),
            $"Expected error '{ZoomErrors.FailedToDeleteMeeting}' but got '{result.Error}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(2),
            $"Expected 2 HTTP requests (auth + delete) but found {FakeHandler.RequestCount}");
    }
}


