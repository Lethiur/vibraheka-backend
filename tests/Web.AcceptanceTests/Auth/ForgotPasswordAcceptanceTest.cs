using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.ConfirmPasswordRecovery;
using VibraHeka.Application.Users.Commands.StartPasswordRecovery;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.Authentication;

namespace VibraHeka.Web.AcceptanceTests.Auth;

[TestFixture]
public class ForgotPasswordAcceptanceTest : GenericAuthAcceptanceTest
{
    [Test]
    public async Task ShouldReturnOkWhenStartingPasswordRecoveryForNonExistingUser()
    {
        // Given: a valid email that does not exist in Cognito
        ResetPasswordRequest request = new ResetPasswordRequest() { Email = TheFaker.Internet.Email() };

        // When: the forgot-password endpoint is called.
        // Then: endpoint returns success to avoid user enumeration
        await PerformResetPassword(request);
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase("   ")]
    [TestCase("invalid-email")]
    public async Task ShouldReturnBadRequestWhenStartingPasswordRecoveryWithInvalidEmail(string? email)
    {
        // Given: a valid email that does not exist in Cognito
        ResetPasswordRequest request = new ResetPasswordRequest() { Email = email! };
        
        // When: the forgot-password endpoint is called
        await PerformCallAndExpectError(() => InvokeResetPasswordEndpoint(request), UserErrors.InvalidEmail);
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenConfirmingPasswordRecoveryWithMalformedToken()
    {
        // Given: a malformed encrypted token with valid password fields
        ConfirmResetPasswordRequest command = new() { EncryptedToken = "invalid-token", NewPassword = "Password123@", NewPasswordConfirmation = "Password123@" };

        // When: the forgot-password confirmation endpoint is called
        // Then: token validation fails with InvalidPasswordResetToken
        await PerformCallAndExpectError(() => InvokeConfirmResetPasswordEndpoint(command), UserErrors.InvalidPasswordResetToken);

    }

    [Test]
    public async Task ShouldReturnBadRequestWhenConfirmingPasswordRecoveryWithEmptyToken()
    {
        // Given: an empty token and valid password fields
        ConfirmResetPasswordRequest command = new() { EncryptedToken = string.Empty, NewPassword = "Password123@", NewPasswordConfirmation = "Password123@" };

        // When: the forgot-password confirmation endpoint is called
        // Then: token validation fails with InvalidPasswordResetToken
        await PerformCallAndExpectError(() => InvokeConfirmResetPasswordEndpoint(command), UserErrors.InvalidPasswordResetToken);

    }

    [Test]
    public async Task ShouldReturnBadRequestWhenConfirmingPasswordRecoveryWithDifferentPasswords()
    {
        // Given: a command where confirmation password does not match
        ConfirmResetPasswordRequest command = new() { EncryptedToken = "v1.invalid", NewPassword = "Password123@", NewPasswordConfirmation = "Password456@" };

        // When: the forgot-password confirmation endpoint is called
        // Then: validator returns invalid password error
        await PerformCallAndExpectError(() => InvokeConfirmResetPasswordEndpoint(command), UserErrors.InvalidPassword);
        
    }
}
