namespace VibraHeka.Web.Entities;

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}
