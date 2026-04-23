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
    [Description("Given a valid command with an encrypted code, when validating, then it should pass")]
    public async Task ShouldPassValidationWhenEncryptedCodeIsPresent()
    {
        // Given
        VerifyUserCommand command = new("v1.someencryptedtoken");

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("", Description = "Empty encrypted code")]
    [TestCase(null!, Description = "Null encrypted code")]
    [TestCase("   ", Description = "Whitespace encrypted code")]
    [Description("Given an invalid encrypted code, when validating, then it should fail with InvalidPasswordResetToken error")]
    public async Task ShouldHaveErrorWhenEncryptedCodeIsEmptyOrNull(string encryptedCode)
    {
        // Given
        VerifyUserCommand command = new(encryptedCode);

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == UserErrors.InvalidPasswordResetToken)
        );
    }
}
