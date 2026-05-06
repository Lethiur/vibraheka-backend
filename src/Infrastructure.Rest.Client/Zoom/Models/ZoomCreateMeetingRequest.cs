using System.Text.Json.Serialization;
using Infrastructure.Rest.Client.Zoom.Enums;

namespace Infrastructure.Rest.Client.Zoom.Models;

public class ZoomCreateMeetingRequest
{
    [JsonPropertyName("type")]
    public MeetingType Type { get; set; } = MeetingType.Scheduled;

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    [JsonPropertyName("start_time")]
    public DateTimeOffset StartTimeUtc { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("duration")]
    public int DurationInMinutes { get; set; } = 60;

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = "Europe/Madrid";

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonIgnore]
    public bool JoinBeforeHost { get; set; } = true;

    [JsonIgnore]
    public bool WaitingRoomEnabled { get; set; } = true;

    [JsonIgnore]
    public bool SendZoomEmail { get; set; } = true;

    
    [JsonPropertyName("settings")]
    public ZoomMeetingSettings Settings =>
        new ZoomMeetingSettings
        {
            JoinBeforeHost = JoinBeforeHost,
            WaitingRoomEnabled = WaitingRoomEnabled,
            SendZoomEmail = SendZoomEmail,
            ApprovalType = MeetingApprovalType.AutomaticallyApprove,
            RegistrationType = MeetingRegistrationType.RegisterForEachOccurence
        };



}
