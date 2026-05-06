using System.Text.Json.Serialization;
using Infrastructure.Rest.Client.Zoom.Enums;

namespace Infrastructure.Rest.Client.Zoom.Models;

public sealed class ZoomCreateMeetingResponse
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("host_id")]
    public string HostId { get; set; } = string.Empty;

    [JsonPropertyName("host_email")]
    public string HostEmail { get; set; } = string.Empty;

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public MeetingType Type { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("start_time")]
    public DateTimeOffset StartTimeUtc { get; set; }

    [JsonPropertyName("duration")]
    public int DurationInMinutes { get; set; }

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAtUtc { get; set; }

    [JsonPropertyName("start_url")]
    public string StartUrl { get; set; } = string.Empty;

    [JsonPropertyName("join_url")]
    public string JoinUrl { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("h323_password")]
    public string H323Password { get; set; } = string.Empty;

    [JsonPropertyName("pstn_password")]
    public string PstnPassword { get; set; } = string.Empty;

    [JsonPropertyName("encrypted_password")]
    public string EncryptedPassword { get; set; } = string.Empty;

    [JsonPropertyName("settings")]
    public ZoomCreateMeetingResponseSettings Settings { get; set; } =
        new ZoomCreateMeetingResponseSettings();

    [JsonPropertyName("registration_url")]
    public string RegistrationUrl { get; set; } = string.Empty;
}
