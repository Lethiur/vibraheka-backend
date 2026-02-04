using FluentValidation.Results;
using NUnit.Framework;
using VibraHeka.Application.EmailTemplates.Queries.GetEmailTemplateURL;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.FunctionalTests.EmailTemplates.Queries;

[TestFixture]
public class GetEmailTemplateURLQueryValidatorTest
{
    private GetEmailTemplateURLQueryValidator _validator = default!;

    [SetUp]
    public void SetUp()
    {
        _validator = new GetEmailTemplateURLQueryValidator();
    }

    [Test]
    [Description("Given a query with a valid TemplateID, when validating, then it should pass validation")]
    public async Task ShouldPassValidationWhenTemplateIdIsValid()
    {
        // Given
        GetEmailTemplateURLQuery query = new(Guid.NewGuid().ToString());

        // When
        ValidationResult result = await _validator.ValidateAsync(query);

        // Then
        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("", Description = "Empty ID")]
    [TestCase(null!, Description = "Null ID")]
    [TestCase("not-a-guid", Description = "Invalid GUID format")]
    [Description("Given a query with an invalid TemplateID, when validating, then it should fail with InvalidTempalteID error")]
    public async Task ShouldFailValidationWhenTemplateIdIsInvalid(string templateId)
    {
        // Given
        GetEmailTemplateURLQuery query = new(templateId);

        // When
        ValidationResult result = await _validator.ValidateAsync(query);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == EmailTemplateErrors.InvalidTempalteID)
        );
    }
}
