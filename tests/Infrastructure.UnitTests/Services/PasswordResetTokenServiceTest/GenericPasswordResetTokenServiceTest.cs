using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.PasswordResetTokenServiceTest;

public abstract class GenericPasswordResetTokenServiceTest
{
    protected AWSConfig Config;
    protected Mock<ILogger<PasswordResetTokenService>> LoggerMock;
    protected PasswordResetTokenService Service;

    [SetUp]
    public void SetUp()
    {
        Config = new AWSConfig
        {
            PasswordResetTokenSecret = "super-secret-for-tests"
        };
        LoggerMock = new Mock<ILogger<PasswordResetTokenService>>();
        Service = new PasswordResetTokenService(Config, LoggerMock.Object);
    }
    
    protected static string BuildEncryptedToken(
        string email,
        string cognitoCode,
        string tokenId,
        DateTimeOffset expiresAt,
        string secret,
        string malformationToken = "")
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
        return $"v1.{malformationToken}{ToBase64Url(payload)}";
    }

    protected static string ToBase64Url(byte[] bytes)
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
    
    protected byte[] FromBase64Url(string s)
    {
        s = s.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
            case 0: break;
            default: throw new FormatException("Invalid base64url length");
        }
        return Convert.FromBase64String(s);
    }
}
