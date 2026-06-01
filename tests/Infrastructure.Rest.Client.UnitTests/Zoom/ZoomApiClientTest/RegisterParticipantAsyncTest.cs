using System.ComponentModel;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;
using Infrastructure.Rest.Client.Zoom.Models;

namespace VibraHeka.Infrastructure.Rest.Client.UnitTests.Zoom.ZoomApiClientTest;

[TestFixture]
public sealed class RegisterParticipantAsyncTest : GenericZoomApiClientTest
{
    [Test]
    [DisplayName("Should return success when HTTP response is 200 OK with valid body")]
    public async Task ShouldReturnSuccessWhenHttpResponseIsOkWithValidBody()
    {
        // Given: HTTP responds with 200 OK and a valid registrant JSON body
        FakeHandler.EnqueueResponse(BuildRegisterParticipantSuccessResponse(
            meetingId: 123456789L,
            registrantId: "reg-abc-123",
            joinUrl: "https://zoom.us/j/123456789?tk=abc"));

        // When: RegisterParticipantAsync is called
        Result<ZoomCreateRegistrantResposne> result = await ApiClient.RegisterParticipantAsync(
            ValidAuthToken, BuildValidRegisterRequest(), CancellationToken.None);

        // Then: result is success with registrant details
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(result.Value.MeetingId, Is.EqualTo(123456789L),
            $"Expected meeting ID 123456789 but got '{(result.IsSuccess ? result.Value.MeetingId : 0L)}'");
        Assert.That(result.Value.RegistrantId, Is.EqualTo("reg-abc-123"),
            $"Expected registrant ID 'reg-abc-123' but got '{(result.IsSuccess ? result.Value.RegistrantId : "N/A")}'");
        Assert.That(result.Value.JoinUrl, Is.EqualTo("https://zoom.us/j/123456789?tk=abc"),
            $"Expected join URL but got '{(result.IsSuccess ? result.Value.JoinUrl : "N/A")}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return Z-004 failure when HTTP response is not 200 OK")]
    public async Task ShouldReturnFailureWhenHttpResponseIsNotOk()
    {
        // Given: HTTP responds with 400 Bad Request
        FakeHandler.EnqueueResponse(BuildRegisterParticipantFailureResponse());

        // When: RegisterParticipantAsync is called
        Result<ZoomCreateRegistrantResposne> result = await ApiClient.RegisterParticipantAsync(
            ValidAuthToken, BuildValidRegisterRequest(), CancellationToken.None);

        // Then: result is failure with Z-004 error code
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with value: '{(result.IsSuccess ? result.Value.RegistrantId : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToRegisterParticipant),
            $"Expected error code '{ZoomErrors.FailedToRegisterParticipant}' but got '{result.Error}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return Z-004 failure when response body is null")]
    public async Task ShouldReturnFailureWhenBodyIsNull()
    {
        // Given: HTTP responds with 200 OK but body is JSON null
        FakeHandler.EnqueueResponse(BuildRegisterParticipantNullBodyResponse());

        // When: RegisterParticipantAsync is called
        Result<ZoomCreateRegistrantResposne> result = await ApiClient.RegisterParticipantAsync(
            ValidAuthToken, BuildValidRegisterRequest(), CancellationToken.None);

        // Then: result is failure with Z-004 error code
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure for null body but got success");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToRegisterParticipant),
            $"Expected error code '{ZoomErrors.FailedToRegisterParticipant}' but got '{result.Error}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request but found {FakeHandler.RequestCount}");
    }
}


