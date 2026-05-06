using System.Text.Json.Serialization;
using Infrastructure.Rest.Client.Zoom.Enums;

namespace Infrastructure.Rest.Client.Zoom.Models;

public sealed class ZoomCreateMeetingResponseSettings
{
    [JsonPropertyName("host_video")]
    public bool HostVideoEnabled { get; set; }

    [JsonPropertyName("participant_video")]
    public bool ParticipantVideoEnabled { get; set; }

    [JsonPropertyName("join_before_host")]
    public bool JoinBeforeHost { get; set; }

    [JsonPropertyName("mute_upon_entry")]
    public bool MuteUponEntry { get; set; }

    [JsonPropertyName("waiting_room")]
    public bool WaitingRoomEnabled { get; set; }

    [JsonPropertyName("approval_type")]
    public MeetingApprovalType ApprovalType { get; set; }

    [JsonPropertyName("registration_type")]
    public MeetingRegistrationType RegistrationType { get; set; }

    [JsonPropertyName("registrants_email_notification")]
    public bool RegistrantsEmailNotification { get; set; }
}
