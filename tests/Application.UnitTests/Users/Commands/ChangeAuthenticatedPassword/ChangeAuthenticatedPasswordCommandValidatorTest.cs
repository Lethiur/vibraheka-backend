using FluentValidation.TestHelper;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.ChangeAuthenticatedPassword;

namespace VibraHeka.Application.UnitTests.Users.Commands.ChangeAuthenticatedPassword;

[TestFixture]
public class ChangeAuthenticatedPasswordCommandValidatorTest
{
    private ChangeAuthenticatedPasswordCommandValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new ChangeAuthenticatedPasswordCommandValidator();
    }

    [Test]
    public void ShouldFailWhenCurrentPasswordIsEmpty()
    {
        // Given: a command with an empty current password.
        ChangeAuthenticatedPasswordCommand command = new("", "NewPassword123!", "NewPassword123!");

        // When: validating the command.
        TestValidationResult<ChangeAuthenticatedPasswordCommand> result = _validator.TestValidate(command);

        // Then: current password should be invalid.
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword)
            .WithErrorMessage(UserErrors.InvalidPassword);
    }

    [Test]
    public void ShouldFailWhenNewPasswordConfirmationDoesNotMatch()
    {
        // Given: a command where new password confirmation does not match.
        ChangeAuthenticatedPasswordCommand command = new("Current123!", "NewPassword123!", "Different123!");

        // When: validating the command.
        TestValidationResult<ChangeAuthenticatedPasswordCommand> result = _validator.TestValidate(command);

        // Then: confirmation should be invalid.
        result.ShouldHaveValidationErrorFor(x => x.NewPasswordConfirmation)
            .WithErrorMessage(UserErrors.InvalidPassword);
    }

    [Test]
    public void ShouldPassWhenCommandIsValid()
    {
        // Given: a valid authenticated password change command.
        ChangeAuthenticatedPasswordCommand command = new("Current123!", "NewPassword123!", "NewPassword123!");

        // When: validating the command.
        TestValidationResult<ChangeAuthenticatedPasswordCommand> result = _validator.TestValidate(command);

        // Then: command should be valid.
        result.ShouldNotHaveAnyValidationErrors();
    }
}
