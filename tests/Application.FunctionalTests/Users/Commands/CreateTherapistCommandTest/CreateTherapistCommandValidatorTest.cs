using FluentValidation.Results;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.AdminCreateTherapist;

namespace VibraHeka.Application.FunctionalTests.Users.Commands.CreateTherapistCommandTest;

[TestFixture]
public class CreateTherapistCommandValidatorTest
{
    private CreateTherapistCommandValidator _validator = default!;

    [SetUp]
    public void SetUp()
    {
        _validator = new CreateTherapistCommandValidator();
    }

    [Test]
    [Description("Given a valid therapist creation command, when validating, then it should pass validation")]
    public async Task ShouldPassValidationWhenCommandIsCorrect()
    {
        // Given
        CreateTherapistCommand command = new("therapist@example.com", "Dr. Jane Doe");

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("", Description = "Empty email")]
    [TestCase(null!, Description = "Null email")]
    [TestCase("invalid-email", Description = "Invalid email format")]
    [Description("Given an invalid email for therapist, when validating, then it should fail with InvalidEmail error")]
    public async Task ShouldHaveErrorWhenEmailIsInvalid(string email)
    {
        // Given
        CreateTherapistCommand command = new(email, "Dr. Jane Doe");

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == UserErrors.InvalidEmail)
        );
    }

    [TestCase("", Description = "Empty name")]
    [TestCase(null!, Description = "Null name")]
    [TestCase("   ", Description = "Whitespace name")]
    [Description("Given an invalid name for therapist, when validating, then it should fail with InvalidFullName error")]
    public async Task ShouldHaveErrorWhenNameIsInvalid(string name)
    {
        // Given
        CreateTherapistCommand command = new("therapist@example.com", name);

        // When
        ValidationResult result = await _validator.ValidateAsync(command);

        // Then
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == UserErrors.InvalidFullName)
        );
    }
}
