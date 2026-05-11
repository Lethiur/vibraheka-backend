using System.Text.Json.Serialization;

namespace Infrastructure.Rest.Client.Zoom.Models;

public class ZoomCreateRegistrantResposne
{
    [JsonPropertyName("id")]
    public long MeetingId { get; set; }

    [JsonPropertyName("registrant_id")]
    public string RegistrantId { get; set; } = string.Empty;

    [JsonPropertyName("join_url")]
    public string JoinUrl { get; set; } = string.Empty;

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    [JsonPropertyName("start_time")]
    public DateTimeOffset StartTimeUtc { get; set; }
}
