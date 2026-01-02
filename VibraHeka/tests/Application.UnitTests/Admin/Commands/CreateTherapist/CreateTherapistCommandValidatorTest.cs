using System.ComponentModel;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using NUnit.Framework;
using VibraHeka.Application.Admin.Commands.CreateTherapist;
using VibraHeka.Application.Common.Exceptions;

namespace VibraHeka.Application.UnitTests.Admin.Commands.CreateTherapist;

[TestFixture]
public class CreateTherapistCommandValidatorTest
{
    private CreateTherapistCommandValidator Validator;

    [SetUp]
    public void SetUp()
    {
        Validator = new CreateTherapistCommandValidator();
    }

    #region Email Validation Tests

    [TestCase("", TestName = "Empty email")]
    [TestCase("   ", TestName = "Whitespace email")]
    [TestCase(null, TestName = "Null email")]
    [DisplayName("Should fail when email is empty or null")]
    public void ShouldFailValidationWhenEmailIsEmptyOrNull(string? email)
    {
        // Given: Command with invalid email
        CreateTherapistCommand command = new CreateTherapistCommand(email!, "John Doe");

        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);

        // Then: Should have validation error for email
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage(UserException.InvalidEmail);
    }

    [TestCase("invalid-email", TestName = "No @ symbol")]
    [TestCase("@domain.com", TestName = "Missing local part")]
    [TestCase("user@", TestName = "Missing domain")]
    [TestCase("user@domain", TestName = "Missing TLD")]
    [DisplayName("Should fail when email format is invalid")]
    public void ShouldFailValidationWhenEmailFormatIsInvalid(string email)
    {
        // Given: Command with invalid email format
        CreateTherapistCommand command = new CreateTherapistCommand(email, "John Doe");

        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);

        // Then: Should have validation error for email
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage(UserException.InvalidEmail);
    }

    [TestCase("therapist@example.com", TestName = "Basic valid email")]
    [TestCase("dr.house@hospital.org", TestName = "Valid professional email")]
    [DisplayName("Should pass when email format is valid")]
    public void ShouldPassValidationWhenEmailFormatIsValid(string email)
    {
        // Given: Command with valid email format
        CreateTherapistCommand command = new CreateTherapistCommand(email, "John Doe");

        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);

        // Then: Should not have validation error for email
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Name Validation Tests

    [TestCase("", TestName = "Empty name")]
    [TestCase("   ", TestName = "Whitespace name")]
    [TestCase(null, TestName = "Null name")]
    [DisplayName("Should fail when name is empty or null")]
    public void ShouldFailValidationWhenNameIsEmptyOrNull(string? name)
    {
        // Given: Command with invalid name
        CreateTherapistCommand command = new CreateTherapistCommand("therapist@example.com", name!);

        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);

        // Then: Should have validation error for name
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage(UserException.InvalidFullName);
    }

    [TestCase("John Doe", TestName = "Normal name")]
    [TestCase("Dr. Smith", TestName = "Name with title")]
    [DisplayName("Should pass when name is valid")]
    public void ShouldPassValidationWhenNameIsValid(string name)
    {
        // Given: Command with valid name
        CreateTherapistCommand command = new CreateTherapistCommand("therapist@example.com", name);

        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);

        // Then: Should not have validation error for name
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region Multiple Field Validation Tests

    [Test]
    [DisplayName("Should pass when all fields are valid")]
    public void ShouldPassValidationWhenAllFieldsAreValid()
    {
        // Given: Command with all valid fields
        CreateTherapistCommand command = new CreateTherapistCommand("therapist@example.com", "John Doe");

        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);

        // Then: Should not have any validation errors
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Cascade Mode Tests

    [Test]
    [DisplayName("Should stop validation on first email error when cascade mode is stop")]
    public void ShouldStopValidationOnFirstEmailErrorWhenCascadeModeIsStop()
    {
        // Given: Command with empty email
        CreateTherapistCommand command = new CreateTherapistCommand("", "John Doe");

        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);

        // Then: Should have only one error for email
        IEnumerable<ValidationFailure> emailErrors = result.Errors.Where(e => e.PropertyName == nameof(CreateTherapistCommand.Email));
        IEnumerable<ValidationFailure> validationFailures = emailErrors.ToList();
        Assert.That(validationFailures.Count(), Is.EqualTo(1));
        Assert.That(validationFailures.First().ErrorMessage, Is.EqualTo(UserException.InvalidEmail));
    }

    #endregion
}
