using System.ComponentModel;
using Amazon.CognitoIdentityProvider.Model;
using MediatR;
using Moq;
using VibraHeka.Application.Common.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserServiceTest;

[TestFixture]
public class ConfirmUserAsync : GenericUserServiceTest
{
    [Test]
    [DisplayName("Should return success when confirmation code is correct")]
    public async Task ShouldReturnSuccessWhenCodeIsCorrect()
    {
        // Given: A valid email and confirmation code
        const string email = "user@test.com";
        const string code = "123456";

        _cognitoMock.Setup(x => x.ConfirmSignUpAsync(It.IsAny<ConfirmSignUpRequest>(), default))
            .ReturnsAsync(new ConfirmSignUpResponse());

        // When: Confirming the user
        var result = await _service.ConfirmUserAsync(email, code);

        // Then: Should return success Unit
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(Unit.Value));
    }

    [Test]
    [DisplayName("Should return WrongVerificationCode error when code mismatch")]
    public async Task ShouldReturnWrongVerificationCodeWhenCognitoThrowsCodeMismatch()
    {
        // Given: Cognito throws CodeMismatchException
        _cognitoMock.Setup(x => x.ConfirmSignUpAsync(It.IsAny<ConfirmSignUpRequest>(), default))
            .ThrowsAsync(new CodeMismatchException("Invalid code"));

        // When: Confirming
        var result = await _service.ConfirmUserAsync("user@test.com", "000000");

        // Then: Should return WrongVerificationCode domain error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.WrongVerificationCode));
    }

    [Test]
    [DisplayName("Should return ExpiredCode error when code has expired")]
    public async Task ShouldReturnExpiredCodeWhenCognitoThrowsExpiredCode()
    {
        // Given: Cognito throws ExpiredCodeException
        _cognitoMock.Setup(x => x.ConfirmSignUpAsync(It.IsAny<ConfirmSignUpRequest>(), default))
            .ThrowsAsync(new ExpiredCodeException("Code expired"));

        // When: Confirming
        var result = await _service.ConfirmUserAsync("user@test.com", "111111");

        // Then: Should return ExpiredCode domain error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.ExpiredCode));
    }

    [Test]
    [DisplayName("Should return TooManyAttempts error when attempts limit exceeded")]
    public async Task ShouldReturnTooManyAttemptsWhenCognitoThrowsTooManyFailedAttempts()
    {
        // Given: Cognito throws TooManyFailedAttemptsException
        _cognitoMock.Setup(x => x.ConfirmSignUpAsync(It.IsAny<ConfirmSignUpRequest>(), default))
            .ThrowsAsync(new TooManyFailedAttemptsException("Too many tries"));

        // When: Confirming
        var result = await _service.ConfirmUserAsync("user@test.com", "222222");

        // Then: Should return TooManyAttempts domain error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.TooManyAttempts));
    }

    [Test]
    [DisplayName("Should return UserNotFound error when email is not registered")]
    public async Task ShouldReturnUserNotFoundWhenCognitoThrowsUserNotFound()
    {
        // Given: Cognito throws UserNotFoundException
        _cognitoMock.Setup(x => x.ConfirmSignUpAsync(It.IsAny<ConfirmSignUpRequest>(), default))
            .ThrowsAsync(new UserNotFoundException("User not found"));

        // When: Confirming
        var result = await _service.ConfirmUserAsync("none@test.com", "123456");

        // Then: Should return UserNotFound domain error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));
    }

    [Test]
    [DisplayName("Should return UnknownError when an unhandled exception occurs")]
    public async Task ShouldReturnUnknownErrorWhenGeneralExceptionOccurs()
    {
        // Given: A random network or server exception
        _cognitoMock.Setup(x => x.ConfirmSignUpAsync(It.IsAny<ConfirmSignUpRequest>(), default))
            .ThrowsAsync(new Exception("AWS is having issues"));

        // When: Confirming
        var result = await _service.ConfirmUserAsync("user@test.com", "123456");

        // Then: Should return AppErrors.UnknownError as defined in your catch
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AppErrors.UnknownError));
    }
}
