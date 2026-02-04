using System.Text;
using FluentValidation.Results;
using NUnit.Framework;
using VibraHeka.Application.EmailTemplates.Commands.CreateEmail;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.FunctionalTests.EmailTemplates.Commands.CreateEmailTemplateTest;

[TestFixture]
public class CreateEmailTemplateCommandValidatorTests
{
    private CreateEmailTemplateCommandValidator Validator;

    [SetUp]
    public void SetUp()
    {
        Validator = new CreateEmailTemplateCommandValidator();
    }

    // ----------------------------
    // TestCase sources (JSON)
    // ----------------------------

    public static IEnumerable<TestCaseData> ValidJsonCases()
    {
        // JSON objects
        yield return new TestCaseData("""{"template":"Welcome Email","subject":"Welcome"}""")
            .SetName("ShouldPassValidationWhenJsonIsValidObjectSimple");
        yield return new TestCaseData("""{"nested":{"data":"value"},"arr":[1,2,3]}""")
            .SetName("ShouldPassValidationWhenJsonIsValidObjectNested");

        // JSON arrays
        yield return new TestCaseData("""[]""")
            .SetName("ShouldPassValidationWhenJsonIsValidArrayEmpty");
        yield return new TestCaseData("""[{"id":1},{"id":2},{"id":3}]""")
            .SetName("ShouldPassValidationWhenJsonIsValidArrayOfObjects");

        // JSON primitives (JsonDocument.ParseAsync accepts these)
        yield return new TestCaseData("123")
            .SetName("ShouldPassValidationWhenJsonIsValidPrimitiveNumber");
        yield return new TestCaseData("true")
            .SetName("ShouldPassValidationWhenJsonIsValidPrimitiveBoolean");
        yield return new TestCaseData("null")
            .SetName("ShouldPassValidationWhenJsonIsValidPrimitiveNull");
      
    }
    // ----------------------------
    // Tests
    // ----------------------------

    [TestCase(null!)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("AB")]
    [Description("Given a command with invalid template name, when validating, then it should fail with InvalidTemplateName error")]
    public async Task ShouldFailValidationWhenTemplateNameIsInvalid(string templateName)
    {
        // Given
        string validJson = """{"template":"Test"}""";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        // When
        ValidationResult result = await Validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == EmailTemplateErrors.InvalidTemplateName)
        );
    }

    [TestCase("ABC")]
    [TestCase("Welcome Email Template")]
    [Description("Given a command with valid template name, when validating, then it should pass validation")]
    public async Task ShouldPassValidationWhenTemplateNameIsValid(string templateName)
    {
        // Given
        string validJson = """{"template":"Test"}""";
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, templateName);

        // When
        ValidationResult result = await Validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    [Description("Given a command with null file stream, when validating, then it should fail with InvalidTemplateContent error")]
    public async Task ShouldFailValidationWhenFileStreamIsNull()
    {
        // Given
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(null!, "Valid Template Name");

        // When
        ValidationResult result = await Validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == EmailTemplateErrors.InvalidTemplateContent)
        );
    }

    [Test]
    [Description("Given a command with empty file stream, when validating, then it should fail with InvalidTemplateContent error")]
    public async Task ShouldFailValidationWhenFileStreamIsEmpty()
    {
        // Given
        MemoryStream fileStream = new MemoryStream([]);
        CreateEmailTemplateCommand command = new(fileStream, "Valid Template Name");

        // When
        ValidationResult result = await Validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == EmailTemplateErrors.InvalidTemplateContent)
        );
    }

    [TestCaseSource(nameof(ValidJsonCases))]
    [Description("Given a command with valid JSON in file stream, when validating, then it should pass validation")]
    public async Task ShouldPassValidationWhenJsonIsValid(string jsonContent)
    {
        // Given
        MemoryStream fileStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(fileStream, "Valid Template Name");

        // When
        ValidationResult result = await Validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }
    
    [Test]
    [Description("Given a command with null template name and null file stream, when validating, then it should fail with both errors")]
    public async Task ShouldFailValidationWhenBothFieldsAreInvalid()
    {
        // Given
        CreateEmailTemplateCommand command = new CreateEmailTemplateCommand(null!, null!);

        // When
        ValidationResult result = await Validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == EmailTemplateErrors.InvalidTemplateName)
        );
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == EmailTemplateErrors.InvalidTemplateContent)
        );
    }
}
