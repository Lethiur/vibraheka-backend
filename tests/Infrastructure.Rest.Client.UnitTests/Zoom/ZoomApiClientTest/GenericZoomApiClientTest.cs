using System.Net;
using System.Text;
using System.Text.Json;
using Infrastructure.Rest.Client.Zoom;
using Infrastructure.Rest.Client.Zoom.Models;
using Microsoft.Extensions.Logging.Abstractions;
using VibraHeka.Infrastructure.Rest.Client.UnitTests.Helpers;

namespace VibraHeka.Infrastructure.Rest.Client.UnitTests.Zoom.ZoomApiClientTest;

public abstract class GenericZoomApiClientTest
{
    protected FakeHttpMessageHandler FakeHandler = default!;
    protected ZoomApiClient ApiClient = default!;

    [SetUp]
    public virtual void SetUp()
    {
        FakeHandler = new FakeHttpMessageHandler();
        HttpClient httpClient = new(FakeHandler);
        ApiClient = new ZoomApiClient(NullLogger<ZoomApiClient>.Instance, httpClient);
    }

    [TearDown]
    public virtual void TearDown()
    {
        FakeHandler.Dispose();
    }

    protected static string ValidClientId => "test-client-id";
    protected static string ValidClientSecret => "test-client-secret";
    protected static string ValidAccountId => "test-account-id";
    protected static string ValidAuthToken => "test-access-token";
    protected static string ValidHostEmail => "host@example.com";

    protected static HttpResponseMessage BuildAuthSuccessResponse(
        string accessToken = "valid-token",
        int expiresIn = 3600)
    {
        ZoomAuthTokenResponse tokenResponse = new()
        {
            AccessToken = accessToken,
            ExpiresIn = expiresIn,
        };
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(tokenResponse),
                Encoding.UTF8,
                "application/json"),
        };
    }

    protected static HttpResponseMessage BuildAuthNullBodyResponse()
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        };
    }

    protected static HttpResponseMessage BuildAuthFailureResponse()
    {
        return new HttpResponseMessage(HttpStatusCode.Unauthorized);
    }

    protected static HttpResponseMessage BuildCreateMeetingSuccessResponse(
        long meetingId = 123456789L,
        string joinUrl = "https://zoom.us/j/123456789",
        string startUrl = "https://zoom.us/s/123456789",
        string password = "P@ssw0rd",
        string registrationUrl = "https://zoom.us/meeting/register/abc")
    {
        ZoomCreateMeetingResponse meetingResponse = new()
        {
            Id = meetingId,
            JoinUrl = joinUrl,
            StartUrl = startUrl,
            Password = password,
            RegistrationUrl = registrationUrl,
        };
        return new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(meetingResponse),
                Encoding.UTF8,
                "application/json"),
        };
    }

    protected static HttpResponseMessage BuildCreateMeetingHttpOkResponse(
        long meetingId = 123456789L,
        string joinUrl = "https://zoom.us/j/123456789",
        string startUrl = "https://zoom.us/s/123456789",
        string password = "P@ssw0rd",
        string registrationUrl = "https://zoom.us/meeting/register/abc")
    {
        ZoomCreateMeetingResponse meetingResponse = new()
        {
            Id = meetingId,
            JoinUrl = joinUrl,
            StartUrl = startUrl,
            Password = password,
            RegistrationUrl = registrationUrl,
        };
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(meetingResponse),
                Encoding.UTF8,
                "application/json"),
        };
    }

    protected static HttpResponseMessage BuildCreateMeetingNullBodyResponse()
    {
        return new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        };
    }

    protected static HttpResponseMessage BuildCreateMeetingFailureResponse()
    {
        return new HttpResponseMessage(HttpStatusCode.BadRequest);
    }

    protected static HttpResponseMessage BuildDeleteMeetingSuccessResponse()
    {
        return new HttpResponseMessage(HttpStatusCode.NoContent);
    }

    protected static HttpResponseMessage BuildDeleteMeetingFailureResponse()
    {
        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }

    protected static HttpResponseMessage BuildRegisterParticipantSuccessResponse(
        long meetingId = 123456789L,
        string registrantId = "reg-abc-123",
        string joinUrl = "https://zoom.us/j/123456789?tk=abc")
    {
        ZoomCreateRegistrantResposne registrantResponse = new()
        {
            MeetingId = meetingId,
            RegistrantId = registrantId,
            JoinUrl = joinUrl,
        };
        return new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(registrantResponse),
                Encoding.UTF8,
                "application/json"),
        };
    }

    protected static HttpResponseMessage BuildRegisterParticipantNullBodyResponse()
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        };
    }

    protected static HttpResponseMessage BuildRegisterParticipantFailureResponse()
    {
        return new HttpResponseMessage(HttpStatusCode.BadRequest);
    }

    protected static HttpResponseMessage BuildUnRegisterParticipantSuccessResponse()
    {
        return new HttpResponseMessage(HttpStatusCode.NoContent);
    }

    protected static HttpResponseMessage BuildUnRegisterParticipantFailureResponse()
    {
        return new HttpResponseMessage(HttpStatusCode.BadRequest);
    }

    protected static ZoomCreateMeetingRequest BuildValidCreateMeetingRequest()
    {
        return new ZoomCreateMeetingRequest
        {
            Topic = "Test Meeting",
            DurationInMinutes = 60,
        };
    }

    protected static ZoomRegisterRegistrantRequest BuildValidRegisterRequest()
    {
        return new ZoomRegisterRegistrantRequest
        {
            MeetingID = 123456789L,
            Email = "attendee@example.com",
            FirstName = "John",
            LastName = "Doe",
        };
    }

    protected static ZoomUnRegisterRegistrantRequest BuildValidUnRegisterRequest()
    {
        return new ZoomUnRegisterRegistrantRequest
        {
            MeetingID = 123456789L,
            RegistrantID = "reg-abc-123",
        };
    }
}



