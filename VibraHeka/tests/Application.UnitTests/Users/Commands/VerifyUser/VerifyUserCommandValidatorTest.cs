using System.ComponentModel;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.VerificationCode;

namespace VibraHeka.Application.UnitTests.Users.Commands.VerifyUser;

[TestFixture]
public class VerifyUserCommandValidatorTests
{
    private VerifyUserCommandValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new VerifyUserCommandValidator();
    }

    #region Email Validation Tests

    [TestCase("", TestName = "Empty email")]
    [TestCase("   ", TestName = "Whitespace email")]
    [TestCase(null, TestName = "Null email")]
    [DisplayName("Should fail when email is empty or null")]
    public void ShouldFailValidationWhenEmailIsEmptyOrNull(string? email)
    {
        // Given: Command with invalid email
        VerifyUserCommand command = new VerifyUserCommand(email!, "123456");

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

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
    [TestCase("user@domain.", TestName = "Empty TLD")]
    [TestCase("user@.domain.com", TestName = "Starting with dot")]
    [DisplayName("Should fail when email format is invalid")]
    public void ShouldFailValidationWhenEmailFormatIsInvalid(string email)
    {
        // Given: Command with invalid email format
        VerifyUserCommand command = new VerifyUserCommand(email, "123456");

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should have validation error for email
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage(UserException.InvalidEmail);
    }

    [TestCase("user@example.com", TestName = "Basic valid email")]
    [TestCase("test.user+tag@domain.co.uk", TestName = "Complex valid email")]
    [TestCase("user123@test-domain.org", TestName = "Alphanumeric domain")]
    [TestCase("valid_user@domain.com", TestName = "Underscore in local part")]
    [TestCase("user@sub.domain.com", TestName = "Subdomain")]
    [TestCase("test@domain-name.info", TestName = "Hyphenated domain")]
    [DisplayName("Should pass when email format is valid")]
    public void ShouldPassValidationWhenEmailFormatIsValid(string email)
    {
        // Given: Command with valid email format
        VerifyUserCommand command = new VerifyUserCommand(email, "123456");

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should not have validation error for email
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Code Validation Tests

    [TestCase("", TestName = "Empty code")]
    [TestCase("   ", TestName = "Whitespace code")]
    [TestCase(null, TestName = "Null code")]
    [TestCase("\t", TestName = "Tab character")]
    [TestCase("\n", TestName = "Newline character")]
    [TestCase("\r\n", TestName = "CRLF characters")]
    [DisplayName("Should fail when verification code is empty or null")]
    public void ShouldFailValidationWhenCodeIsEmptyOrNull(string? code)
    {
        // Given: Command with invalid code
        VerifyUserCommand command = new VerifyUserCommand("test@example.com", code!);

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should have validation error for code
        result.ShouldHaveValidationErrorFor(x => x.Code)
              .WithErrorMessage(UserException.InvalidVerificationCode);
    }

    [TestCase("1", TestName = "1 character")]
    [TestCase("12", TestName = "2 characters")]
    [TestCase("123", TestName = "3 characters")]
    [TestCase("1234", TestName = "4 characters")]
    [TestCase("12345", TestName = "5 characters")]
    [DisplayName("Should fail when verification code is too short")]
    public void ShouldFailValidationWhenCodeIsTooShort(string code)
    {
        // Given: Command with short code
        VerifyUserCommand command = new VerifyUserCommand("test@example.com", code);

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should have validation error for code
        result.ShouldHaveValidationErrorFor(x => x.Code)
              .WithErrorMessage(UserException.InvalidVerificationCode);
    }

    [TestCase("123456", TestName = "6-digit numeric code")]
    [TestCase("ABCDEF", TestName = "6-character alphabetic code")]
    [TestCase("abc123", TestName = "6-character alphanumeric lowercase")]
    [TestCase("ABC123", TestName = "6-character alphanumeric uppercase")]
    [TestCase("A1B2C3", TestName = "6-character mixed case alphanumeric")]
    [TestCase("1234567", TestName = "7-character code")]
    [TestCase("12345678", TestName = "8-character code")]
    [TestCase("1234567890", TestName = "10-character code")]
    [TestCase("!@#$%^", TestName = "6 special characters")]
    [TestCase("123ABC!@#", TestName = "9-character mixed with special")]
    [DisplayName("Should pass when verification code length is valid")]
    public void ShouldPassValidationWhenCodeLengthIsValid(string code)
    {
        // Given: Command with valid code length
        VerifyUserCommand command = new VerifyUserCommand("test@example.com", code);

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should not have validation error for code
        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    #endregion

    #region Multiple Field Validation Tests

    [Test]
    [DisplayName("Should fail when both email and code are invalid")]
    public void ShouldFailValidationWhenBothEmailAndCodeAreInvalid()
    {
        // Given: Command with both invalid fields
        VerifyUserCommand command = new VerifyUserCommand("invalid-email", "123");

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should have validation errors for both fields
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage(UserException.InvalidEmail);
        result.ShouldHaveValidationErrorFor(x => x.Code)
              .WithErrorMessage(UserException.InvalidVerificationCode);
    }

    [Test]
    [DisplayName("Should pass when both email and code are valid")]
    public void ShouldPassValidationWhenBothEmailAndCodeAreValid()
    {
        // Given: Command with all valid fields
        VerifyUserCommand command = new VerifyUserCommand("test@example.com", "123456");

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should not have any validation errors
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    [DisplayName("Should fail when only email is invalid")]
    public void ShouldFailValidationWhenOnlyEmailIsInvalid()
    {
        // Given: Command with only invalid email
        VerifyUserCommand command = new VerifyUserCommand("invalid-email", "123456");

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should have validation error only for email
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    [Test]
    [DisplayName("Should fail when only code is invalid")]
    public void ShouldFailValidationWhenOnlyCodeIsInvalid()
    {
        // Given: Command with only invalid code
        VerifyUserCommand command = new VerifyUserCommand("test@example.com", "123");

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should have validation error only for code
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [TestCase("", "", TestName = "Both fields empty")]
    [TestCase(null, null, TestName = "Both fields null")]
    [DisplayName("Should fail when both fields are empty or null")]
    public void ShouldFailValidationWhenBothFieldsAreEmptyOrNull(string? email, string? code)
    {
        // Given: Command with both invalid fields
        VerifyUserCommand command = new VerifyUserCommand(email!, code!);

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should have validation errors for both fields
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Code);
        
        // And: Should have exactly 2 errors
        Assert.That(result.Errors.Count, Is.EqualTo(2));
    }

    #endregion

    #region Cascade Mode Tests

    [Test]
    [DisplayName("Should stop validation on first email error when cascade mode is stop")]
    public void ShouldStopValidationOnFirstEmailErrorWhenCascadeModeIsStop()
    {
        // Given: Command with empty email (should fail NotEmpty and not continue to EmailAddress)
        VerifyUserCommand command = new VerifyUserCommand("", "123456");

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should have only one error for email (due to cascade stop)
        IEnumerable<ValidationFailure> emailErrors = result.Errors.Where(e => e.PropertyName == nameof(VerifyUserCommand.Email));
        IEnumerable<ValidationFailure> validationFailures = emailErrors.ToList();
        Assert.That(validationFailures.Count(), Is.EqualTo(1));
        Assert.That(validationFailures.First().ErrorMessage, Is.EqualTo(UserException.InvalidEmail));
    }

    [Test]
    [DisplayName("Should stop validation on first code error when cascade mode is stop")]
    public void ShouldStopValidationOnFirstCodeErrorWhenCascadeModeIsStop()
    {
        // Given: Command with empty code
        VerifyUserCommand command = new VerifyUserCommand("test@example.com", "");

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should have only one error for code
        IEnumerable<ValidationFailure> codeErrors = result.Errors.Where(e => e.PropertyName == nameof(VerifyUserCommand.Code));
        IEnumerable<ValidationFailure> validationFailures = codeErrors.ToList();
        Assert.That(validationFailures.Count(), Is.EqualTo(1));
        Assert.That(validationFailures.First().ErrorMessage, Is.EqualTo(UserException.InvalidVerificationCode));
    }

    [Test]
    [DisplayName("Should validate all fields independently with cascade stop")]
    public void ShouldValidateAllFieldsIndependentlyWhenCascadeModeIsStop()
    {
        // Given: Command with invalid email format (should pass NotEmpty but fail EmailAddress)
        VerifyUserCommand command = new VerifyUserCommand("invalid-email-format", "123456");

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should fail on email format validation
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Code);
        
        // And: Should have exactly one error (cascade stopped after format validation)
        IEnumerable<ValidationFailure> emailErrors = result.Errors.Where(e => e.PropertyName == nameof(VerifyUserCommand.Email));
        Assert.That(emailErrors.Count(), Is.EqualTo(1));
    }

    #endregion

    #region Edge Case Tests

    [Test]
    [DisplayName("Should handle very long email")]
    public void ShouldHandleVeryLongEmailWhenValidating()
    {
        // Given: Command with very long email
        string longLocalPart = new string('a', 60); // Just under the 64 character limit
        string longEmail = $"{longLocalPart}@example.com";
        VerifyUserCommand command = new VerifyUserCommand(longEmail, "123456");

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should handle gracefully (pass if within email limits)
        if (longEmail.Length <= 254) // RFC limit
        {
            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }
    }

    [Test]
    [DisplayName("Should handle very long verification code")]
    public void ShouldHandleVeryLongCodeWhenValidating()
    {
        // Given: Command with very long verification code
        string longCode = new string('1', 1000);
        VerifyUserCommand command = new VerifyUserCommand("test@example.com", longCode);

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should pass validation (no upper limit on code length)
        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    [TestCase(254, true, TestName = "Email at 254 chars should pass")]
    [TestCase(255, false, TestName = "Email at 255 chars should fail")]
    [TestCase(300, false, TestName = "Email at 300 chars should fail")]
    [DisplayName("Should validate email length according to RFC limits")]
    public void ShouldValidateEmailLengthAccordingToRfcLimits(int emailLength, bool shouldPass)
    {
        // Given: Email of specific length
        var totalDomainLength = emailLength - 65; // 64 (local) + 1 (@)
        var domainName = totalDomainLength > 4 
            ? new string('b', totalDomainLength - 4) + ".com"
            : "b.co";
    
        var email = $"{new string('a', 64)}@{domainName}";
        var command = new VerifyUserCommand(email, "123456");

        // When: Validating the command
        var result = _validator.TestValidate(command);

        // Then: Should pass or fail based on expectation
        if (shouldPass)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }
    }

    [TestCase("test@xn--domain.com", TestName = "Punycode domain")]
    [TestCase("user@example.org", TestName = "Standard international TLD")]
    [TestCase("test@domain.co.uk", TestName = "Multi-level TLD")]
    [DisplayName("Should handle international domain names")]
    public void ShouldHandleInternationalDomainNamesWhenValidating(string email)
    {
        // Given: Command with international email
        VerifyUserCommand command = new VerifyUserCommand(email, "123456");

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should handle gracefully
        Assert.That(() => result, Throws.Nothing);
    }

    #endregion

    #region Boundary Value Tests

    [TestCase("123456", true, TestName = "Exactly 6 characters - should pass")]
    [TestCase("12345", false, TestName = "Exactly 5 characters - should fail")]
    [DisplayName("Should handle boundary values for code length")]
    public void ShouldHandleBoundaryValuesForCodeLengthWhenValidating(string code, bool shouldPass)
    {
        // Given: Command with boundary value code
        VerifyUserCommand command = new VerifyUserCommand("test@example.com", code);

        // When: Validating the command
        TestValidationResult<VerifyUserCommand>? result = _validator.TestValidate(command);

        // Then: Should pass or fail based on expected result
        if (shouldPass)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.Code);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.Code)
                  .WithErrorMessage(UserException.InvalidVerificationCode);
        }
    }

    #endregion
}
