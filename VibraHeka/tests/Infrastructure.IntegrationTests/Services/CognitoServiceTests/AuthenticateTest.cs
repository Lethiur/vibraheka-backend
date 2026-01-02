using System.ComponentModel;
using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Models.Results;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.CognitoServiceTests;

[TestFixture]
public class AuthenticateUserTests : GenericCognitoServiceTest
{
    private const string DefaultPassword = "ValidPassword123!";

    #region AuthenticateUserAsync - Success Cases

    [Test]
    [DisplayName("Should successfully authenticate a confirmed user")]
    public async Task ShouldAuthenticateUserSuccessfullyWhenCredentialsAreValid()
    {
        // Given: A registered and confirmed user
        string email = GenerateUniqueEmail("test-auth-success@");
        await RegisterUser(email);
        Result<VerificationCodeEntity> codeResult = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));
        await _cognitoService.ConfirmUserAsync(email, codeResult.Value.Code);

        // When: Attempting to authenticate
        Result<AuthenticationResult> authResult = await _cognitoService.AuthenticateUserAsync(email, DefaultPassword);

        // Then: Authentication should succeed and return tokens
        Assert.That(authResult.IsSuccess, Is.True);
        Assert.That(authResult.Value.UserID, Is.Not.Null.Or.Empty);
        Assert.That(authResult.Value.AccessToken, Is.Not.Null.Or.Empty);
        Assert.That(authResult.Value.RefreshToken, Is.Not.Null.Or.Empty);
    }

    #endregion

    #region AuthenticateUserAsync - User Not Found Cases

    [Test]
    [DisplayName("Should fail when user does not exist")]
    public async Task ShouldReturnUserNotFoundWhenUserDoesNotExist()
    {
        // Given: A non-existent email
        string email = $"ghost-{Guid.NewGuid()}@example.com";

        // When: Attempting to authenticate
        Result<AuthenticationResult> authResult = await _cognitoService.AuthenticateUserAsync(email, DefaultPassword);

        // Then: Should fail with UserNotFound error
        Assert.That(authResult.IsFailure, Is.True);
        Assert.That(authResult.Error, Is.EqualTo(UserException.UserNotFound));
    }

    #endregion

    #region AuthenticateUserAsync - Invalid Password Cases

    [Test]
    [DisplayName("Should fail when password is incorrect")]
    public async Task ShouldReturnInvalidPasswordWhenPasswordIsIncorrect()
    {
        // Given: A registered and confirmed user
        string email = GenerateUniqueEmail("test-auth-wrong-pass@");
        await RegisterUser(email);
        Result<VerificationCodeEntity> codeResult = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));
        await _cognitoService.ConfirmUserAsync(email, codeResult.Value.Code);

        // When: Attempting to authenticate with wrong password
        Result<AuthenticationResult> authResult = await _cognitoService.AuthenticateUserAsync(email, "WrongPass123!");

        // Then: Should fail with InvalidPassword error
        Assert.That(authResult.IsFailure, Is.True);
        Assert.That(authResult.Error, Is.EqualTo(UserException.InvalidPassword)); 
        // Nota: Cognito a veces lanza UserNotFound o NotAuthorized por seguridad para no revelar si el usuario existe.
        // Según tu implementación de CognitoService.cs, NotAuthorizedException mapea a UserException.InvalidPassword.
    }

    #endregion

    #region AuthenticateUserAsync - User Status Cases

    [Test]
    [DisplayName("Should fail when user is registered but not confirmed")]
    public async Task ShouldReturnUserNotConfirmedWhenUserIsNotConfirmed()
    {
        // Given: A registered user that is NOT yet confirmed
        string email = GenerateUniqueEmail("test-auth-unconfirmed@");
        await RegisterUser(email);

        // When: Attempting to authenticate
        Result<AuthenticationResult> authResult = await _cognitoService.AuthenticateUserAsync(email, DefaultPassword);

        // Then: Should fail with UserNotConfirmed error
        Assert.That(authResult.IsFailure, Is.True);
        Assert.That(authResult.Error, Is.EqualTo(UserException.UserNotConfirmed));
    }

    #endregion

    #region AuthenticateUserAsync - Empty/Null Parameters

    [TestCase("", "ValidPass123!", TestName = "Empty email")]
    [TestCase("test@example.com", "", TestName = "Empty password")]
    [TestCase(null, "ValidPass123!", TestName = "Null email")]
    [TestCase("test@example.com", null, TestName = "Null password")]
    [DisplayName("Should fail when parameters are empty or null")]
    public async Task ShouldReturnUnexpectedErrorWhenParametersAreInvalid(string? email, string? password)
    {
        // When: Attempting to authenticate with invalid parameters
        Result<AuthenticationResult> authResult = await _cognitoService.AuthenticateUserAsync(email!, password!);

        // Then: Should fail (AWS SDK usually throws for nulls, caught as UnexpectedError in your catch-all)
        Assert.That(authResult.IsFailure, Is.True);
        Assert.That(authResult.Error, Is.EqualTo(UserException.UnexpectedError));
    }

    #endregion
}
