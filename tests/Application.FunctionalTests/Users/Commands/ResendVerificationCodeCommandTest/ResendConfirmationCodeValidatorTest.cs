using FluentValidation.Results;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.ResendConfirmationCode;

namespace VibraHeka.Application.FunctionalTests.Users.Commands.ResendVerificationCodeCommandTest;

[TestFixture]
public class ResendConfirmationCodeValidatorTest
{
    private ResendConfirmationCodeValidator _validator = default!;

    [SetUp]
    public void SetUp()
    {
        _validator = new ResendConfirmationCodeValidator();
    }

    [Test]
    [Description("Given a command with a valid email, when validating, then it should pass validation")]
    public async Task ShouldPassValidationWhenEmailIsValid()
    {
        // Given
        ResendConfirmationCodeCommand command = new("test@example.com");

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("", Description = "Empty email")]
    [TestCase(null!, Description = "Null email")]
    [TestCase("invalid-email", Description = "Invalid email format")]
    [Description("Given a command with an invalid email, when validating, then it should fail with InvalidEmail error")]
    public async Task ShouldFailValidationWhenEmailIsInvalid(string email)
    {
        // Given
        ResendConfirmationCodeCommand command = new(email);

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == UserErrors.InvalidEmail)
        );
    }
}
