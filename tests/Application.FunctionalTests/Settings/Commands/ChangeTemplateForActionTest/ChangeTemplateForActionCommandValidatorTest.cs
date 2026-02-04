using FluentValidation.Results;
using NUnit.Framework;
using VibraHeka.Application.Settings.Commands.ChangeTemplateForAction;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.FunctionalTests.Settings.Commands.ChangeTemplateForActionTest;

[TestFixture]
public class ChangeTemplateForActionCommandValidatorTest
{
    private ChangeTemplateForActionCommandValidator _validator = default!;

    [SetUp]
    public void SetUp()
    {
        _validator = new ChangeTemplateForActionCommandValidator();
    }

    [Test]
    [Description("Given a command with a valid TemplateID, when validating, then it should pass validation")]
    public async Task ShouldPassValidationWhenTemplateIdIsValid()
    {
        // Given
        ChangeTemplateForActionCommand command = new(Guid.NewGuid().ToString(), ActionType.PasswordReset);

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("", Description = "Empty ID")]
    [TestCase(null!, Description = "Null ID")]
    [TestCase("not-a-guid", Description = "Invalid GUID format")]
    [Description("Given a command with an invalid TemplateID, when validating, then it should fail with InvalidTempalteID error")]
    public async Task ShouldFailValidationWhenTemplateIdIsInvalid(string templateId)
    {
        // Given
        ChangeTemplateForActionCommand command = new(templateId, ActionType.PasswordReset);

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == EmailTemplateErrors.InvalidTempalteID)
        );
    }
}
