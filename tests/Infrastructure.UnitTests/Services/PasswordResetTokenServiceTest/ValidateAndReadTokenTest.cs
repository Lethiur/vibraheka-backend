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
        Service = new(Config, LoggerMock.Object);
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


    [Test]
    [TestCase(null, null, null)]
    [TestCase("", "ok", "ok")] // Email vacío
    [TestCase("   ", "ok", "ok")] // Email whitespace
    [TestCase("a@b.com", "", "ok")] // CognitoCode vacío
    [TestCase("a@b.com", "   ", "ok")] // CognitoCode whitespace
    [TestCase("a@b.com", "ok", "")] // TokenId vacío
    [TestCase("a@b.com", "ok", "   ")] // TokenId whitespace
    public void ShouldFailWhenFieldsAreEmpty(string? email, string? cognitoCode, string? tokenId)
    {
        // Given: a valid encrypted token
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(20);
        string token = BuildEncryptedToken(
            email!,
            cognitoCode!,
            tokenId!,
            expiresAt,
            Config.PasswordResetTokenSecret);

        // When: validating and decoding the token
        Result<PasswordResetTokenData> result = Service.ValidateAndReadToken(token);

        // Then: validation fails with invalid token error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPasswordResetToken));
    }

    [Test]
    [TestCase("v1.not@base64url")]
    [TestCase("v1.***")]
    [TestCase("v1.a")] // a veces falla por longitud
    [TestCase("v1.ab$cd")]
    public void ShouldHandleInvalidEncryptedTokenWithNonBase64(string malformationToken)
    {
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(20);
        string token = BuildEncryptedToken(
            "asdfasd@a.com",
            "12456",
            "asdfasdfasd",
            expiresAt,
            Config.PasswordResetTokenSecret, malformationToken);

        // When: validating and decoding the token
        Result<PasswordResetTokenData> result = Service.ValidateAndReadToken(token);

        // Then: validation fails with invalid token error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPasswordResetToken));
    }
    
    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(28)] // justo el límite: también debe fallar (<=)
    public void ShouldHandleShortTokens(int len)
    {
        // Given: Wrong payload
        byte[] shortPayload = new byte[len];
        string token = $"v1.{ToBase64Url(shortPayload)}";

        // When: Service is invoked
        Result<PasswordResetTokenData> result = Service.ValidateAndReadToken(token);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPasswordResetToken));
    }
    
    [Test]
    public void ShouldFailIfTamperedToken()
    {
        // Arrange
        const string secret = "my-secret";
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(10);

        string goodToken = BuildEncryptedToken(
            email: "a@b.com",
            cognitoCode: "123",
            tokenId: "tok-1",
            expiresAt: expiresAt,
            secret: secret);

        // Split "v1.{payload}"
        string[] parts = goodToken.Split('.', 2);
        byte[] bytes = FromBase64Url(parts[1]);

        // Corrompe 1 byte del ciphertext/tag/nonce (da igual cuál; con tocar uno basta)
        bytes[^1] ^= 0x01; // flip last bit

        string tamperedToken = $"v1.{ToBase64Url(bytes)}";

        // Act
        Result<PasswordResetTokenData> result = Service.ValidateAndReadToken(tamperedToken);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPasswordResetToken));
    }
}
