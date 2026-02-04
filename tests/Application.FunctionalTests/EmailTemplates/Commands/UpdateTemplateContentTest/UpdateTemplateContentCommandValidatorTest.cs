using System.Text;
using FluentValidation.Results;
using NUnit.Framework;
using VibraHeka.Application.EmailTemplates.Commands.UpdateTemplateContent;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.FunctionalTests.EmailTemplates.Commands.UpdateTemplateContentTest;

[TestFixture]
public class UpdateTemplateContentCommandValidatorTest
{
    private UpdateTemplateContentCommandValidator _validator = default!;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateTemplateContentCommandValidator();
    }

    [Test]
    [Description("Given a command with valid data, when validating, then it should pass validation")]
    public async Task ShouldPassValidationWhenCommandIsValid()
    {
        // Given
        MemoryStream stream = new(Encoding.UTF8.GetBytes("{\"test\":\"content\"}"));
        UpdateTemplateContentCommand command = new(Guid.NewGuid().ToString(), stream);

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("", Description = "Empty ID")]
    [TestCase(null!, Description = "Null ID")]
    [TestCase("invalid-guid", Description = "Invalid GUID format")]
    [Description("Given a command with invalid TemplateID, when validating, then it should fail with InvalidTempalteID error")]
    public async Task ShouldFailValidationWhenTemplateIdIsInvalid(string templateId)
    {
        // Given
        MemoryStream stream = new(Encoding.UTF8.GetBytes("{}"));
        UpdateTemplateContentCommand command = new(templateId, stream);

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == EmailTemplateErrors.InvalidTempalteID)
        );
    }

    [Test]
    [Description("Given a command with null stream, when validating, then it should fail")]
    public async Task ShouldFailValidationWhenStreamIsNull()
    {
        // Given
        UpdateTemplateContentCommand command = new(Guid.NewGuid().ToString(), null!);

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    [Description("Given a command with empty stream, when validating, then it should fail")]
    public async Task ShouldFailValidationWhenStreamIsEmpty()
    {
        // Given
        MemoryStream stream = new(Array.Empty<byte>());
        UpdateTemplateContentCommand command = new(Guid.NewGuid().ToString(), stream);

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
    }
}
