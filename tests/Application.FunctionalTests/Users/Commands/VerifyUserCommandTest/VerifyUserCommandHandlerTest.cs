using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Users.Commands.VerificationCode;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Application.FunctionalTests.Users;

[TestFixture]
public class VerifyUserCommandHandlerTest
{
    private Mock<IUserService> _userServiceMock = default!;
    private VerifyUserCommandHandler _handler = default!;

    [SetUp]
    public void SetUp()
    {
        _userServiceMock = new Mock<IUserService>();
        _handler = new VerifyUserCommandHandler(_userServiceMock.Object);
    }

    [Test]
    public async Task ShouldReturnSuccessWhenServiceSucceeds()
    {
        // Given
        VerifyUserCommand command = new("user@test.com", "123456");
        AuthenticationResult authResult = new("user-id", "access-token", "refresh-token");

        _userServiceMock
            .Setup(x => x.ConfirmUserAsync(command.Email, command.Code))
            .ReturnsAsync(Result.Success(Unit.Value));

        _userServiceMock
            .Setup(x => x.AdminAuthUserAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(authResult));

        // When
        Result<AuthenticationResult> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(authResult));
        _userServiceMock.Verify(x => x.ConfirmUserAsync(command.Email, command.Code), Times.Once);
        _userServiceMock.Verify(x => x.AdminAuthUserAsync(command.Email, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldReturnFailureWhenServiceFails()
    {
        // Given
        VerifyUserCommand command = new("user@test.com", "123456");
        _userServiceMock
            .Setup(x => x.ConfirmUserAsync(command.Email, command.Code))
            .ReturnsAsync(Result.Failure<Unit>("E-009"));

        // When
        Result<AuthenticationResult> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("E-009"));
        _userServiceMock.Verify(x => x.AdminAuthUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Test]
    public async Task ShouldReturnFailureWhenAdminAuthFails()
    {
        // Given
        VerifyUserCommand command = new("user@test.com", "123456");
        _userServiceMock
            .Setup(x => x.ConfirmUserAsync(command.Email, command.Code))
            .ReturnsAsync(Result.Success(Unit.Value));

        _userServiceMock
            .Setup(x => x.AdminAuthUserAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AuthenticationResult>("E-AUTH-FAILED"));

        // When
        Result<AuthenticationResult> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("E-AUTH-FAILED"));
        _userServiceMock.Verify(x => x.ConfirmUserAsync(command.Email, command.Code), Times.Once);
        _userServiceMock.Verify(x => x.AdminAuthUserAsync(command.Email, It.IsAny<CancellationToken>()), Times.Once);
    }
}

