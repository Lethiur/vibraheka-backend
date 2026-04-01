using FluentValidation.Results;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.RefreshToken;

namespace VibraHeka.Application.FunctionalTests.Users.Commands.RefreshTokenCommandTest;

[TestFixture]
public class RefreshTokenCommandValidatorTest
{
    private const string ValidEmail = "user@test.com";
    private const string ValidRefreshToken = "eyJraWQiOiJtb2NrLXJlZnJlc2gtdG9rZW4iLCJhbGciOiJSUzI1NiJ9.eyJzdWIiOiI2YzQ5Y2M4Yi1hYWEwLTQ0YzUtOWQzYi0xMjM0NTY3ODkwYWIiLCJ0b2tlbl91c2UiOiJyZWZyZXNoIiwiYXV0aF90aW1lIjoxNzExODg4ODAwLCJpc3MiOiJodHRwczovL2NvZ25pdG8taWRwLmV1LXdlc3QtMS5hbWF6b25hd3MuY29tL2V1LXdlc3QtMV9Nb2NrUG9vbCIsImV4cCI6MTc0MzQyNDgwMCwiaWF0IjoxNzExODg4ODAwLCJjbGllbnRfaWQiOiI0b2NrY2xpZW50aWQxMjM0NTYifQ.mock-signature-for-testing-only";

    private RefreshTokenCommandValidator Validator = new();

    [SetUp]
    public void SetUp()
    {
        Validator = new RefreshTokenCommandValidator();
    }

    [TestCase("", Description = "Empty email")]
    [TestCase(null!, Description = "Null email")]
    [TestCase("invalid-email", Description = "Invalid email format")]
    [Description("Given an invalid email, when validating, then it should fail with InvalidEmail error")]
    public async Task ShouldHandleInvalidEmailCases(string? email)
    {
        // Given: A command
        RefreshTokenCommand command = new(ValidRefreshToken, email!);

        // When : The validator is triggered
        ValidationResult result = await Validator.ValidateAsync(command);

        // Then : Then the validation should fail in the email field
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.ErrorMessage == UserErrors.InvalidEmail));
    }

    [TestCase("", Description = "Empty refresh token")]
    [TestCase(null!, Description = "Null refresh token")]
    [TestCase("short-token", Description = "Refresh token shorter than minimum length")]
    [TestCase("token with spaces inside", Description = "Refresh token with whitespace")]
    [TestCase("token-with-asterisk-*", Description = "Refresh token with invalid characters")]
    [Description("Given an invalid refresh token, when validating, then it should fail on RefreshToken")]
    public async Task ShouldFailValidationWhenRefreshTokenIsInvalid(string? refreshToken)
    {
        // Given: A command
        RefreshTokenCommand command = new(refreshToken!, ValidEmail);

        // When: The validator is triggered
        ValidationResult result = await Validator.ValidateAsync(command);

        // Then: Then the validation should fail in the refresh token field
        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors,
            Has.Some.Matches<ValidationFailure>(e => e.PropertyName == nameof(RefreshTokenCommand.RefreshToken)));
    }

    [Test]
    [Description("Given a valid refresh token and email, when validating, then it should pass")]
    public async Task ShouldPassValidationWhenRefreshTokenAndEmailAreValid()
    {
        // Given: A valid command
        RefreshTokenCommand command = new(ValidRefreshToken, ValidEmail);

        // When: The validator is triggered
        ValidationResult result = await Validator.ValidateAsync(command);

        // Then: Then the validation should pass
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }
}
