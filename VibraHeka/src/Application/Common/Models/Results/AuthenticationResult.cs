namespace VibraHeka.Application.Common.Models.Results;

/// <summary>
/// Represents the result of an authentication operation, providing the user ID, access token, and refresh token.
/// </summary>
public record AuthenticationResult(string UserID, string AccessToken, string RefreshToken);
