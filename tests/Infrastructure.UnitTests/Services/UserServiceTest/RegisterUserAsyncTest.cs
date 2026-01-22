using System.ComponentModel;
using Amazon.CognitoIdentityProvider.Model;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Application.Common.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserServiceTest;

[TestFixture]
public class RegisterUserAsyncTest : GenericUserServiceTest
{

    [Test]
    [DisplayName("Should return user sub when registration is successful")]
    public async Task ShouldReturnUserSubWhenRegistrationIsSuccessful()
    {
        // Given: A valid user registration data
        const string email = "test@example.com";
        const string subId = "uuid-12345";
        SignUpResponse response = new SignUpResponse { UserSub = subId };

        _cognitoMock.Setup(x => x.SignUpAsync(It.IsAny<SignUpRequest>(), default))
            .ReturnsAsync(response);

        // When: Registering the user
        Result<string> result = await _service.RegisterUserAsync(email, "Password123!", "John Doe");

        // Then: Should return success with the sub
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(subId));
    }

    [Test]
    [DisplayName("Should return UserAlreadyExist error when username exists in Cognito")]
    public async Task ShouldReturnUserAlreadyExistWhenCognitoThrowsUsernameExists()
    {
        // Given: Cognito throws UsernameExistsException
        _cognitoMock.Setup(x => x.SignUpAsync(It.IsAny<SignUpRequest>(), default))
            .ThrowsAsync(new UsernameExistsException("User exists"));

        // When: Registering the user
        Result<string> result = await _service.RegisterUserAsync("exists@test.com", "Pass123!", "Name");

        // Then: Should return the specific domain error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserAlreadyExist));
    }

    [Test]
    [DisplayName("Should return InvalidPassword error when password policy fails")]
    public async Task ShouldReturnInvalidPasswordWhenCognitoThrowsInvalidPassword()
    {
        // Given: Cognito throws InvalidPasswordException
        _cognitoMock.Setup(x => x.SignUpAsync(It.IsAny<SignUpRequest>(), default))
            .ThrowsAsync(new InvalidPasswordException("Weak password"));

        // When: Registering
        Result<string> result = await _service.RegisterUserAsync("test@test.com", "123", "Name");

        // Then: Should return InvalidPassword error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPassword));
    }

    [Test]
    [DisplayName("Should return UnexpectedError when an unhandled exception occurs")]
    public async Task ShouldReturnUnexpectedErrorWhenGeneralExceptionOccurs()
    {
        // Given: A non-Cognito specific exception
        _cognitoMock.Setup(x => x.SignUpAsync(It.IsAny<SignUpRequest>(), default))
            .ThrowsAsync(new Exception("Network fail"));

        // When: Registering
        Result<string> result = await _service.RegisterUserAsync("test@test.com", "Pass123!", "Name");

        // Then: Should return UnexpectedError
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UnexpectedError));
    }
}
