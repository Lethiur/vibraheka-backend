using System.ComponentModel;
using FluentValidation.TestHelper;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.AuthenticateUsers;

namespace VibraHeka.Application.UnitTests.Users.Commands.AuthenticateUser;

[TestFixture]
public class AuthenticateUserCommandValidatorTest
{
    private AuthenticateUserCommandValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new AuthenticateUserCommandValidator();
    }

    #region Email Validation Cases

    [Test]
    [DisplayName("Should be valid when all fields are correct")]
    public void ShouldBeValidWhenCommandIsCorrect()
    {
        // Given
        AuthenticateUserCommand command = new AuthenticateUserCommand ("test@example.com", "Password123!" );

        // When
        TestValidationResult<AuthenticateUserCommand>? result = _validator.TestValidate(command);

        // Then
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestCase("", TestName = "Empty email")]
    [TestCase(null, TestName = "Null email")]
    [DisplayName("Should fail when email is empty or null")]
    public void ShouldHaveErrorWhenEmailIsMissing(string? email)
    {
        // Given
        AuthenticateUserCommand command = new AuthenticateUserCommand (email!, "Password123!" );

        // When
        TestValidationResult<AuthenticateUserCommand>? result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(UserException.InvalidEmail);
    }

    [TestCase("invalid-email", TestName = "Email without @")]
    [TestCase("test@", TestName = "Email without domain")]
    [TestCase("@example.com", TestName = "Email without local part")]
    [DisplayName("Should fail when email format is invalid")]
    public void ShouldHaveErrorWhenEmailFormatIsInvalid(string invalidEmail)
    {
        // Given
        AuthenticateUserCommand command = new AuthenticateUserCommand (invalidEmail, "Password123!" );

        // When
        TestValidationResult<AuthenticateUserCommand>? result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(UserException.InvalidEmail);
    }

    #endregion

    #region Password Validation Cases

    [TestCase("", TestName = "Empty password")]
    [TestCase(null, TestName = "Null password")]
    [DisplayName("Should fail when password is empty or null")]
    public void ShouldHaveErrorWhenPasswordIsMissing(string? password)
    {
        // Given
        AuthenticateUserCommand command = new AuthenticateUserCommand ("test@example.com", password! );

        // When
        TestValidationResult<AuthenticateUserCommand>? result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(UserException.InvalidPassword);
    }

    [Test]
    [DisplayName("Should fail when password is shorter than 6 characters")]
    public void ShouldHaveErrorWhenPasswordIsTooShort()
    {
        // Given
        AuthenticateUserCommand command = new AuthenticateUserCommand ("test@example.com", "Pas");

        // When
        TestValidationResult<AuthenticateUserCommand>? result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(UserException.InvalidPassword);
    }

    #endregion
}
