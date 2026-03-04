using System.ComponentModel;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.AdminCreateTherapist;
using VibraHeka.Domain.Models.Results.User;

namespace VibraHeka.Application.UnitTests.Users.Commands.AdminCreateTherapist;

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
        CreateTherapistCommand command = new(new UserDTO() { Email = email!, FirstName = "Dr. Smith" });


        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);

        // Then: Should have validation error for email
        result.ShouldHaveValidationErrorFor(x => x.TherapistData.Email)
            .WithErrorMessage(UserErrors.InvalidEmail);
    }

    [TestCase("invalid-email", TestName = "No @ symbol")]
    [TestCase("@domain.com", TestName = "Missing local part")]
    [TestCase("user@", TestName = "Missing domain")]
    [TestCase("user@domain", TestName = "Missing TLD")]
    [DisplayName("Should fail when email format is invalid")]
    public void ShouldFailValidationWhenEmailFormatIsInvalid(string email)
    {
        // Given: Command with invalid email format
        CreateTherapistCommand command = new(new UserDTO() { Email = email, FirstName = "Dr. Smith" });


        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);

        // Then: Should have validation error for email
        result.ShouldHaveValidationErrorFor(x => x.TherapistData.Email)
            .WithErrorMessage(UserErrors.InvalidEmail);
    }

    [TestCase("therapist@example.com", TestName = "Basic valid email")]
    [TestCase("dr.house@hospital.org", TestName = "Valid professional email")]
    [DisplayName("Should pass when email format is valid")]
    public void ShouldPassValidationWhenEmailFormatIsValid(string email)
    {
        // Given: Command with valid email format
        CreateTherapistCommand command = new(new UserDTO() { Email = email, FirstName = "Dr. Smith" });

        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);

        // Then: Should not have validation error for email
        result.ShouldNotHaveValidationErrorFor(x => x.TherapistData.Email);
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
        CreateTherapistCommand command = new(new UserDTO() { Email = "test@therapist.com", FirstName = name!, MiddleName = "Test", LastName = "Test", Bio = "Test", TimezoneID = "Europe/Madrid", PhoneNumber = "6359875", ProfilePictureUrl = "test"});


        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);

        // Then: Should have validation error for name
        result.ShouldHaveValidationErrorFor(x => x.TherapistData.FirstName)
            .WithErrorMessage(UserErrors.InvalidFullName);
    }

    [TestCase("John Doe", TestName = "Normal name")]
    [TestCase("Dr. Smith", TestName = "Name with title")]
    [DisplayName("Should pass when name is valid")]
    public void ShouldPassValidationWhenNameIsValid(string name)
    {
        // Given: Command with valid name
        CreateTherapistCommand command = new(new UserDTO() { Email = "test@therapist.com", FirstName = name });


        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);

        // Then: Should not have validation error for name
        result.ShouldNotHaveValidationErrorFor(x => x.TherapistData.FirstName);
    }

    #endregion

    #region Multiple Field Validation Tests

    [Test]
    [DisplayName("Should pass when all fields are valid")]
    public void ShouldPassValidationWhenAllFieldsAreValid()
    {
        // Given: Command with all valid fields
        CreateTherapistCommand command = new(new UserDTO()
        {
            Email = "test@therapist.com",
            FirstName = "Dr. Smith",
            Bio = "  ASDFASDF",
            LastName = "Test",
            MiddleName = "Test",
            PhoneNumber = "6359875",
            TimezoneID = "Europe/Madrid"
        });

        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);

        // Then: Should not have any validation errors
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Url validation Tests

    [Test]
    public void ShouldFailValidationWhenUrlIsInvalid()
    {   
        // Given: A command with an invalid url
        CreateTherapistCommand command = new(new UserDTO()
        {
            Email = "test@therapist.com",
            FirstName = "Dr. Smith",
            Bio = "  ASDFASDF",
            LastName = "Test",
            MiddleName = "Test",
            PhoneNumber = "6359875",
            TimezoneID = "Europe/Madrid",
            ProfilePictureUrl = "invalid-url"
        });
        
        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);
        
        // Then: Should have validation errors
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsValid, Is.False);
        
        result.ShouldHaveValidationErrorFor(x => x.TherapistData.ProfilePictureUrl)
            .WithErrorMessage(UserErrors.InvalidForm);
    }

    [Test]
    public void ShouldPassValidationWhenUrlIsValid()
    {
        // Given: A command with an invalid url
        CreateTherapistCommand command = new(new UserDTO()
        {
            Email = "test@therapist.com",
            FirstName = "Dr. Smith",
            Bio = "  ASDFASDF",
            LastName = "Test",
            MiddleName = "Test",
            PhoneNumber = "6359875",
            TimezoneID = "Europe/Madrid",
            ProfilePictureUrl = "https://example.com/avatar.png"
        });
        
        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);
        
        // Then: Should have validation errors
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsValid, Is.True);
        
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void ShouldFailWhenUrlIsTooLong()
    {
        // Given: A command with an invalid url
        CreateTherapistCommand command = new(new UserDTO()
        {
            Email = "test@therapist.com",
            FirstName = "Dr. Smith",
            Bio = "  ASDFASDF",
            LastName = "Test",
            MiddleName = "Test",
            PhoneNumber = "6359875",
            TimezoneID = "Europe/Madrid",
            ProfilePictureUrl = new string('a', 3001)
        });
        
        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);
        
        // Then: Should have validation errors
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsValid, Is.False);
        
        result.ShouldHaveValidationErrorFor(x => x.TherapistData.ProfilePictureUrl)
            .WithErrorMessage(UserErrors.InvalidForm);
    }
    #endregion
    
    #region Cascade Mode Tests

    [Test]
    [DisplayName("Should stop validation on first email error when cascade mode is stop")]
    public void ShouldStopValidationOnFirstEmailErrorWhenCascadeModeIsStop()
    {
        // Given: Command with empty email
        CreateTherapistCommand command = new(new UserDTO() { Email = "", FirstName = " " });

        // When: Validating the command
        TestValidationResult<CreateTherapistCommand>? result = Validator.TestValidate(command);

        // Then: Should have only one error for email
        IEnumerable<ValidationFailure> emailErrors =
            result.Errors.Where(e => e.PropertyName == "TherapistData.Email").ToList();
        IEnumerable<ValidationFailure> validationFailures = emailErrors.ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(validationFailures.Count(), Is.EqualTo(1));
            Assert.That(validationFailures.First().ErrorMessage, Is.EqualTo(UserErrors.InvalidEmail));
        }
    }

    #endregion
}
