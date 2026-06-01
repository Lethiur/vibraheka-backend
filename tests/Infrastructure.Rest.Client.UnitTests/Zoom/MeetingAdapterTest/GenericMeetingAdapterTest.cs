using System.Net;
using System.Text;
using System.Text.Json;
using Infrastructure.Rest.Client.Zoom;
using Infrastructure.Rest.Client.Zoom.Adapters;
using Infrastructure.Rest.Client.Zoom.Config;
using Infrastructure.Rest.Client.Zoom.Mappers;
using Infrastructure.Rest.Client.Zoom.Models;
using Infrastructure.Rest.Client.Zoom.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using VibraHeka.Domain.Events.Models;
using VibraHeka.Infrastructure.Rest.Client.UnitTests.Helpers;

namespace VibraHeka.Infrastructure.Rest.Client.UnitTests.Zoom.MeetingAdapterTest;

public abstract class GenericMeetingAdapterTest
{
    protected FakeHttpMessageHandler FakeHandler = default!;
    protected ZoomApiClient ApiClient = default!;
    protected ZoomAuthService AuthService = default!;
    protected MeetingAdapter Adapter = default!;

    protected static ZoomConfig ValidConfig => new()
    {
        AccountID = "test-account-id",
        ClientID = "test-client-id",
        ClientSecret = "test-client-secret",
        HostEmail = "host@example.com",
    };

    [SetUp]
    public virtual void SetUp()
    {
        FakeHandler = new FakeHttpMessageHandler();
        HttpClient httpClient = new(FakeHandler);
        ApiClient = new ZoomApiClient(NullLogger<ZoomApiClient>.Instance, httpClient);
        IOptions<ZoomConfig> configOptions = Options.Create(ValidConfig);
        AuthService = new ZoomAuthService(ApiClient, configOptions);
        ZoomMeetingMapper mapper = new();
        Adapter = new MeetingAdapter(
            AuthService,
            ApiClient,
            configOptions,
            mapper,
            NullLogger<MeetingAdapter>.Instance);
    }

    [TearDown]
    public virtual void TearDown()
    {
        FakeHandler.Dispose();
    }

    /// <summary>
    /// Enqueues a successful auth token HTTP response so the auth step passes.
    /// </summary>
    protected void EnqueueAuthSuccess(string accessToken = "test-token")
    {
        ZoomAuthTokenResponse tokenResponse = new()
        {
            AccessToken = accessToken,
            ExpiresIn = 3600,
        };
        FakeHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(tokenResponse),
                Encoding.UTF8,
                "application/json"),
        });
    }

    /// <summary>
    /// Enqueues a 401 auth failure response so the auth step fails.
    /// </summary>
    protected void EnqueueAuthFailure()
    {
        FakeHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.Unauthorized));
    }

    protected void EnqueueCreateMeetingSuccess(
        long meetingId = 123456789L,
        string joinUrl = "https://zoom.us/j/123456789",
        string startUrl = "https://zoom.us/s/123456789",
        string password = "P@ssw0rd",
        string registrationUrl = "https://zoom.us/meeting/register/abc")
    {
        ZoomCreateMeetingResponse response = new()
        {
            Id = meetingId,
            JoinUrl = joinUrl,
            StartUrl = startUrl,
            Password = password,
            RegistrationUrl = registrationUrl,
        };
        FakeHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(response),
                Encoding.UTF8,
                "application/json"),
        });
    }

    protected void EnqueueCreateMeetingFailure()
    {
        FakeHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.BadRequest));
    }

    protected void EnqueueDeleteMeetingSuccess()
    {
        FakeHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.NoContent));
    }

    protected void EnqueueDeleteMeetingFailure()
    {
        FakeHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.NotFound));
    }

    protected void EnqueueRegisterParticipantSuccess(
        long meetingId = 123456789L,
        string registrantId = "reg-abc-123",
        string joinUrl = "https://zoom.us/j/123456789?tk=abc")
    {
        ZoomCreateRegistrantResposne response = new()
        {
            MeetingId = meetingId,
            RegistrantId = registrantId,
            JoinUrl = joinUrl,
        };
        FakeHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(response),
                Encoding.UTF8,
                "application/json"),
        });
    }

    protected void EnqueueRegisterParticipantFailure()
    {
        FakeHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.BadRequest));
    }

    protected void EnqueueUnRegisterParticipantSuccess()
    {
        FakeHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.NoContent));
    }

    protected void EnqueueUnRegisterParticipantFailure()
    {
        FakeHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.BadRequest));
    }

    protected static CreateEventModel BuildValidCreateEventModel()
    {
        return new CreateEventModel
        {
            Name = "Test Zoom Meeting",
            StartDate = DateTimeOffset.UtcNow.AddDays(1),
            Duration = 60,
            EventPassword = "P@ssw0rd",
            EventTimezone = "Europe/Madrid",
        };
    }

    protected static RegisterAttendeeModel BuildValidRegisterAttendeeModel()
    {
        return new RegisterAttendeeModel
        {
            EventID = 123456789L,
            RegistrantEmail = "attendee@example.com",
            RegistrantName = "John",
            RegistrantLastName = "Doe",
        };
    }

    protected static UnRegisterAttendeeModel BuildValidUnRegisterAttendeeModel()
    {
        return new UnRegisterAttendeeModel
        {
            EventID = 123456789,
            RegistrantID = "reg-abc-123",
        };
    }
}


