namespace VibraHeka.Domain.Models.Results;

/// <summary>
/// Represents the decoded payload contained in a password reset token.
/// </summary>
/// <param name="Email">Email associated with the recovery operation.</param>
/// <param name="CognitoCode">Cognito recovery code extracted from the token.</param>
/// <param name="TokenId">Unique token identifier used for replay protection.</param>
/// <param name="ExpiresAt">Expiration timestamp of the token.</param>
public record PasswordResetTokenData(
    string Email,
    string CognitoCode,
    string TokenId,
    DateTimeOffset ExpiresAt);
