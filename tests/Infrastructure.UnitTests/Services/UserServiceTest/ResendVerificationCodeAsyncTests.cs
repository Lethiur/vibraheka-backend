using System.ComponentModel;
using Amazon.CognitoIdentityProvider.Model;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Application.Common.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserServiceTest;

[TestFixture]
public class ResendVerificationCodeAsyncTests : GenericUserServiceTest
{
    [Test]
    [DisplayName("Should return success when Cognito resends confirmation code")]
    public async Task ShouldReturnSuccessWhenCognitoResendsConfirmationCode()
    {
        // Given
        const string email = "user@test.com";

        _cognitoMock
            .Setup(x => x.ResendConfirmationCodeAsync(It.IsAny<ResendConfirmationCodeRequest>(), default))
            .ReturnsAsync(new ResendConfirmationCodeResponse());

        // When
        Result<Unit> result = await _service.ResendVerificationCodeAsync(email);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(Unit.Value));

        _cognitoMock.Verify(x => x.ResendConfirmationCodeAsync(
            It.Is<ResendConfirmationCodeRequest>(r => r.Username == email && r.ClientId == _configMock.ClientId),
            default), Times.Once);
    }

    [Test]
    [DisplayName("Should return UserNotFound when Cognito throws UserNotFoundException")]
    public async Task ShouldReturnUserNotFoundWhenCognitoThrowsUserNotFoundException()
    {
        _cognitoMock
            .Setup(x => x.ResendConfirmationCodeAsync(It.IsAny<ResendConfirmationCodeRequest>(), default))
            .ThrowsAsync(new UserNotFoundException("User not found"));

        Result<Unit> result = await _service.ResendVerificationCodeAsync("missing@test.com");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));
    }

    [Test]
    [DisplayName("Should return InvalidForm when Cognito throws InvalidParameterException")]
    public async Task ShouldReturnInvalidFormWhenCognitoThrowsInvalidParameterException()
    {
        _cognitoMock
            .Setup(x => x.ResendConfirmationCodeAsync(It.IsAny<ResendConfirmationCodeRequest>(), default))
            .ThrowsAsync(new InvalidParameterException("Invalid username"));

        Result<Unit> result = await _service.ResendVerificationCodeAsync("");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidForm));
    }

    [Test]
    [DisplayName("Should return UserNotConfirmed when Cognito throws UserNotConfirmedException")]
    public async Task ShouldReturnUserNotConfirmedWhenCognitoThrowsUserNotConfirmedException()
    {
        _cognitoMock
            .Setup(x => x.ResendConfirmationCodeAsync(It.IsAny<ResendConfirmationCodeRequest>(), default))
            .ThrowsAsync(new UserNotConfirmedException("User not confirmed"));

        Result<Unit> result = await _service.ResendVerificationCodeAsync("pending@test.com");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotConfirmed));
    }

    [Test]
    [DisplayName("Should return LimitExceeded when Cognito throws LimitExceededException")]
    public async Task ShouldReturnLimitExceededWhenCognitoThrowsLimitExceededException()
    {
        _cognitoMock
            .Setup(x => x.ResendConfirmationCodeAsync(It.IsAny<ResendConfirmationCodeRequest>(), default))
            .ThrowsAsync(new LimitExceededException("Limit exceeded"));

        Result<Unit> result = await _service.ResendVerificationCodeAsync("user@test.com");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.LimitExceeded));
    }

    [Test]
    [DisplayName("Should return UnexpectedError when a general exception occurs")]
    public async Task ShouldReturnUnexpectedErrorWhenGeneralExceptionOccurs()
    {
        _cognitoMock
            .Setup(x => x.ResendConfirmationCodeAsync(It.IsAny<ResendConfirmationCodeRequest>(), default))
            .ThrowsAsync(new Exception("AWS is down"));

        Result<Unit> result = await _service.ResendVerificationCodeAsync("user@test.com");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UnexpectedError));
    }
}

