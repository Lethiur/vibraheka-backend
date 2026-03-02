using System.ComponentModel;
using Amazon.CognitoIdentityProvider.Model;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Application.Common.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserServiceTest;

[TestFixture]
public class ChangePasswordAsyncTest : GenericUserServiceTest
{
    [Test]
    [DisplayName("Should return success when Cognito changes password")]
    public async Task ShouldReturnSuccessWhenCognitoChangesPassword()
    {
        // Given: a valid authenticated context for password change.
        const string accessToken = "access-token";
        const string currentPassword = "Current123!";
        const string newPassword = "NewPassword123!";

        _cognitoMock.Setup(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChangePasswordResponse());

        // When: requesting password change through the service.
        Result<Unit> result = await _service.ChangePasswordAsync(accessToken, currentPassword, newPassword, CancellationToken.None);

        // Then: operation should succeed.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(Unit.Value));
        _cognitoMock.Verify(x => x.ChangePasswordAsync(
                It.Is<ChangePasswordRequest>(request =>
                    request.AccessToken == accessToken &&
                    request.PreviousPassword == currentPassword &&
                    request.ProposedPassword == newPassword),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    [DisplayName("Should return NotAuthorized when previous password is invalid")]
    public async Task ShouldReturnNotAuthorizedWhenPreviousPasswordIsInvalid()
    {
        // Given: Cognito rejects password change because credentials are invalid.
        _cognitoMock.Setup(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotAuthorizedException("Incorrect username or password."));

        // When: requesting password change.
        Result<Unit> result = await _service.ChangePasswordAsync("access-token", "Wrong123!", "NewPassword123!", CancellationToken.None);

        // Then: domain should map to NotAuthorized.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.NotAuthorized));
    }

    [Test]
    [DisplayName("Should return InvalidPassword when proposed password is invalid")]
    public async Task ShouldReturnInvalidPasswordWhenProposedPasswordIsInvalid()
    {
        // Given: Cognito rejects password policy validation.
        _cognitoMock.Setup(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidPasswordException("Password does not conform to policy."));

        // When: requesting password change.
        Result<Unit> result = await _service.ChangePasswordAsync("access-token", "Current123!", "weak", CancellationToken.None);

        // Then: domain should map to InvalidPassword.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPassword));
    }

    [Test]
    [DisplayName("Should return UnexpectedError when Cognito throws an unknown exception")]
    public async Task ShouldReturnUnexpectedErrorWhenCognitoThrowsUnknownException()
    {
        // Given: Cognito throws an unexpected exception.
        _cognitoMock.Setup(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected failure"));

        // When: requesting password change.
        Result<Unit> result = await _service.ChangePasswordAsync("access-token", "Current123!", "NewPassword123!", CancellationToken.None);

        // Then: domain should map to UnexpectedError.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UnexpectedError));
    }
}
