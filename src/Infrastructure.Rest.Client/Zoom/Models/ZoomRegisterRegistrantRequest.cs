using System.Text.Json.Serialization;

namespace Infrastructure.Rest.Client.Zoom.Models;

public sealed class ZoomRegisterRegistrantRequest
{
    [JsonIgnore]
    public long MeetingID { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("auto_approve")]
    public bool AutoApprove { get; set; } = true;
}
