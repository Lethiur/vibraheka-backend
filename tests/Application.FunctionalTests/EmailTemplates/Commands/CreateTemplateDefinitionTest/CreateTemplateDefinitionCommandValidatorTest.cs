using FluentValidation.Results;
using NUnit.Framework;
using VibraHeka.Application.EmailTemplates.Commands.CreateTemplateDefinition;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.FunctionalTests.EmailTemplates.Commands;

[TestFixture]
public class CreateTemplateDefinitionCommandValidatorTest
{
    private CreateTemplateDefinitionCommandValidator _validator = default!;

    [SetUp]
    public void SetUp()
    {
        _validator = new CreateTemplateDefinitionCommandValidator();
    }

    [Test]
    [Description("Given a valid template definition command, when validating, then it should pass validation")]
    public async Task ShouldPassValidationWhenCommandIsCorrect()
    {
        // Given
        CreateTemplateDefinitionCommand command = new("Welcome Template");

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("", Description = "Empty name")]
    [TestCase(null!, Description = "Null name")]
    [TestCase("Te", Description = "Too short name")]
    [Description("Given an invalid template name, when validating, then it should fail with InvalidTemplateName error")]
    public async Task ShouldHaveErrorWhenTemplateNameIsInvalid(string templateName)
    {
        // Given
        CreateTemplateDefinitionCommand command = new(templateName);

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
