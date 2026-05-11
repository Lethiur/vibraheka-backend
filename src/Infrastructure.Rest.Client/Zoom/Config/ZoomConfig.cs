using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Rest.Client.Zoom.Config;

public class ZoomConfig()
{
    [Required]
    public string AccountID { get; set; } = string.Empty;

    [Required]
    public string ClientID { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    [Required]
    public string HostEmail { get; set; } = string.Empty;
}
