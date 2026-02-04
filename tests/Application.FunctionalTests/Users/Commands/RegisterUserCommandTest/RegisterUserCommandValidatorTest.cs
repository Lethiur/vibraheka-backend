using FluentValidation.Results;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.RegisterUser;

namespace VibraHeka.Application.FunctionalTests.Users.Commands.RegisterUserCommandTest;

[TestFixture]
public class RegisterUserCommandValidatorTest
{
    private RegisterUserCommandValidator _validator = default!;

    [SetUp]
    public void SetUp()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Test]
    [Description("Given a valid registration command, when validating, then it should pass validation")]
    public async Task ShouldPassValidationWhenCommandIsCorrect()
    {
        // Given
        RegisterUserCommand command = new("test@example.com", "Password123!", "John Doe");

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
        RegisterUserCommand command = new(email, "Password123!", "John Doe");

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
        RegisterUserCommand command = new("test@example.com", password, "John Doe");

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == UserErrors.InvalidPassword)
        );
    }

    [TestCase("", Description = "Empty name")]
    [TestCase(null!, Description = "Null name")]
    [TestCase("Jo", Description = "Too short name")]
    [Description("Given an invalid full name, when validating, then it should fail with InvalidFullName error")]
    public async Task ShouldHaveErrorWhenFullNameIsInvalid(string name)
    {
        // Given
        RegisterUserCommand command = new("test@example.com", "Password123!", name);

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == UserErrors.InvalidFullName)
        );
    }
}
