using System.Text.Json.Serialization;
using Infrastructure.Rest.Client.Zoom.Enums;

namespace Infrastructure.Rest.Client.Zoom.Models;

public sealed class ZoomMeetingSettings
{
    [JsonPropertyName("join_before_host")]
    public bool JoinBeforeHost { get; set; }

    [JsonPropertyName("waiting_room")]
    public bool WaitingRoomEnabled { get; set; }

    [JsonPropertyName("registrants_email_notification")]
    public bool SendZoomEmail { get; set; }

    [JsonPropertyName("approval_type")]
    public MeetingApprovalType ApprovalType { get; set; }

    [JsonPropertyName("registration_type")]
    public MeetingRegistrationType RegistrationType { get; set; }
}
