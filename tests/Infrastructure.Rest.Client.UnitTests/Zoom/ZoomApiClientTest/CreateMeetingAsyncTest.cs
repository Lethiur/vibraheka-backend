using System.ComponentModel;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;
using Infrastructure.Rest.Client.Zoom.Models;

namespace VibraHeka.Infrastructure.Rest.Client.UnitTests.Zoom.ZoomApiClientTest;

[TestFixture]
public sealed class CreateMeetingAsyncTest : GenericZoomApiClientTest
{
    [Test]
    [DisplayName("Should return success when HTTP response is 201 Created with valid body")]
    public async Task ShouldReturnSuccessWhenHttpResponseIsCreatedWithValidBody()
    {
        // Given: HTTP responds with 201 Created and a valid meeting JSON body
        FakeHandler.EnqueueResponse(BuildCreateMeetingSuccessResponse(
            meetingId: 987654321L,
            joinUrl: "https://zoom.us/j/987654321",
            startUrl: "https://zoom.us/s/987654321",
            password: "Secret123",
            registrationUrl: "https://zoom.us/meeting/register/xyz"));

        // When: CreateMeetingAsync is called
        Result<ZoomCreateMeetingResponse> result = await ApiClient.CreateMeetingAsync(
            ValidAuthToken, ValidHostEmail, BuildValidCreateMeetingRequest(), CancellationToken.None);

        // Then: result is success with meeting details
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(result.Value.Id, Is.EqualTo(987654321L),
            $"Expected meeting ID 987654321 but got '{(result.IsSuccess ? result.Value.Id : 0L)}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return success when HTTP response is 200 OK with valid body")]
    public async Task ShouldReturnSuccessWhenHttpResponseIsOkWithValidBody()
    {
        // Given: HTTP responds with 200 OK and a valid meeting JSON body
        FakeHandler.EnqueueResponse(BuildCreateMeetingHttpOkResponse(
            meetingId: 111222333L,
            joinUrl: "https://zoom.us/j/111222333",
            startUrl: "https://zoom.us/s/111222333",
            password: "Pass200",
            registrationUrl: "https://zoom.us/meeting/register/ok200"));

        // When: CreateMeetingAsync is called
        Result<ZoomCreateMeetingResponse> result = await ApiClient.CreateMeetingAsync(
            ValidAuthToken, ValidHostEmail, BuildValidCreateMeetingRequest(), CancellationToken.None);

        // Then: result is success with meeting details
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success for HTTP 200 but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(result.Value.Id, Is.EqualTo(111222333L),
            $"Expected meeting ID 111222333 but got '{(result.IsSuccess ? result.Value.Id : 0L)}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return Z-002 failure when HTTP response is error status")]
    public async Task ShouldReturnFailureWhenHttpResponseIsErrorStatus()
    {
        // Given: HTTP responds with 400 Bad Request
        FakeHandler.EnqueueResponse(BuildCreateMeetingFailureResponse());

        // When: CreateMeetingAsync is called
        Result<ZoomCreateMeetingResponse> result = await ApiClient.CreateMeetingAsync(
            ValidAuthToken, ValidHostEmail, BuildValidCreateMeetingRequest(), CancellationToken.None);

        // Then: result is failure with Z-002 error code
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with value: '{(result.IsSuccess ? result.Value.Id : 0L)}'");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToCreateMeeting),
            $"Expected error code '{ZoomErrors.FailedToCreateMeeting}' but got '{result.Error}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request but found {FakeHandler.RequestCount}");
    }

    [Test]
    [DisplayName("Should return Z-002 failure when response body deserializes to null")]
    public async Task ShouldReturnFailureWhenDeserializationYieldsNull()
    {
        // Given: HTTP responds with 201 Created but body is JSON null
        FakeHandler.EnqueueResponse(BuildCreateMeetingNullBodyResponse());

        // When: CreateMeetingAsync is called
        Result<ZoomCreateMeetingResponse> result = await ApiClient.CreateMeetingAsync(
            ValidAuthToken, ValidHostEmail, BuildValidCreateMeetingRequest(), CancellationToken.None);

        // Then: result is failure with Z-002 error code
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure for null body but got success.");
        Assert.That(result.Error, Is.EqualTo(ZoomErrors.FailedToCreateMeeting),
            $"Expected error code '{ZoomErrors.FailedToCreateMeeting}' but got '{result.Error}'");
        Assert.That(FakeHandler.RequestCount, Is.EqualTo(1),
            $"Expected exactly 1 HTTP request but found {FakeHandler.RequestCount}");
    }
}
