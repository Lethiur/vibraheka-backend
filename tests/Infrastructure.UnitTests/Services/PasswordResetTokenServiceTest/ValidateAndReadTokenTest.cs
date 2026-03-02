using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Infrastructure.UnitTests.Services.PasswordResetTokenServiceTest;

[TestFixture]
public class ValidateAndReadTokenTest : GenericPasswordResetTokenServiceTest
{
    [Test]
    public void ShouldReturnFailureWhenTokenIsEmpty()
    {
        // Given: an empty token
        const string token = "";

        // When: validating and decoding the token
        Result<PasswordResetTokenData> result = Service.ValidateAndReadToken(token);

        // Then: validation fails with invalid token error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPasswordResetToken));
    }

    [Test]
    public void ShouldReturnFailureWhenSecretIsMissing()
    {
        // Given: a valid formatted token but missing service secret
        Config.PasswordResetTokenSecret = string.Empty;
        Service = new Infrastructure.Services.PasswordResetTokenService(Config, LoggerMock.Object);
        string token = BuildEncryptedToken(
            "user@test.com",
            "123456",
            "token-123",
            DateTimeOffset.UtcNow.AddMinutes(10),
            "any-secret");

        // When: validating and decoding the token
        Result<PasswordResetTokenData> result = Service.ValidateAndReadToken(token);

        // Then: validation fails with unexpected error from invalid config
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UnexpectedError));
    }

    [Test]
    public void ShouldReturnFailureWhenTokenFormatIsInvalid()
    {
        // Given: a malformed token without expected version prefix
        const string token = "invalid-format";

        // When: validating and decoding the token
        Result<PasswordResetTokenData> result = Service.ValidateAndReadToken(token);

        // Then: validation fails with invalid token error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPasswordResetToken));
    }

    [Test]
    public void ShouldReturnFailureWhenTokenIsExpired()
    {
        // Given: an encrypted token with expiration in the past
        string token = BuildEncryptedToken(
            "user@test.com",
            "123456",
            "token-123",
            DateTimeOffset.UtcNow.AddMinutes(-5),
            Config.PasswordResetTokenSecret);

        // When: validating and decoding the token
        Result<PasswordResetTokenData> result = Service.ValidateAndReadToken(token);

        // Then: validation fails with expired token error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.PasswordResetTokenExpired));
    }

    [Test]
    public void ShouldReturnDecodedPayloadWhenTokenIsValid()
    {
        // Given: a valid encrypted token
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(20);
        string token = BuildEncryptedToken(
            "user@test.com",
            "123456",
            "token-123",
            expiresAt,
            Config.PasswordResetTokenSecret);

        // When: validating and decoding the token
        Result<PasswordResetTokenData> result = Service.ValidateAndReadToken(token);

        // Then: payload is decoded and returned successfully
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Email, Is.EqualTo("user@test.com"));
        Assert.That(result.Value.CognitoCode, Is.EqualTo("123456"));
        Assert.That(result.Value.TokenId, Is.EqualTo("token-123"));
        Assert.That(result.Value.ExpiresAt.ToUnixTimeSeconds(), Is.EqualTo(expiresAt.ToUnixTimeSeconds()));
    }

    private static string BuildEncryptedToken(
        string email,
        string cognitoCode,
        string tokenId,
        DateTimeOffset expiresAt,
        string secret)
    {
        byte[] plainText = JsonSerializer.SerializeToUtf8Bytes(new PasswordResetPayloadTestModel
        {
            Email = email,
            CognitoCode = cognitoCode,
            TokenId = tokenId,
            ExpiresAtUnix = expiresAt.ToUnixTimeSeconds()
        });

        byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        byte[] nonce = RandomNumberGenerator.GetBytes(12);
        byte[] cipherText = new byte[plainText.Length];
        byte[] tag = new byte[16];

        using AesGcm aes = new(key, 16);
        aes.Encrypt(nonce, plainText, cipherText, tag);

        byte[] payload = nonce.Concat(tag).Concat(cipherText).ToArray();
        return $"v1.{ToBase64Url(payload)}";
    }

    private static string ToBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private sealed class PasswordResetPayloadTestModel
    {
        public string Email { get; set; } = string.Empty;
        public string CognitoCode { get; set; } = string.Empty;
        public string TokenId { get; set; } = string.Empty;
        public long ExpiresAtUnix { get; set; }
    }
}
