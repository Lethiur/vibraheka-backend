using Infrastructure.Rest.Client.IntegrationTests.Helpers;
using Infrastructure.Rest.Client.Zoom;
using Infrastructure.Rest.Client.Zoom.Models;

namespace Infrastructure.Rest.Client.IntegrationTests.Zoom.ZoomApiClientTest;

/// <summary>
/// Base class for ZoomApiClient integration tests using deterministic HTTP stubs.
/// </summary>
public abstract class GenericZoomApiClientTest : TestBase
{
    protected FakeHttpMessageHandler FakeHandler = default!;
    protected ZoomApiClient ApiClient = default!;

    [SetUp]
    public virtual void SetUp()
    {
        FakeHandler = new FakeHttpMessageHandler();
        ApiClient = new ZoomApiClient(CreateTestLogger<ZoomApiClient>(), new HttpClient(FakeHandler));
    }

    [TearDown]
    public virtual void TearDown()
    {
        FakeHandler.Dispose();
    }

    protected static string BuildValidAuthTokenJson(int expiresIn = 3600) =>
        $$$"""{"access_token":"stub-access-token-xyz","token_type":"bearer","expires_in":{{{expiresIn}}}}""";

    protected static string BuildValidCreateMeetingResponseJson() =>
        """
        {
          "id": 987654321,
          "uuid": "abcd1234==",
          "host_email": "host@example.com",
          "topic": "Stub Integration Meeting",
          "type": 2,
          "status": "waiting",
          "start_time": "2025-01-01T10:00:00Z",
          "duration": 30,
          "timezone": "Europe/Madrid",
          "created_at": "2024-06-01T00:00:00Z",
          "start_url": "https://zoom.us/s/987654321",
          "join_url": "https://zoom.us/j/987654321",
          "password": "stubpass",
          "h323_password": "",
          "pstn_password": "",
          "encrypted_password": "",
          "settings": {},
          "registration_url": "https://zoom.us/meeting/register/987654321"
        }
        """;

    protected static string BuildValidRegistrantResponseJson() =>
        """
        {
          "id": 987654321,
          "registrant_id": "reg-stub-001",
          "join_url": "https://zoom.us/j/987654321?tk=abc",
          "topic": "Stub Integration Meeting",
          "start_time": "2025-01-01T10:00:00Z"
        }
        """;

    protected static ZoomCreateMeetingRequest BuildCreateMeetingRequest() =>
        new ZoomCreateMeetingRequest
        {
            Topic = "Stub Test Meeting",
            StartTimeUtc = DateTimeOffset.UtcNow.AddDays(1),
            DurationInMinutes = 30,
            Timezone = "Europe/Madrid",
            Password = "T3stP@ss",
        };

    protected static ZoomRegisterRegistrantRequest BuildRegisterRegistrantRequest(long meetingId = 987654321L) =>
        new ZoomRegisterRegistrantRequest
        {
            MeetingID = meetingId,
            Email = "attendee@example.com",
            FirstName = "Test",
            LastName = "Attendee",
            AutoApprove = true,
        };

    protected static ZoomUnRegisterRegistrantRequest BuildUnRegisterRegistrantRequest(
        long meetingId = 987654321L,
        string registrantId = "reg-stub-001") =>
        new ZoomUnRegisterRegistrantRequest
        {
            MeetingID = meetingId,
            RegistrantID = registrantId,
        };
}

