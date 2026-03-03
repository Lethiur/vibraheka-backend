using System.ComponentModel;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.UserServiceTest;

[TestFixture]
public class ChangePasswordAsyncIntegrationTest : GenericCognitoServiceTest
{
    [Test]
    [DisplayName("Should change password and allow authenticating with the new one")]
    public async Task ShouldChangePasswordAndAuthenticateWithNewPassword()
    {
        // Given: a registered and confirmed user with a known initial password.
        string email = GenerateUniqueEmail("test-change-password@");
        const string initialPassword = "ValidPassword123!";
        const string newPassword = "UpdatedPassword123!";
        const string fullName = "Password Change User";

        Result<string> registerResult = await _userService.RegisterUserAsync(email, initialPassword, fullName);
        Assert.That(registerResult.IsSuccess, Is.True);

        Result<VerificationCodeEntity> codeResult = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));
        Assert.That(codeResult.IsSuccess, Is.True);
        Result<Unit> confirmResult = await _userService.ConfirmUserAsync(email, codeResult.Value.Code);
        Assert.That(confirmResult.IsSuccess, Is.True);

        Result<AuthenticationResult> authResult = await _userService.AuthenticateUserAsync(email, initialPassword);
        Assert.That(authResult.IsSuccess, Is.True);
        string accessToken = authResult.Value.AccessToken;

        // When: changing password using the authenticated access token.
        Result<Unit> changePasswordResult =
            await _userService.ChangePasswordAsync(accessToken, initialPassword, newPassword, CancellationToken.None);

        // Then: password change succeeds and authentication with new password works.
        Assert.That(changePasswordResult.IsSuccess, Is.True);

        Result<AuthenticationResult> oldPasswordAuthResult = await _userService.AuthenticateUserAsync(email, initialPassword);
        Assert.That(oldPasswordAuthResult.IsFailure, Is.True);

        Result<AuthenticationResult> newPasswordAuthResult = await _userService.AuthenticateUserAsync(email, newPassword);
        Assert.That(newPasswordAuthResult.IsSuccess, Is.True);
        Assert.That(newPasswordAuthResult.Value.AccessToken, Is.Not.Null.Or.Empty);
    }

    [Test]
    [DisplayName("Should fail when changing password with an invalid current password")]
    public async Task ShouldFailWhenCurrentPasswordIsInvalid()
    {
        // Given: a registered and confirmed user.
        string email = GenerateUniqueEmail("test-change-password-wrong-current@");
        const string initialPassword = "ValidPassword123!";
        const string newPassword = "UpdatedPassword123!";
        const string wrongCurrentPassword = "WrongCurrent123!";
        const string fullName = "Wrong Current User";

        Result<string> registerResult = await _userService.RegisterUserAsync(email, initialPassword, fullName);
        Assert.That(registerResult.IsSuccess, Is.True);

        Result<VerificationCodeEntity> codeResult = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));
        Assert.That(codeResult.IsSuccess, Is.True);
        Result<Unit> confirmResult = await _userService.ConfirmUserAsync(email, codeResult.Value.Code);
        Assert.That(confirmResult.IsSuccess, Is.True);

        Result<AuthenticationResult> authResult = await _userService.AuthenticateUserAsync(email, initialPassword);
        Assert.That(authResult.IsSuccess, Is.True);
        string accessToken = authResult.Value.AccessToken;

        // When: changing password with wrong current password.
        Result<Unit> changePasswordResult =
            await _userService.ChangePasswordAsync(accessToken, wrongCurrentPassword, newPassword, CancellationToken.None);

        // Then: operation should fail and old password should still work.
        Assert.That(changePasswordResult.IsFailure, Is.True);

        Result<AuthenticationResult> initialPasswordAuthResult = await _userService.AuthenticateUserAsync(email, initialPassword);
        Assert.That(initialPasswordAuthResult.IsSuccess, Is.True);
    }
}
