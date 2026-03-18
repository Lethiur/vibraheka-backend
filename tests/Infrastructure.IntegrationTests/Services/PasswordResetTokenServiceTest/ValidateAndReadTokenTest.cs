using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.PasswordResetTokenServiceTest;

[TestFixture]
public class ValidateAndReadTokenTest : GenericPasswordResetTokenServiceIntegrationTest
{
    [Test]
    public void ShouldDecodeTokenWhenPayloadIsValid()
    {
        // Given: un token cifrado valido con expiracion futura.
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(20);
        string token = BuildEncryptedToken(
            "integration@test.com",
            "654321",
            Guid.NewGuid().ToString("N"),
            expiresAt,
            _configuration.PasswordResetTokenSecret);

        // When: se valida y decodifica el token.
        Result<PasswordResetTokenData> result = _service.ValidateAndReadToken(token);

        // Then: debe devolverse el payload correctamente.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Email, Is.EqualTo("integration@test.com"));
        Assert.That(result.Value.CognitoCode, Is.EqualTo("654321"));
        Assert.That(result.Value.ExpiresAt.ToUnixTimeSeconds(), Is.EqualTo(expiresAt.ToUnixTimeSeconds()));
    }

    [Test]
    public void ShouldReturnExpiredErrorWhenTokenHasPastExpiration()
    {
        // Given: un token con expiracion ya vencida.
        string token = BuildEncryptedToken(
            "integration@test.com",
            "654321",
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow.AddMinutes(-5),
            _configuration.PasswordResetTokenSecret);

        // When: se valida el token vencido.
        Result<PasswordResetTokenData> result = _service.ValidateAndReadToken(token);

        // Then: debe devolverse PasswordResetTokenExpired.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.PasswordResetTokenExpired));
    }

    [Test]
    public void ShouldReturnInvalidTokenWhenFormatIsMalformed()
    {
        // Given: un token mal formado.
        const string malformedToken = "invalid-token";

        // When: se valida el token mal formado.
        Result<PasswordResetTokenData> result = _service.ValidateAndReadToken(malformedToken);

        // Then: debe devolverse InvalidPasswordResetToken.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPasswordResetToken));
    }

    [Test]
    public void ShouldReturnInvalidTokenWhenTokenIsEmpty()
    {
        // Given: un token vacio.
        const string token = "";

        // When: se valida el token vacio.
        Result<PasswordResetTokenData> result = _service.ValidateAndReadToken(token);

        // Then: debe devolverse InvalidPasswordResetToken.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPasswordResetToken));
    }

    [Test]
    public void ShouldReturnUnexpectedErrorWhenSecretIsMissing()
    {
        // Given: un servicio creado con secret vacio.
        AWSConfig invalidConfig = new()
        {
            EmailTemplatesBucketName = _configuration.EmailTemplatesBucketName,
            UserCodesTable = _configuration.UserCodesTable,
            EmailTemplatesTable = _configuration.EmailTemplatesTable,
            UsersTable = _configuration.UsersTable,
#if DEBUG
            CodesTable = _configuration.CodesTable,
#endif
            ClientId = _configuration.ClientId,
            UserPoolId = _configuration.UserPoolId,
            Location = _configuration.Location,
            Profile = _configuration.Profile,
            PasswordResetTokenSecret = string.Empty,
            ActionLogTable = _configuration.ActionLogTable,
            SubscriptionTable = _configuration.SubscriptionTable,
            SubscriptionUserIdIndex = _configuration.SubscriptionUserIdIndex,
            SettingsNameSpace = _configuration.SettingsNameSpace
        };
        PasswordResetTokenService service = new(invalidConfig, CreateTestLogger<PasswordResetTokenService>());
        string token = BuildEncryptedToken(
            "integration@test.com",
            "654321",
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow.AddMinutes(20),
            _configuration.PasswordResetTokenSecret);

        // When: se valida el token con servicio mal configurado.
        Result<PasswordResetTokenData> result = service.ValidateAndReadToken(token);

        // Then: debe devolverse UnexpectedError.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UnexpectedError));
    }

    [Test]
    public void ShouldReturnInvalidTokenWhenPayloadIsTampered()
    {
        // Given: un token valido cuyo payload es alterado.
        string goodToken = BuildEncryptedToken(
            "integration@test.com",
            "654321",
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow.AddMinutes(20),
            _configuration.PasswordResetTokenSecret);

        string[] parts = goodToken.Split('.', 2);
        byte[] bytes = FromBase64Url(parts[1]);
        bytes[^1] ^= 0x01;
        string tamperedToken = $"v1.{ToBase64Url(bytes)}";

        // When: se valida el token alterado.
        Result<PasswordResetTokenData> result = _service.ValidateAndReadToken(tamperedToken);

        // Then: debe devolverse InvalidPasswordResetToken.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPasswordResetToken));
    }

    [Test]
    public void ShouldReturnInvalidTokenWhenPayloadHasEmptyRequiredFields()
    {
        // Given: un token con payload invalido (email vacio).
        string token = BuildEncryptedToken(
            "",
            "654321",
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow.AddMinutes(20),
            _configuration.PasswordResetTokenSecret);

        // When: se valida el token con payload incompleto.
        Result<PasswordResetTokenData> result = _service.ValidateAndReadToken(token);

        // Then: debe devolverse InvalidPasswordResetToken.
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

    private static byte[] FromBase64Url(string value)
    {
        string padded = value.Replace('-', '+').Replace('_', '/');
        int mod = padded.Length % 4;
        if (mod > 0)
        {
            padded = padded.PadRight(padded.Length + (4 - mod), '=');
        }

        return Convert.FromBase64String(padded);
    }

    private sealed class PasswordResetPayloadTestModel
    {
        public string Email { get; set; } = string.Empty;
        public string CognitoCode { get; set; } = string.Empty;
        public string TokenId { get; set; } = string.Empty;
        public long ExpiresAtUnix { get; set; }
    }
}
