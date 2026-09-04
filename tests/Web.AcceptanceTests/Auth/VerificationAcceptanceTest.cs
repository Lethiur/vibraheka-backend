using System.ComponentModel;
using System.Net;
using System.Net.Http.Json;
using Bogus;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.VerificationCode;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;

namespace VibraHeka.Web.AcceptanceTests.Auth;

[TestFixture]
public class VerificationAcceptanceTest : GenericAuthAcceptanceTest
{
    [Test]
    [DisplayName("Should verify a user")]
    public async Task ShouldVerifyAUser()
    {
        // Given: Some registered user
        string email = TheFaker.Internet.Email();
        await RegisterUser(email);

        // And: The verification code and its encrypted token
        VerificationCodeEntity verificationCode = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));

        // When: The user verifies their account
        await PerformVerifyRegistration(new VerifyUserRequest { EncryptedCode = verificationCode.Code });
    }

    [Test]
    [DisplayName("Should fail verification with non-existent user")]
    public async Task ShouldFailVerificationWithNonExistentUser()
    {
        // Given: A valid token pointing to a non-existent user
        string encryptedToken = CreateEncryptedToken("nonexistent@example.com", "123456");
        VerifyUserRequest command = new() { EncryptedCode = encryptedToken };

        // When: Endpoint is invoked
        // Then: Should return 404 Not Found
        await PerformCallAndExpectStatusCode(() => InvokeVerifyRegistrationEndpoint(command), HttpStatusCode.NotFound);
    }

    [Test]
    [DisplayName("Should fail when the token is reused")]
    public async Task ShouldFailWhenTheTokenIsReused()
    {
        // Given: A registered user
        string email = TheFaker.Internet.Email();
        await RegisterUser(email);
        VerificationCodeEntity waitForVerificationCode = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));

        // And: An encrypted token with a wrong code
        VerifyUserRequest command = new() { EncryptedCode = waitForVerificationCode.Code };

        // And: The token is used once 
        await PerformVerifyRegistration(command);

        // When: The same token is reused
        // Then: Should return 400 Bad Request with PasswordResetTokenAlreadyUsed error
        await PerformCallAndExpectError(() => InvokeVerifyRegistrationEndpoint(command),
            UserErrors.PasswordResetTokenAlreadyUsed);
    }

    [Test]
    [DisplayName("Should fail verification with wrong code")]
    public async Task ShouldFailVerificationWithWrongCode()
    {
        // Given: A registered user
        string email = TheFaker.Internet.Email();
        await RegisterUser(email);
        await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));

        // And: An encrypted token with a wrong code
        string encryptedToken = CreateEncryptedToken(email, "999999");
        VerifyUserRequest command = new() { EncryptedCode = encryptedToken };

        // When
        await PerformCallAndExpectError(() => InvokeVerifyRegistrationEndpoint(command),
            UserErrors.WrongVerificationCode);
    }

    // === INVALID TOKEN FORMAT TESTS ===
    [TestCase("not-a-valid-token", TestName = "No v1 prefix")]
    [TestCase("v1.!!!invalid-base64!!!", TestName = "Invalid base64 payload")]
    [TestCase("v2.somevalue", TestName = "Wrong version prefix")]
    [TestCase("justplaintext", TestName = "Plain text without prefix")]
    [TestCase("", TestName = "Plain text without prefix")]
    [TestCase("     ", TestName = "Plain text without prefix")]
    [TestCase(null, TestName = "Plain text without prefix")]
    [DisplayName("Should return BadRequest when token format is invalid")]
    public async Task ShouldReturnBadRequestWhenTokenFormatIsInvalid(string encryptedCode)
    {
        await PerformCallAndExpectError(
            () => InvokeVerifyRegistrationEndpoint(new VerifyUserRequest { EncryptedCode = encryptedCode }),
            UserErrors.InvalidPasswordResetToken);
    }

    [Test]
    [DisplayName("Should return BadRequest when token is expired")]
    public async Task ShouldReturnBadRequestWhenTokenIsExpired()
    {
        // Given: An already-expired encrypted token
        string expiredToken = CreateEncryptedToken("user@test.com", "123456", DateTimeOffset.UtcNow.AddMinutes(-20));
        await PerformCallAndExpectError(
            () => InvokeVerifyRegistrationEndpoint(new VerifyUserRequest { EncryptedCode = expiredToken }),
            UserErrors.PasswordResetTokenExpired);
    }
}
