using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.ResendConfirmationCode;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;

namespace VibraHeka.Application.FunctionalTests.Users;

[TestFixture]
public class ResendConfirmationCodeCommandHandlerTest
{
    private Mock<IUserService> _userServiceMock = default!;
    private Mock<IPrivilegeService> _privilegeServiceMock = default!;
    private ResendConfirmationCodeCommandHandler _handler = default!;

    [SetUp]
    public void SetUp()
    {
        _userServiceMock = new Mock<IUserService>();
        _privilegeServiceMock = new Mock<IPrivilegeService>();
        _handler = new ResendConfirmationCodeCommandHandler(_userServiceMock.Object, _privilegeServiceMock.Object);
    }

    [Test]
    public async Task ShouldResendCodeWhenCanExecuteActionIsTrue()
    {
        // Given
        ResendConfirmationCodeCommand command = new("user@test.com");
        _userServiceMock
            .Setup(x => x.GetUserID(command.Email))
            .ReturnsAsync(Result.Success("user-id"));

        _privilegeServiceMock
            .Setup(x => x.CanExecuteAction("user-id", ActionType.RequestVerificationCode, CancellationToken.None))
            .ReturnsAsync(Result.Success(true));

        _userServiceMock
            .Setup(x => x.ResendVerificationCodeAsync(command.Email))
            .ReturnsAsync(Result.Success(Unit.Value));

        // When
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        _userServiceMock.Verify(x => x.ResendVerificationCodeAsync(command.Email), Times.Once);
    }

    [Test]
    public async Task ShouldReturnNotAuthorizedWhenCanExecuteActionIsFalse()
    {
        // Given
        ResendConfirmationCodeCommand command = new("user@test.com");
        _userServiceMock
            .Setup(x => x.GetUserID(command.Email))
            .ReturnsAsync(Result.Success("user-id"));

        _privilegeServiceMock
            .Setup(x => x.CanExecuteAction("user-id", ActionType.RequestVerificationCode, CancellationToken.None))
            .ReturnsAsync(Result.Success(false));

        // When
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.NotAuthorized));
        _userServiceMock.Verify(x => x.ResendVerificationCodeAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ShouldReturnFailureWhenGetUserIdFails()
    {
        // Given
        ResendConfirmationCodeCommand command = new("user@test.com");
        _userServiceMock
            .Setup(x => x.GetUserID(command.Email))
            .ReturnsAsync(Result.Failure<string>("E-003"));

        // When
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("E-003"));
        _privilegeServiceMock.Verify(x => x.CanExecuteAction(It.IsAny<string>(), It.IsAny<ActionType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldReturnFailureWhenPrivilegeServiceFails()
    {
        // Given
        ResendConfirmationCodeCommand command = new("user@test.com");
        _userServiceMock
            .Setup(x => x.GetUserID(command.Email))
            .ReturnsAsync(Result.Success("user-id"));

        _privilegeServiceMock
            .Setup(x => x.CanExecuteAction("user-id", ActionType.RequestVerificationCode, CancellationToken.None))
            .ReturnsAsync(Result.Failure<bool>("P-FAIL"));

        // When
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("P-FAIL"));
        _userServiceMock.Verify(x => x.ResendVerificationCodeAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ShouldReturnFailureWhenResendFails()
    {
        // Given
        ResendConfirmationCodeCommand command = new("user@test.com");
        _userServiceMock
            .Setup(x => x.GetUserID(command.Email))
            .ReturnsAsync(Result.Success("user-id"));

        _privilegeServiceMock
            .Setup(x => x.CanExecuteAction("user-id", ActionType.RequestVerificationCode, CancellationToken.None))
            .ReturnsAsync(Result.Success(true));

        _userServiceMock
            .Setup(x => x.ResendVerificationCodeAsync(command.Email))
            .ReturnsAsync(Result.Failure<Unit>("E-012"));

        // When
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("E-012"));
    }
}

