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
public class VerificationAcceptanceTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    [DisplayName("Should verify a user")]
    public async Task ShouldVerifyAUser()
    {
        // Given: Some registered user
        Faker faker = new();
        string email = faker.Internet.Email();
        const string password = "Password123@";

        await RegisterUser(email, password);

        // And: The verification code and its encrypted token
        VerificationCodeEntity verificationCode = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));

        // When: The user verifies their account
        HttpResponseMessage response = await Client.PatchAsJsonAsync("api/v1/auth/verify",new VerifyUserRequest() {EncryptedCode = verificationCode.Code});
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent), "The response should be 204 No Content");
    }

    [Test]
    [DisplayName("Should fail verification with non-existent user")]
    public async Task ShouldFailVerificationWithNonExistentUser()
    {
        // Given: A valid token pointing to a non-existent user
        string encryptedToken = CreateEncryptedToken("nonexistent@example.com", "123456");
        VerifyUserCommand command = new(encryptedToken);

        // When
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/auth/verify", command);
        
        // Then: Should return 404 Not Found
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound),
            "Should return 404 Not Found when trying to verify a non-existent user");
    }

    [Test]
    [DisplayName("Should fail when the token is reused")]
    public async Task ShouldFailWhenTheTokenIsReused()
    {
        // Given: A registered user
        Faker faker = new();
        string email = faker.Internet.Email();
        await RegisterUser(email, ThePassword);
        VerificationCodeEntity waitForVerificationCode = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));

        // And: An encrypted token with a wrong code
        VerifyUserCommand command = new(waitForVerificationCode.Code);

        // And: The token is used once 
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/auth/verify", command);
        response.EnsureSuccessStatusCode();

        // When: The same token is reused
        response = await Client.PatchAsJsonAsync("/api/v1/auth/confirm", command);
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.PasswordResetTokenAlreadyUsed));
    }

    [Test]
    [DisplayName("Should fail verification with wrong code")]
    public async Task ShouldFailVerificationWithWrongCode()
    {
        // Given: A registered user
        Faker faker = new();
        string email = faker.Internet.Email();
        await RegisterUser(email, "Password123@");
        await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));

        // And: An encrypted token with a wrong code
        string encryptedToken = CreateEncryptedToken(email, "999999");
        VerifyUserCommand command = new(encryptedToken);

        // When
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/auth/verify", command);
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.WrongVerificationCode));
    }

    [Test]
    [DisplayName("Should return BadRequest when token was already used")]
    public async Task ShouldReturnBadRequestWhenTokenWasAlreadyUsed()
    {
        // Given: A registered user with a valid encrypted token
        Faker faker = new();
        string email = faker.Internet.Email();
        await RegisterUser(email, "Password123@");
        VerificationCodeEntity verificationCode = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));
        string encryptedToken = verificationCode.Code;

        // And: The token is used once successfully
        HttpResponseMessage firstResponse = await Client.PatchAsJsonAsync("/api/v1/auth/verify", new VerifyUserCommand(encryptedToken));
        firstResponse.EnsureSuccessStatusCode();

        // When: The same token is reused
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/auth/verify", new VerifyUserCommand(encryptedToken));
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();

        // Then: Replay protection blocks the second attempt
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.PasswordResetTokenAlreadyUsed));
    }

    // === VALIDATION TESTS ===
    [TestCase("", UserErrors.InvalidPasswordResetToken, TestName = "Empty encrypted code")]
    [TestCase(null, UserErrors.InvalidPasswordResetToken, TestName = "Null encrypted code")]
    [TestCase("   ", UserErrors.InvalidPasswordResetToken, TestName = "Whitespace encrypted code")]
    [DisplayName("Should return BadRequest when encrypted code is empty or null")]
    public async Task ShouldNotAllowVerificationWithEmptyOrNullEncryptedCode(string encryptedCode, string expectedErrorCode)
    {
        // Given
        VerifyUserCommand command = new(encryptedCode);

        // When
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/auth/verify", command);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(expectedErrorCode));
    }

    // === INVALID TOKEN FORMAT TESTS ===
    [TestCase("not-a-valid-token", TestName = "No v1 prefix")]
    [TestCase("v1.!!!invalid-base64!!!", TestName = "Invalid base64 payload")]
    [TestCase("v2.somevalue", TestName = "Wrong version prefix")]
    [TestCase("justplaintext", TestName = "Plain text without prefix")]
    [DisplayName("Should return BadRequest when token format is invalid")]
    public async Task ShouldReturnBadRequestWhenTokenFormatIsInvalid(string encryptedCode)
    {
        // Given
        VerifyUserCommand command = new(encryptedCode);

        // When
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/auth/verify", command);
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.InvalidPasswordResetToken));
    }

    [Test]
    [DisplayName("Should return BadRequest when token is expired")]
    public async Task ShouldReturnBadRequestWhenTokenIsExpired()
    {
        // Given: An already-expired encrypted token
        string expiredToken = CreateEncryptedToken("user@test.com", "123456", DateTimeOffset.UtcNow.AddMinutes(-20));
        VerifyUserCommand command = new(expiredToken);

        // When
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/auth/verify", command);
        BadRequestResponse responseEntity = await response.ParseContentAsync<BadRequestResponse>();

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.PasswordResetTokenExpired));
    }
}
