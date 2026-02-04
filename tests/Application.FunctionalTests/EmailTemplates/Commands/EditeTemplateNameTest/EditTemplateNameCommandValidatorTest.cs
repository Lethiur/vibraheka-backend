using FluentValidation.Results;
using NUnit.Framework;
using VibraHeka.Application.EmailTemplates.Commands.EditTemplateName;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.FunctionalTests.EmailTemplates.Commands.EditeTemplateNameTest;

[TestFixture]
public class EditTemplateNameCommandValidatorTest
{
    private EditTemplateNameCommandValidator _validator = default!;

    [SetUp]
    public void SetUp()
    {
        _validator = new EditTemplateNameCommandValidator();
    }

    [Test]
    [Description("Given a command with valid data, when validating, then it should pass validation")]
    public async Task ShouldPassValidationWhenCommandIsValid()
    {
        // Given
        EditTemplateNameCommand command = new(Guid.NewGuid().ToString(), "New Template Name");

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("", Description = "Empty ID")]
    [TestCase(null!, Description = "Null ID")]
    [TestCase("not-a-guid", Description = "Invalid GUID format")]
    [Description("Given a command with invalid TemplateID, when validating, then it should fail with InvalidTempalteID error")]
    public async Task ShouldFailValidationWhenTemplateIdIsInvalid(string templateId)
    {
        // Given
        EditTemplateNameCommand command = new(templateId, "Valid Name");

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == EmailTemplateErrors.InvalidTempalteID)
        );
    }

    [TestCase("", Description = "Empty name")]
    [TestCase(null!, Description = "Null name")]
    [TestCase("ab", Description = "Name too short")]
    [Description("Given a command with invalid NewTemplateName, when validating, then it should fail with InvalidTemplateName error")]
    public async Task ShouldFailValidationWhenNewTemplateNameIsInvalid(string newName)
    {
        // Given
        EditTemplateNameCommand command = new(Guid.NewGuid().ToString(), newName);

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == EmailTemplateErrors.InvalidTemplateName)
        );
    }
}
