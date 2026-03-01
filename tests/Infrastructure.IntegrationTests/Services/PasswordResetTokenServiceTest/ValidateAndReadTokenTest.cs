using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.PasswordResetTokenServiceTest;

[TestFixture]
public class ValidateAndReadTokenTest : GenericPasswordResetTokenServiceIntegrationTest
{
    [Test]
    public void ShouldDecodeTokenWhenPayloadIsValid()
    {
        // Given: an encrypted token with a valid payload and future expiration
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(20);
        string token = BuildEncryptedToken(
            "integration@test.com",
            "654321",
            Guid.NewGuid().ToString("N"),
            expiresAt,
            _configuration.PasswordResetTokenSecret);

        // When: the token is validated and decoded
        Result<PasswordResetTokenData> result = _service.ValidateAndReadToken(token);

        // Then: the decoded payload is returned successfully
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Email, Is.EqualTo("integration@test.com"));
        Assert.That(result.Value.CognitoCode, Is.EqualTo("654321"));
        Assert.That(result.Value.ExpiresAt.ToUnixTimeSeconds(), Is.EqualTo(expiresAt.ToUnixTimeSeconds()));
    }

    [Test]
    public void ShouldReturnExpiredErrorWhenTokenHasPastExpiration()
    {
        // Given: an encrypted token whose expiration is already in the past
        string token = BuildEncryptedToken(
            "integration@test.com",
            "654321",
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow.AddMinutes(-5),
            _configuration.PasswordResetTokenSecret);

        // When: the token is validated
        Result<PasswordResetTokenData> result = _service.ValidateAndReadToken(token);

        // Then: the service returns the expired-token error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.PasswordResetTokenExpired));
    }

    [Test]
    public void ShouldReturnInvalidTokenWhenFormatIsMalformed()
    {
        // Given: a malformed token string that does not follow expected format
        const string malformedToken = "invalid-token";

        // When: the token is validated
        Result<PasswordResetTokenData> result = _service.ValidateAndReadToken(malformedToken);

        // Then: the service returns invalid-token error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPasswordResetToken));
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
