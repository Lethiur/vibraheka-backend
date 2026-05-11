namespace Infrastructure.Rest.Client.Zoom.Models;

public sealed class ZoomUnRegisterRegistrantRequest
{
    public long MeetingID { get; set; }
    public string RegistrantID { get; set; } = string.Empty;
}
