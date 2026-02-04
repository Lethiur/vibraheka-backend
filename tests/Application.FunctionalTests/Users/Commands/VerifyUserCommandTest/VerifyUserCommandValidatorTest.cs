using FluentValidation.Results;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.VerificationCode;

namespace VibraHeka.Application.FunctionalTests.Users.Commands.VerifyUserCommandTest;

[TestFixture]
public class VerifyUserCommandValidatorTest
{
    private VerifyUserCommandValidator _validator = default!;

    [SetUp]
    public void SetUp()
    {
        _validator = new VerifyUserCommandValidator();
    }

    [Test]
    [Description("Given a valid verification command, when validating, then it should pass validation")]
    public async Task ShouldPassValidationWhenCommandIsCorrect()
    {
        // Given
        VerifyUserCommand command = new("test@example.com", "123456");

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("", Description = "Empty email")]
    [TestCase(null!, Description = "Null email")]
    [TestCase("invalid-email", Description = "Invalid email format")]
    [Description("Given an invalid email for verification, when validating, then it should fail with InvalidEmail error")]
    public async Task ShouldHaveErrorWhenEmailIsInvalid(string email)
    {
        // Given
        VerifyUserCommand command = new(email, "123456");

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == UserErrors.InvalidEmail)
        );
    }

    [TestCase("", Description = "Empty code")]
    [TestCase(null!, Description = "Null code")]
    [TestCase("abc", Description = "Non-numeric code")]
    [TestCase("12345", Description = "Too short code")]
    [Description("Given an invalid verification code, when validating, then it should fail with InvalidVerificationCode error")]
    public async Task ShouldHaveErrorWhenCodeIsInvalid(string code)
    {
        // Given
        VerifyUserCommand command = new("test@example.com", code);

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == UserErrors.InvalidVerificationCode)
        );
    }
}
