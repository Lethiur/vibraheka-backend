using System.ComponentModel;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands;

namespace VibraHeka.Application.UnitTests.Users.Commands.RegisterUser;

[TestFixture]
public class RegisterUserCommandValidatorTests
{
    private RegisterUserCommandValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new RegisterUserCommandValidator();
    }

    #region Email Validation Tests

    [TestCase("", TestName = "Empty email")]
    [TestCase("   ", TestName = "Whitespace email")]
    [TestCase(null, TestName = "Null email")]
    [DisplayName("Should fail when email is empty or null")]
    public void ShouldFailValidationWhenEmailIsEmptyOrNull(string? email)
    {
        // Given: Command with invalid email
        var command = new RegisterUserCommand(email!, "Password123!", "John Doe");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should have validation error for email
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage(UserException.InvalidEmail);
    }

    [TestCase("invalid-email", TestName = "No @ symbol")]
    [TestCase("@domain.com", TestName = "Missing local part")]
    [TestCase("user@", TestName = "Missing domain")]
    [TestCase("user@domain", TestName = "Missing TLD")]
    [TestCase("user..test@domain.com", TestName = "Double dots")]
    [TestCase("user.domain.com", TestName = "Missing @ symbol")]
    [DisplayName("Should fail when email format is invalid")]
    public void ShouldFailValidationWhenEmailFormatIsInvalid(string email)
    {
        // Given: Command with invalid email format
        var command = new RegisterUserCommand(email, "Password123!", "John Doe");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should have validation error for email
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage(UserException.InvalidEmail);
    }

    [TestCase("user@example.com", TestName = "Basic valid email")]
    [TestCase("test.user+tag@domain.co.uk", TestName = "Complex valid email")]
    [TestCase("user123@test-domain.org", TestName = "Alphanumeric domain")]
    [TestCase("valid_user@domain.com", TestName = "Underscore in local part")]
    [DisplayName("Should pass when email format is valid")]
    public void ShouldPassValidationWhenEmailFormatIsValid(string email)
    {
        // Given: Command with valid email format
        var command = new RegisterUserCommand(email, "Password123!", "John Doe");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should not have validation error for email
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Password Validation Tests

    [TestCase("", TestName = "Empty password")]
    [TestCase("   ", TestName = "Whitespace password")]
    [TestCase(null, TestName = "Null password")]
    [DisplayName("Should fail when password is empty or null")]
    public void ShouldFailValidationWhenPasswordIsEmptyOrNull(string? password)
    {
        // Given: Command with invalid password
        var command = new RegisterUserCommand("test@example.com", password!, "John Doe");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should have validation error for password
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage(UserException.InvalidPassword);
    }

    [TestCase("1", TestName = "1 character")]
    [TestCase("12", TestName = "2 characters")]
    [TestCase("123", TestName = "3 characters")]
    [TestCase("1234", TestName = "4 characters")]
    [TestCase("12345", TestName = "5 characters")]
    [DisplayName("Should fail when password is too short")]
    public void ShouldFailValidationWhenPasswordIsTooShort(string password)
    {
        // Given: Command with short password
        var command = new RegisterUserCommand("test@example.com", password, "John Doe");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should have validation error for password
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage(UserException.InvalidPassword);
    }

    [TestCase("123456", TestName = "6 characters - minimum valid")]
    [TestCase("Password123!", TestName = "Complex password")]
    [TestCase("abcdef", TestName = "6 lowercase letters")]
    [TestCase("ABCDEF", TestName = "6 uppercase letters")]
    [TestCase("!@#$%^", TestName = "6 special characters")]
    [TestCase("VeryLongPasswordWithLotsOfCharacters123!", TestName = "Very long password")]
    [DisplayName("Should pass when password length is valid")]
    public void ShouldPassValidationWhenPasswordLengthIsValid(string password)
    {
        // Given: Command with valid password length
        var command = new RegisterUserCommand("test@example.com", password, "John Doe");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should not have validation error for password
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    #endregion

    #region FullName Validation Tests

    [TestCase("", TestName = "Empty full name")]
    [TestCase("   ", TestName = "Whitespace full name")]
    [TestCase(null, TestName = "Null full name")]
    [TestCase("\t", TestName = "Tab character")]
    [TestCase("\n", TestName = "Newline character")]
    [TestCase("\r\n", TestName = "CRLF characters")]
    [DisplayName("Should fail when full name is empty or null")]
    public void ShouldFailValidationWhenFullNameIsEmptyOrNull(string? fullName)
    {
        // Given: Command with invalid full name
        var command = new RegisterUserCommand("test@example.com", "Password123!", fullName!);

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should have validation error for full name
        result.ShouldHaveValidationErrorFor(x => x.FullName)
              .WithErrorMessage(UserException.InvalidFullName);
    }

    [TestCase("A", TestName = "1 character")]
    [TestCase("AB", TestName = "2 characters")]
    [TestCase("  A  ", TestName = "Single char with spaces")]
    [DisplayName("Should fail when full name is too short")]
    public void ShouldFailValidationWhenFullNameIsTooShort(string fullName)
    {
        // Given: Command with short full name
        var command = new RegisterUserCommand("test@example.com", "Password123!", fullName);

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should have validation error for full name
        result.ShouldHaveValidationErrorFor(x => x.FullName)
              .WithErrorMessage(UserException.InvalidFullName);
    }

    [TestCase("John Doe", TestName = "Normal name")]
    [TestCase("José María", TestName = "Accented characters")]
    [TestCase("O'Connor-Smith", TestName = "Apostrophe and hyphen")]
    [TestCase("Van der Berg", TestName = "Multiple words")]
    [TestCase("李小明", TestName = "Chinese characters")]
    [TestCase("محمد عبدالله", TestName = "Arabic characters")]
    [TestCase("ABC", TestName = "3 characters - minimum valid")]
    [DisplayName("Should pass when full name is valid")]
    public void ShouldPassValidationWhenFullNameIsValid(string fullName)
    {
        // Given: Command with valid full name
        var command = new RegisterUserCommand("test@example.com", "Password123!", fullName);

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should not have validation error for full name
        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    #endregion

    #region Multiple Field Validation Tests

    [Test]
    [DisplayName("Should fail when all fields are invalid")]
    public void ShouldFailValidationWhenAllFieldsAreInvalid()
    {
        // Given: Command with all invalid fields
        var command = new RegisterUserCommand("invalid-email", "123", "A");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should have validation errors for all fields
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage(UserException.InvalidEmail);
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage(UserException.InvalidPassword);
        result.ShouldHaveValidationErrorFor(x => x.FullName)
              .WithErrorMessage(UserException.InvalidFullName);
    }

    [Test]
    [DisplayName("Should pass when all fields are valid")]
    public void ShouldPassValidationWhenAllFieldsAreValid()
    {
        // Given: Command with all valid fields
        var command = new RegisterUserCommand("test@example.com", "Password123!", "John Doe");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should not have any validation errors
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    [DisplayName("Should fail when only email is invalid")]
    public void ShouldFailValidationWhenOnlyEmailIsInvalid()
    {
        // Given: Command with only invalid email
        var command = new RegisterUserCommand("invalid-email", "Password123!", "John Doe");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should have validation error only for email
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    [Test]
    [DisplayName("Should fail when only password is invalid")]
    public void ShouldFailValidationWhenOnlyPasswordIsInvalid()
    {
        // Given: Command with only invalid password
        var command = new RegisterUserCommand("test@example.com", "123", "John Doe");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should have validation error only for password
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    [Test]
    [DisplayName("Should fail when only full name is invalid")]
    public void ShouldFailValidationWhenOnlyFullNameIsInvalid()
    {
        // Given: Command with only invalid full name
        var command = new RegisterUserCommand("test@example.com", "Password123!", "A");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should have validation error only for full name
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    #endregion

    #region Cascade Mode Tests

    [Test]
    [DisplayName("Should stop validation on first email error when cascade mode is stop")]
    public void ShouldStopValidationOnFirstEmailErrorWhenCascadeModeIsStop()
    {
        // Given: Command with empty email (should fail NotEmpty and not continue to EmailAddress)
        var command = new RegisterUserCommand("", "Password123!", "John Doe");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should have only one error for email (due to cascade stop)
        var emailErrors = result.Errors.Where(e => e.PropertyName == nameof(RegisterUserCommand.Email));
        IEnumerable<ValidationFailure> validationFailures = emailErrors.ToList();
        Assert.That(validationFailures.Count(), Is.EqualTo(1));
        Assert.That(validationFailures.First().ErrorMessage, Is.EqualTo(UserException.InvalidEmail));
    }

    [Test]
    [DisplayName("Should stop validation on first password error when cascade mode is stop")]
    public void ShouldStopValidationOnFirstPasswordErrorWhenCascadeModeIsStop()
    {
        // Given: Command with empty password
        var command = new RegisterUserCommand("test@example.com", "", "John Doe");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should have only one error for password
        var passwordErrors = result.Errors.Where(e => e.PropertyName == nameof(RegisterUserCommand.Password));
        IEnumerable<ValidationFailure> validationFailures = passwordErrors.ToList();
        Assert.That(validationFailures.Count(), Is.EqualTo(1));
        Assert.That(validationFailures.First().ErrorMessage, Is.EqualTo(UserException.InvalidPassword));
    }

    [Test]
    [DisplayName("Should stop validation on first full name error when cascade mode is stop")]
    public void ShouldStopValidationOnFirstFullNameErrorWhenCascadeModeIsStop()
    {
        // Given: Command with empty full name
        var command = new RegisterUserCommand("test@example.com", "Password123!", "");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should have only one error for full name
        var fullNameErrors = result.Errors.Where(e => e.PropertyName == nameof(RegisterUserCommand.FullName));
        IEnumerable<ValidationFailure> validationFailures = fullNameErrors.ToList();
        Assert.That(validationFailures.Count(), Is.EqualTo(1));
        Assert.That(validationFailures.First().ErrorMessage, Is.EqualTo(UserException.InvalidFullName));
    }

    #endregion

    #region Edge Case Tests

    [Test]
    [DisplayName("Should handle very long email")]
    public void ShouldHandleVeryLongEmailWhenValidating()
    {
        // Given: Command with very long email
        var longLocalPart = new string('a', 60); // Just under the 64 character limit
        var longEmail = $"{longLocalPart}@example.com";
        var command = new RegisterUserCommand(longEmail, "Password123!", "John Doe");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should handle gracefully (pass if within email limits)
        if (longEmail.Length <= 254) // RFC limit
        {
            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }
    }

    [Test]
    [DisplayName("Should handle very long password")]
    public void ShouldHandleVeryLongPasswordWhenValidating()
    {
        // Given: Command with very long password
        var longPassword = new string('A', 1000);
        var command = new RegisterUserCommand("test@example.com", longPassword, "John Doe");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should pass validation (no upper limit on password length)
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Test]
    [DisplayName("Should handle very long full name")]
    public void ShouldHandleVeryLongFullNameWhenValidating()
    {
        // Given: Command with very long full name
        var longName = new string('A', 500);
        var command = new RegisterUserCommand("test@example.com", "Password123!", longName);

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should handle gracefully (depends on your business rules)
        // This test verifies that the validator doesn't throw exceptions
        Assert.That(() => result, Throws.Nothing);
    }

    #endregion
}
