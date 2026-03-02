using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.ConfirmPasswordRecovery;
using VibraHeka.Application.Users.Commands.StartPasswordRecovery;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Auth;

[TestFixture]
public class ForgotPasswordAcceptanceTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldReturnOkWhenStartingPasswordRecoveryForNonExistingUser()
    {
        // Given: a valid email that does not exist in Cognito
        StartPasswordRecoveryCommand command = new($"{Guid.NewGuid():N}@example.com");

        // When: the forgot-password endpoint is called
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/forgot-password", command);

        // Then: endpoint returns success to avoid user enumeration
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        ResponseEntity responseEntity = await response.GetAsResponseEntityAndContentAs<string>();
        Assert.That(responseEntity.Success, Is.True);
        Assert.That(responseEntity.Content, Is.EqualTo("If the account exists, a recovery email has been sent."));
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase("   ")]
    [TestCase("invalid-email")]
    public async Task ShouldReturnBadRequestWhenStartingPasswordRecoveryWithInvalidEmail(string? email)
    {
        // Given: an invalid forgot-password command
        StartPasswordRecoveryCommand command = new(email!);

        // When: the forgot-password endpoint is called
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/forgot-password", command);

        // Then: validation error is returned
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.InvalidEmail));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenConfirmingPasswordRecoveryWithMalformedToken()
    {
        // Given: a malformed encrypted token with valid password fields
        ConfirmPasswordRecoveryCommand command = new("invalid-token", "Password123@", "Password123@");

        // When: the forgot-password confirm endpoint is called
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/forgot-password/confirm", command);

        // Then: token validation fails with InvalidPasswordResetToken
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.InvalidPasswordResetToken));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenConfirmingPasswordRecoveryWithEmptyToken()
    {
        // Given: an empty token and valid password fields
        ConfirmPasswordRecoveryCommand command = new(string.Empty, "Password123@", "Password123@");

        // When: the forgot-password confirm endpoint is called
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/forgot-password/confirm", command);

        // Then: validator rejects the request with InvalidPasswordResetToken
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.InvalidPasswordResetToken));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenConfirmingPasswordRecoveryWithDifferentPasswords()
    {
        // Given: a command where confirmation password does not match
        ConfirmPasswordRecoveryCommand command = new("v1.invalid", "Password123@", "Password456@");

        // When: the forgot-password confirm endpoint is called
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/forgot-password/confirm", command);

        // Then: validator returns invalid password error
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.InvalidPassword));
    }
}
