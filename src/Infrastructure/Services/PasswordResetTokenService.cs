using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.Services;

/// <summary>
/// Service that validates and decrypts password reset tokens.
/// </summary>
public class PasswordResetTokenService(AWSConfig config, ILogger<PasswordResetTokenService> logger) : IPasswordResetTokenService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const string TokenPrefix = "v1";

    /// <summary>
    /// Validates and decodes a password reset token.
    /// </summary>
    /// <param name="token">Encrypted token value.</param>
    /// <returns>Decoded token payload when token is valid and not expired.</returns>
    public Result<PasswordResetTokenData> ValidateAndReadToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogWarning("Password reset token validation failed: token is empty");
            return Result.Failure<PasswordResetTokenData>(UserErrors.InvalidPasswordResetToken);
        }

        Result<byte[]> keyResult = GetKey();
        if (keyResult.IsFailure)
        {
            logger.LogWarning("Password reset token validation failed: secret is invalid");
            return Result.Failure<PasswordResetTokenData>(keyResult.Error);
        }

        string[] parts = token.Split('.', 2);
        if (parts.Length != 2 || !string.Equals(parts[0], TokenPrefix, StringComparison.Ordinal))
        {
            logger.LogWarning("Password reset token validation failed: invalid format");
            return Result.Failure<PasswordResetTokenData>(UserErrors.InvalidPasswordResetToken);
        }

        byte[] bytes;
        try
        {
            bytes = FromBase64Url(parts[1]);
        }
        catch (FormatException)
        {
            logger.LogWarning("Password reset token validation failed: invalid base64url payload");
            return Result.Failure<PasswordResetTokenData>(UserErrors.InvalidPasswordResetToken);
        }

        if (bytes.Length <= NonceSize + TagSize)
        {
            logger.LogWarning("Password reset token validation failed: payload too short");
            return Result.Failure<PasswordResetTokenData>(UserErrors.InvalidPasswordResetToken);
        }

        byte[] nonce = bytes[..NonceSize];
        byte[] tag = bytes[NonceSize..(NonceSize + TagSize)];
        byte[] cipherText = bytes[(NonceSize + TagSize)..];
        byte[] plainText = new byte[cipherText.Length];

        try
        {
            using AesGcm aes = new(keyResult.Value, TagSize);
            aes.Decrypt(nonce, cipherText, tag, plainText);
        }
        catch (CryptographicException)
        {
            logger.LogWarning("Password reset token validation failed: decryption error");
            return Result.Failure<PasswordResetTokenData>(UserErrors.InvalidPasswordResetToken);
        }

        PasswordResetTokenPayload? payload = JsonSerializer.Deserialize<PasswordResetTokenPayload>(plainText);
        if (payload == null ||
            string.IsNullOrWhiteSpace(payload.Email) ||
            string.IsNullOrWhiteSpace(payload.CognitoCode) ||
            string.IsNullOrWhiteSpace(payload.TokenId))
        {
            logger.LogWarning("Password reset token validation failed: invalid payload");
            return Result.Failure<PasswordResetTokenData>(UserErrors.InvalidPasswordResetToken);
        }

        DateTimeOffset expiresAt = DateTimeOffset.FromUnixTimeSeconds(payload.ExpiresAtUnix);
        if (expiresAt <= DateTimeOffset.UtcNow)
        {
            logger.LogWarning("Password reset token validation failed: token expired for email {Email}", payload.Email);
            return Result.Failure<PasswordResetTokenData>(UserErrors.PasswordResetTokenExpired);
        }

        PasswordResetTokenData tokenData = new(
            payload.Email,
            payload.CognitoCode,
            payload.TokenId,
            expiresAt);

        logger.LogInformation("Password reset token validated for email {Email}", tokenData.Email);
        return Result.Success(tokenData);
    }

    /// <summary>
    /// Resolves and derives the encryption key used for token decryption.
    /// </summary>
    /// <returns>Derived key bytes or failure if configuration is invalid.</returns>
    private Result<byte[]> GetKey()
    {
        if (string.IsNullOrWhiteSpace(config.PasswordResetTokenSecret))
        {
            return Result.Failure<byte[]>(UserErrors.UnexpectedError);
        }

        byte[] rawSecret = Encoding.UTF8.GetBytes(config.PasswordResetTokenSecret);
        return SHA256.HashData(rawSecret);
    }

    /// <summary>
    /// Converts a base64url string into raw bytes.
    /// </summary>
    /// <param name="value">Base64url encoded value.</param>
    /// <returns>Decoded bytes.</returns>
    private static byte[] FromBase64Url(string value)
    {
        string padded = value
            .Replace('-', '+')
            .Replace('_', '/');

        int mod = padded.Length % 4;
        if (mod > 0)
        {
            padded = padded.PadRight(padded.Length + (4 - mod), '=');
        }

        return Convert.FromBase64String(padded);
    }

    /// <summary>
    /// Internal DTO used to deserialize encrypted token payloads.
    /// </summary>
    private sealed class PasswordResetTokenPayload
    {
        public string Email { get; set; } = string.Empty;
        public string CognitoCode { get; set; } = string.Empty;
        public string TokenId { get; set; } = string.Empty;
        public long ExpiresAtUnix { get; set; }
    }
}
