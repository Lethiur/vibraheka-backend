using System.ComponentModel;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Application.Common.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.UserServiceTest;

[TestFixture]
public class ResendVerificationCodeAsyncTests : GenericCognitoServiceTest
{
    [Test]
    [DisplayName("Should successfully resend verification code for an unconfirmed user")]
    public async Task ShouldResendVerificationCodeSuccessfullyForUnconfirmedUser()
    {
        // Given: A registered (unconfirmed) user
        string email = GenerateUniqueEmail("test-resend-code@");
        await RegisterUser(email);

        // When: Resending verification code
        Result<Unit> result = await _userService.ResendVerificationCodeAsync(email);

        // Then: Should succeed
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    [DisplayName("Should fail with UserNotFound when user does not exist")]
    public async Task ShouldFailWithUserNotFoundWhenUserDoesNotExist()
    {
        // Given: A non-existent email
        string email = $"ghost-{Guid.NewGuid()}@example.com";

        // When: Resending verification code
        Result<Unit> result = await _userService.ResendVerificationCodeAsync(email);

        // Then: Should map to our domain error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    [DisplayName("Should fail with InvalidForm when email is empty")]
    public async Task ShouldFailWithInvalidFormWhenEmailIsEmpty(string? email)
    {
        Result<Unit> result = await _userService.ResendVerificationCodeAsync(email!);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidForm));
    }
}

