using System.ComponentModel;
using Amazon.CognitoIdentityProvider.Model;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Application.Common.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserServiceTest;

[TestFixture]
public class GetUserIDTests : GenericUserServiceTest
{
    [Test]
    [DisplayName("Should return user id when user exists")]
    public async Task ShouldReturnUserIdWhenUserExists()
    {
        // Given
        const string email = "user@test.com";
        const string expectedSub = "sub-123";

        AdminGetUserResponse response = new()
        {
            UserAttributes =
            [
                new AttributeType { Name = "email", Value = email },
                new AttributeType { Name = "sub", Value = expectedSub }
            ]
        };

        _cognitoMock
            .Setup(x => x.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), CancellationToken.None))
            .ReturnsAsync(response);

        // When
        Result<string> result = await _service.GetUserID(email, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedSub));

        _cognitoMock.Verify(x => x.AdminGetUserAsync(
            It.Is<AdminGetUserRequest>(r => r.Username == email && r.UserPoolId == _configMock.UserPoolId),
            default), Times.Once);
    }

    [Test]
    [DisplayName("Should return UserNotFound when Cognito throws UserNotFoundException")]
    public async Task ShouldReturnUserNotFoundWhenCognitoThrowsUserNotFoundException()
    {
        _cognitoMock
            .Setup(x => x.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), default))
            .ThrowsAsync(new UserNotFoundException("User not found"));

        Result<string> result = await _service.GetUserID("missing@test.com", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));
    }

    [Test]
    [DisplayName("Should return InvalidForm when Cognito throws InvalidParameterException")]
    public async Task ShouldReturnInvalidFormWhenCognitoThrowsInvalidParameterException()
    {
        _cognitoMock
            .Setup(x => x.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), default))
            .ThrowsAsync(new InvalidParameterException("Invalid username"));

        Result<string> result = await _service.GetUserID("", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidForm));
    }

    [Test]
    [DisplayName("Should return UnexpectedError when response has no sub attribute")]
    public async Task ShouldReturnUnexpectedErrorWhenResponseHasNoSubAttribute()
    {
        AdminGetUserResponse response = new()
        {
            UserAttributes =
            [
                new AttributeType { Name = "email", Value = "user@test.com" }
            ]
        };

        _cognitoMock
            .Setup(x => x.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), default))
            .ReturnsAsync(response);

        Result<string> result = await _service.GetUserID("user@test.com", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UnexpectedError));
    }

    [Test]
    [DisplayName("Should return UnexpectedError when a general exception occurs")]
    public async Task ShouldReturnUnexpectedErrorWhenGeneralExceptionOccurs()
    {
        _cognitoMock
            .Setup(x => x.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), default))
            .ThrowsAsync(new Exception("AWS is down"));

        Result<string> result = await _service.GetUserID("user@test.com", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UnexpectedError));
    }
}
