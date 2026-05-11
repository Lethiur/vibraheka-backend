using System.Text.Json.Serialization;

namespace Infrastructure.Rest.Client.Zoom.Models;

public class ZoomAuthTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; } = 0;
}
