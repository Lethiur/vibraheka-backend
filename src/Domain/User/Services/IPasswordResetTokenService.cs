using CSharpFunctionalExtensions;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Domain.Common.Interfaces.User;

/// <summary>
/// Provides operations to validate and decode password reset tokens.
/// </summary>
public interface IPasswordResetTokenService
{
    /// <summary>
    /// Validates and decodes an encrypted password reset token.
    /// </summary>
    /// <param name="token">Encrypted token received from the client.</param>
    /// <returns>Decoded token data or failure when token is invalid/expired.</returns>
    Result<PasswordResetTokenData> ValidateAndReadToken(string token);
}
