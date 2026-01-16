using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Models.Results;

public class AuthenticationResult
{
    public string UserID { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } = UserRole.User;

    // Constructor vacío para permitir la creación en dos pasos
    public AuthenticationResult() { }

    // Constructor opcional por si quieres inicializarla de golpe en algún sitio
    public AuthenticationResult(string userId, string accessToken, string refreshToken)
    {
        UserID = userId;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }
}
