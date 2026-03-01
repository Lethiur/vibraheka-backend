using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.ConfirmPasswordRecovery;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Application.UnitTests.Users.Commands.ConfirmPasswordRecovery;

[TestFixture]
public class ConfirmPasswordRecoveryCommandHandlerTest
{
    private Mock<IPasswordResetTokenService> _passwordResetTokenServiceMock;
    private Mock<IUserCodeService> _userCodeServiceMock;
    private Mock<IUserService> _userServiceMock;
    private Mock<ILogger<ConfirmPasswordRecoveryCommandHandler>> _loggerMock;
    private IRequestHandler<ConfirmPasswordRecoveryCommand, Result<Unit>> _handler;

    [SetUp]
    public void SetUp()
    {
        _passwordResetTokenServiceMock = new Mock<IPasswordResetTokenService>();
        _userCodeServiceMock = new Mock<IUserCodeService>();
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<ConfirmPasswordRecoveryCommandHandler>>();

        _handler = new ConfirmPasswordRecoveryCommandHandler(
            _passwordResetTokenServiceMock.Object,
            _userCodeServiceMock.Object,
            _userServiceMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task ShouldConfirmPasswordAndStoreReplayMarkerWhenFlowIsValid()
    {
        // Given: a valid token and successful downstream operations
        ConfirmPasswordRecoveryCommand command = new("encrypted-token", "Password123!", "Password123!");
        PasswordResetTokenData tokenData = new("user@test.com", "123456", "token-id", DateTimeOffset.UtcNow.AddMinutes(20));

        _passwordResetTokenServiceMock.Setup(x => x.ValidateAndReadToken(command.EncryptedToken))
            .Returns(Result.Success(tokenData));
        _userCodeServiceMock.Setup(x => x.IsPasswordResetTokenUsedAsync(tokenData.Email, tokenData.TokenId, CancellationToken.None))
            .ReturnsAsync(Result.Success(false));
        _userServiceMock.Setup(x => x.ConfirmPasswordRecoveryAsync(tokenData.Email, tokenData.CognitoCode, command.NewPassword, CancellationToken.None))
            .ReturnsAsync(Result.Success(Unit.Value));
        _userCodeServiceMock.Setup(x => x.MarkPasswordResetTokenAsUsedAsync(tokenData.Email, tokenData.TokenId, tokenData.ExpiresAt, CancellationToken.None))
            .ReturnsAsync(Result.Success(Unit.Value));

        // When: the command handler is executed
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: the flow succeeds and every dependency is called with expected values
        Assert.That(result.IsSuccess, Is.True);
        _passwordResetTokenServiceMock.Verify(x => x.ValidateAndReadToken(command.EncryptedToken), Times.Once);
        _userCodeServiceMock.Verify(x => x.IsPasswordResetTokenUsedAsync(tokenData.Email, tokenData.TokenId, CancellationToken.None), Times.Once);
        _userServiceMock.Verify(x => x.ConfirmPasswordRecoveryAsync(tokenData.Email, tokenData.CognitoCode, command.NewPassword, CancellationToken.None), Times.Once);
        _userCodeServiceMock.Verify(x => x.MarkPasswordResetTokenAsUsedAsync(tokenData.Email, tokenData.TokenId, tokenData.ExpiresAt, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ShouldFailWhenTokenValidationFails()
    {
        // Given: an invalid encrypted token
        ConfirmPasswordRecoveryCommand command = new("bad-token", "Password123!", "Password123!");
        _passwordResetTokenServiceMock.Setup(x => x.ValidateAndReadToken(command.EncryptedToken))
            .Returns(Result.Failure<PasswordResetTokenData>(UserErrors.InvalidPasswordResetToken));

        // When: the command handler is executed
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: the error is propagated and no downstream calls are executed
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPasswordResetToken));
        _userCodeServiceMock.Verify(x => x.IsPasswordResetTokenUsedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _userServiceMock.Verify(x => x.ConfirmPasswordRecoveryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _userCodeServiceMock.Verify(x => x.MarkPasswordResetTokenAsUsedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldFailWhenTokenWasAlreadyUsed()
    {
        // Given: a valid token that is already marked as consumed
        ConfirmPasswordRecoveryCommand command = new("encrypted-token", "Password123!", "Password123!");
        PasswordResetTokenData tokenData = new("user@test.com", "123456", "token-id", DateTimeOffset.UtcNow.AddMinutes(20));

        _passwordResetTokenServiceMock.Setup(x => x.ValidateAndReadToken(command.EncryptedToken))
            .Returns(Result.Success(tokenData));
        _userCodeServiceMock.Setup(x => x.IsPasswordResetTokenUsedAsync(tokenData.Email, tokenData.TokenId, CancellationToken.None))
            .ReturnsAsync(Result.Success(true));

        // When: the command handler is executed
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: replay is blocked and Cognito is not called
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.PasswordResetTokenAlreadyUsed));
        _userServiceMock.Verify(x => x.ConfirmPasswordRecoveryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _userCodeServiceMock.Verify(x => x.MarkPasswordResetTokenAsUsedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldFailWhenCognitoConfirmationFails()
    {
        // Given: a valid token and a Cognito failure during password confirmation
        ConfirmPasswordRecoveryCommand command = new("encrypted-token", "Password123!", "Password123!");
        PasswordResetTokenData tokenData = new("user@test.com", "123456", "token-id", DateTimeOffset.UtcNow.AddMinutes(20));

        _passwordResetTokenServiceMock.Setup(x => x.ValidateAndReadToken(command.EncryptedToken))
            .Returns(Result.Success(tokenData));
        _userCodeServiceMock.Setup(x => x.IsPasswordResetTokenUsedAsync(tokenData.Email, tokenData.TokenId, CancellationToken.None))
            .ReturnsAsync(Result.Success(false));
        _userServiceMock.Setup(x => x.ConfirmPasswordRecoveryAsync(tokenData.Email, tokenData.CognitoCode, command.NewPassword, CancellationToken.None))
            .ReturnsAsync(Result.Failure<Unit>(UserErrors.InvalidPassword));

        // When: the command handler is executed
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: the Cognito error is returned and replay marker is not stored
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPassword));
        _userCodeServiceMock.Verify(x => x.MarkPasswordResetTokenAsUsedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldReturnSuccessWhenReplayMarkerStorageFailsAfterCognitoSuccess()
    {
        // Given: Cognito confirmation succeeds but persistence of replay marker fails
        ConfirmPasswordRecoveryCommand command = new("encrypted-token", "Password123!", "Password123!");
        PasswordResetTokenData tokenData = new("user@test.com", "123456", "token-id", DateTimeOffset.UtcNow.AddMinutes(20));

        _passwordResetTokenServiceMock.Setup(x => x.ValidateAndReadToken(command.EncryptedToken))
            .Returns(Result.Success(tokenData));
        _userCodeServiceMock.Setup(x => x.IsPasswordResetTokenUsedAsync(tokenData.Email, tokenData.TokenId, CancellationToken.None))
            .ReturnsAsync(Result.Success(false));
        _userServiceMock.Setup(x => x.ConfirmPasswordRecoveryAsync(tokenData.Email, tokenData.CognitoCode, command.NewPassword, CancellationToken.None))
            .ReturnsAsync(Result.Success(Unit.Value));
        _userCodeServiceMock.Setup(x => x.MarkPasswordResetTokenAsUsedAsync(tokenData.Email, tokenData.TokenId, tokenData.ExpiresAt, CancellationToken.None))
            .ReturnsAsync(Result.Failure<Unit>(UserErrors.UnexpectedError));

        // When: the command handler is executed
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: result is still success because replay-marker persistence is best effort
        Assert.That(result.IsSuccess, Is.True);
        _userServiceMock.Verify(x => x.ConfirmPasswordRecoveryAsync(tokenData.Email, tokenData.CognitoCode, command.NewPassword, CancellationToken.None), Times.Once);
        _userCodeServiceMock.Verify(x => x.MarkPasswordResetTokenAsUsedAsync(tokenData.Email, tokenData.TokenId, tokenData.ExpiresAt, CancellationToken.None), Times.Once);
    }
}
