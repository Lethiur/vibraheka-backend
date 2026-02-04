using FluentValidation.Results;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Queries.GetCode;

namespace VibraHeka.Application.FunctionalTests.Users.Queries;

[TestFixture]
public class GetCodeQueryValidatorTest
{
    private GetCodeQueryValidator _validator = default!;

    [SetUp]
    public void SetUp()
    {
        _validator = new GetCodeQueryValidator();
    }

    [Test]
    [Description("Given a query with a valid UserName (email), when validating, then it should pass validation")]
    public async Task ShouldPassValidationWhenUserNameIsValid()
    {
        // Given
        GetCodeQuery query = new("test@example.com");

        // When
        ValidationResult result = await _validator.ValidateAsync(query);

        // Then
        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("", Description = "Empty UserName")]
    [TestCase(null!, Description = "Null UserName")]
    [TestCase("invalid-email", Description = "Invalid email format")]
    [Description("Given a query with an invalid UserName, when validating, then it should fail with InvalidEmail error")]
    public async Task ShouldFailValidationWhenUserNameIsInvalid(string userName)
    {
        // Given
        GetCodeQuery query = new(userName);

        // When
        ValidationResult result = await _validator.ValidateAsync(query);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == UserErrors.InvalidEmail)
        );
    }
}
