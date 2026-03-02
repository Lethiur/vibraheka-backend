using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.ChangeAuthenticatedPassword;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;

namespace VibraHeka.Application.UnitTests.Users.Commands.ChangeAuthenticatedPassword;

[TestFixture]
public class ChangeAuthenticatedPasswordCommandHandlerTest
{
    private Mock<ICurrentUserService> _currentUserServiceMock;
    private Mock<IUserService> _userServiceMock;
    private Mock<ILogger<ChangeAuthenticatedPasswordCommandHandler>> _loggerMock;
    private ChangeAuthenticatedPasswordCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<ChangeAuthenticatedPasswordCommandHandler>>();

        _handler = new ChangeAuthenticatedPasswordCommandHandler(
            _currentUserServiceMock.Object,
            _userServiceMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task ShouldChangePasswordWhenUserIsAuthenticatedAndCommandIsValid()
    {
        // Given: a valid authenticated context and a successful downstream service response.
        ChangeAuthenticatedPasswordCommand command = new("Current123!", "NewPassword123!", "NewPassword123!");
        _currentUserServiceMock.Setup(x => x.UserId).Returns("user-1");
        _currentUserServiceMock.Setup(x => x.AccessToken).Returns("access-token");
        _userServiceMock.Setup(x => x.ChangePasswordAsync("access-token", command.CurrentPassword, command.NewPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        // When: handling the authenticated password change command.
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: the operation should succeed and call the service once with expected parameters.
        Assert.That(result.IsSuccess, Is.True);
        _userServiceMock.Verify(x => x.ChangePasswordAsync("access-token", command.CurrentPassword, command.NewPassword, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldReturnNotAuthorizedWhenAccessTokenIsMissing()
    {
        // Given: an authenticated command request without access token in current user context.
        ChangeAuthenticatedPasswordCommand command = new("Current123!", "NewPassword123!", "NewPassword123!");
        _currentUserServiceMock.Setup(x => x.UserId).Returns("user-1");
        _currentUserServiceMock.Setup(x => x.AccessToken).Returns((string?)null);

        // When: handling the command.
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: the operation should fail as not authorized and avoid service calls.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.NotAuthorized));
        _userServiceMock.Verify(x => x.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldReturnFailureWhenUserServiceFails()
    {
        // Given: an authenticated context where downstream change password fails.
        ChangeAuthenticatedPasswordCommand command = new("Current123!", "NewPassword123!", "NewPassword123!");
        _currentUserServiceMock.Setup(x => x.UserId).Returns("user-1");
        _currentUserServiceMock.Setup(x => x.AccessToken).Returns("access-token");
        _userServiceMock.Setup(x => x.ChangePasswordAsync("access-token", command.CurrentPassword, command.NewPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Unit>(UserErrors.InvalidPassword));

        // When: handling the command.
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: the command should fail and propagate domain error.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPassword));
        _userServiceMock.Verify(x => x.ChangePasswordAsync("access-token", command.CurrentPassword, command.NewPassword, It.IsAny<CancellationToken>()), Times.Once);
    }
}
