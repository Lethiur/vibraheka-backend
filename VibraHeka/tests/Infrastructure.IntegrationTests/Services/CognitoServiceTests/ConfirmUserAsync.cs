using System.ComponentModel;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.CognitoServiceTests;

[TestFixture]
public class CognitoServiceConfirmUserTests : GenericCognitoServiceTest
{
    #region ConfirmUserAsync - Success Cases

    [Test]
    [DisplayName("Should successfully confirm user with valid code")]
    public async Task ShouldConfirmUserSuccessfullyWhenValidCodeProvided()
    {
        // Given: A registered user (this will be UNCONFIRMED status)
        string email = GenerateUniqueEmail();
        await RegisterUser(email);

        // When: The user is confirmed
        Result<VerificationCodeEntity> codeFor = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));
    
        // Then: The user should be confirmed successfully
        Result<Unit> confirmResult = await _cognitoService.ConfirmUserAsync(email, codeFor.Value.Code);
        Assert.That(confirmResult.IsSuccess, Is.True, "User confirmation should succeed");
    }

    #endregion

    #region ConfirmUserAsync - User Not Found Cases

    [Test]
    [DisplayName("Should fail when user does not exist")]
    public async Task ShouldReturnUserNotFoundWhenUserDoesNotExist()
    {
        // Given: A non-existent user email
        string nonExistentEmail = $"nonexistent-{Guid.NewGuid()}@example.com";
        string confirmationCode = "123456";

        // When: Trying to confirm non-existent user
        Result<Unit> result = await _cognitoService.ConfirmUserAsync(nonExistentEmail, confirmationCode);

        // Then: Should fail with UserNotFound error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserException.UserNotFound));
    }

    [Test]
    [DisplayName("Should fail when email format is invalid")]
    public async Task ShouldReturnUserNotFoundWhenEmailFormatIsInvalid()
    {
        // Given: Invalid email format
        string invalidEmail = "invalid-email-format";
        string confirmationCode = "123456";

        // When: Trying to confirm with invalid email
        Result<Unit> result = await _cognitoService.ConfirmUserAsync(invalidEmail, confirmationCode);

        // Then: Should fail with UserNotFound error (Cognito treats malformed emails as not found)
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserException.UserNotFound));
    }

    #endregion

    #region ConfirmUserAsync - Wrong Code Cases

    [Test]
    [DisplayName("Should fail when confirmation code is wrong")]
    public async Task ShouldReturnWrongVerificationCodeWhenConfirmationCodeIsWrong()
    {
        // Given: A registered user
        string email = GenerateUniqueEmail();
        await RegisterUser(email);

        // When: Confirming with wrong code
        Result<Unit> result = await _cognitoService.ConfirmUserAsync(email, "000000");

        // Then: Should fail with WrongVerificationCode error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserException.WrongVerificationCode));
    }

    [TestCase("12345", TestName = "Code too short")]
    [TestCase("1234567", TestName = "Code too long")]
    [TestCase("ABCDEF", TestName = "Alphabetic code")]
    [TestCase("12345A", TestName = "Mixed alphanumeric code")]
    [DisplayName("Should fail when confirmation code format is invalid")]
    public async Task ShouldReturnWrongVerificationCodeWhenCodeFormatIsInvalid(string invalidCode)
    {
        // Given: A registered user
        string email = GenerateUniqueEmail();
        await RegisterUser(email);
    
        // When: Confirming with invalid code format
        Result<Unit> result = await _cognitoService.ConfirmUserAsync(email, invalidCode);

        // Then: Should fail with WrongVerificationCode error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserException.WrongVerificationCode));
    }

    #endregion

    #region ConfirmUserAsync - Empty/Null Parameters

    [TestCase("", TestName = "Empty email")]
    [TestCase(null, TestName = "Null email")]
    [TestCase("   ", TestName = "Whitespace email")]
    [DisplayName("Should fail when email is empty, null or whitespace")]
    public async Task ShouldReturnInvalidFormWhenEmailIsInvalid(string? email)
    {
        // When: Trying to confirm with invalid email
        Result<Unit> result = await _cognitoService.ConfirmUserAsync(email!, "123456");

        // Then: Should fail with InvalidForm (Cognito treats empty/invalid emails as invalid parameters)
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserException.InvalidForm));
    }

    [Test]
    [DisplayName("Should fail when code is not numeric")]
    public async Task ShouldReturnWrongVerificationCodeWhenCodeIsNotNumeric()
    {
        // Given: A registered user
        string email = GenerateUniqueEmail();
        await RegisterUser(email);
        
        // When: Trying to confirm with non-numeric code
        Result<Unit> result = await _cognitoService.ConfirmUserAsync(email, "testes");

        // Then: Should fail with WrongVerificationCode
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserException.WrongVerificationCode));
    }

    [TestCase("", TestName = "Empty code")]
    [TestCase(null, TestName = "Null code")]
    [TestCase("   ", TestName = "Whitespace code")]
    [DisplayName("Should fail when confirmation code is empty, null or whitespace")]
    public async Task ShouldReturnInvalidFormWhenCodeIsInvalid(string? code)
    {
        // When: Trying to confirm with invalid code
        Result<Unit> result = await _cognitoService.ConfirmUserAsync("test@example.com", code!);

        // Then: Should fail with InvalidForm
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserException.InvalidForm));
    }

    #endregion
    
    #region ConfirmUserAsync - Wrong Code Cases

    [Test]
    [DisplayName("Should fail when code format is valid but code is incorrect")]
    public async Task ShouldReturnWrongVerificationCodeWhenValidFormatCodeIsIncorrect()
    {
        // Given: A registered user in Cognito
        string email = GenerateUniqueEmail();
        await RegisterUser(email);

        // When: We send a 6-digit numeric code that isn't the one Cognito generated
        // This will trigger CodeMismatchException in the Cognito SDK
        Result<Unit> result = await _cognitoService.ConfirmUserAsync(email, "999999");

        // Then: The service should catch the AWS exception and return our domain error
        Assert.That(result.IsFailure, Is.True, "The operation should fail");
        Assert.That(result.Error, Is.EqualTo(UserException.WrongVerificationCode), 
            "Should return WrongVerificationCode when the code does not match AWS records");
    }
    
    #endregion

    #region ConfirmUserAsync - Already Confirmed User

    [Test]
    [DisplayName("Should fail when trying to confirm already confirmed user")]
    public async Task ShouldReturnNotAuthorizedWhenUserAlreadyConfirmed()
    {
        // Given: A user that will be already confirmed
        string email = GenerateUniqueEmail();
        await RegisterUser(email);

        // When: The user is confirmed twice
        Result<VerificationCodeEntity> codeFor = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));
        await _cognitoService.ConfirmUserAsync(email, codeFor.Value.Code);
        Result<Unit> secondConfirmResult = await _cognitoService.ConfirmUserAsync(email, codeFor.Value.Code);
    
        // Then: The error should be NotAuthorized (Cognito does not allow confirming already confirmed users)
        Assert.That(secondConfirmResult.IsFailure, Is.True);
        Assert.That(secondConfirmResult.Error, Is.EqualTo(UserException.NotAuthorized));
    }

    #endregion

    #region ConfirmUserAsync - Rate Limiting

    [Test]
    [DisplayName("Should handle too many failed attempts")]
    public async Task ShouldHandleRateLimitingWhenTooManyFailedAttempts()
    {
        // Given: A registered user
        string email = GenerateUniqueEmail();
        await RegisterUser(email);

        // When: Making multiple failed confirmation attempts
        List<Task<Result<Unit>>> attempts = new();

        for (int i = 0; i < 6; i++) // Exceed the typical limit
        {
            attempts.Add(_cognitoService.ConfirmUserAsync(email, $"00000{i}"));
        }

        Result<Unit>[] results = await Task.WhenAll(attempts);

        // Then: Should have at least wrong code errors (rate limiting might not trigger immediately)
        bool hasWrongCodeError = results.Any(r => r.IsFailure && r.Error == UserException.WrongVerificationCode);
        Assert.That(hasWrongCodeError, Is.True, "Should have at least one wrong code error");
    }

    #endregion

    #region ConfirmUserAsync - Concurrent Operations

    [Test]
    [DisplayName("Should handle concurrent confirmation attempts")]
    public async Task ShouldHandleConsistentBehaviorWhenConcurrentAttempts()
    {
        // Given: A registered user
        string email = GenerateUniqueEmail();
        await RegisterUser(email);
    
        // When: Making concurrent confirmation attempts
        List<Task<Result<Unit>>> tasks = new();

        for (int i = 0; i < 3; i++)
        {
            tasks.Add(_cognitoService.ConfirmUserAsync(email, "123456"));
        }

        Result<Unit>[] results = await Task.WhenAll(tasks);

        // Then: All should fail with wrong code (consistent behavior)
        foreach (Result<Unit> result in results)
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserException.WrongVerificationCode));
        }
    }

    #endregion
}
