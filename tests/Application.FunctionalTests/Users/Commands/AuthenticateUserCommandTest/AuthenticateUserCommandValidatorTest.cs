using FluentValidation.Results;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.AuthenticateUsers;

namespace VibraHeka.Application.FunctionalTests.Users.Commands.AuthenticateUserCommandTest;

[TestFixture]
public class AuthenticateUserCommandValidatorTest
{
    private AuthenticateUserCommandValidator _validator = default!;

    [SetUp]
    public void SetUp()
    {
        _validator = new AuthenticateUserCommandValidator();
    }

    [Test]
    [Description("Given a valid authentication command, when validating, then it should pass validation")]
    public async Task ShouldPassValidationWhenCommandIsCorrect()
    {
        // Given
        AuthenticateUserCommand command = new("test@example.com", "Password123!");

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("", Description = "Empty email")]
    [TestCase(null!, Description = "Null email")]
    [TestCase("invalid-email", Description = "Invalid email format")]
    [Description("Given an invalid email, when validating, then it should fail with InvalidEmail error")]
    public async Task ShouldHaveErrorWhenEmailIsInvalid(string email)
    {
        // Given
        AuthenticateUserCommand command = new(email, "Password123!");

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == UserErrors.InvalidEmail)
        );
    }

    [TestCase("", Description = "Empty password")]
    [TestCase(null!, Description = "Null password")]
    [TestCase("Pas", Description = "Too short password")]
    [Description("Given an invalid password, when validating, then it should fail with InvalidPassword error")]
    public async Task ShouldHaveErrorWhenPasswordIsInvalid(string password)
    {
        // Given
        AuthenticateUserCommand command = new("test@example.com", password);

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == UserErrors.InvalidPassword)
        );
    }
}
