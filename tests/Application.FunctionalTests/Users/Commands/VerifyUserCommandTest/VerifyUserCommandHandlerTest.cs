using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.VerificationCode;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Application.FunctionalTests.Users;

[TestFixture]
public class VerifyUserCommandHandlerTest
{
    private Mock<IPasswordResetTokenService> _passwordResetTokenServiceMock = default!;
    private Mock<IUserCodeService> _userCodeServiceMock = default!;
    private Mock<IUserService> _userServiceMock = default!;
    private Mock<ILogger<VerifyUserCommandHandler>> _loggerMock = default!;
    private IRequestHandler<VerifyUserCommand, Result<Unit>> _handler = default!;

    [SetUp]
    public void SetUp()
    {
        _passwordResetTokenServiceMock = new Mock<IPasswordResetTokenService>();
        _userCodeServiceMock = new Mock<IUserCodeService>();
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<VerifyUserCommandHandler>>();

        _handler = new VerifyUserCommandHandler(
            _passwordResetTokenServiceMock.Object,
            _userCodeServiceMock.Object,
            _userServiceMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task ShouldConfirmUserAndStoreReplayMarkerWhenFlowIsValid()
    {
        // Given: a valid token and successful downstream operations
        VerifyUserCommand command = new("encrypted-token");
        PasswordResetTokenData tokenData = new("user@test.com", "123456", "token-id", DateTimeOffset.UtcNow.AddMinutes(20));

        _passwordResetTokenServiceMock.Setup(x => x.ValidateAndReadToken(command.EncryptedCode))
            .Returns(Result.Success(tokenData));
        _userCodeServiceMock.Setup(x => x.IsPasswordResetTokenUsedAsync(tokenData.Email, tokenData.TokenId, CancellationToken.None))
            .ReturnsAsync(Result.Success(false));
        _userServiceMock.Setup(x => x.ConfirmUserAsync(tokenData.Email, tokenData.CognitoCode))
            .ReturnsAsync(Result.Success(Unit.Value));
        _userCodeServiceMock.Setup(x => x.MarkPasswordResetTokenAsUsedAsync(tokenData.Email, tokenData.TokenId, tokenData.ExpiresAt, CancellationToken.None))
            .ReturnsAsync(Result.Success(Unit.Value));

        // When: the command handler is executed
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: the flow succeeds and every dependency is called with expected values
        Assert.That(result.IsSuccess, Is.True);
        _passwordResetTokenServiceMock.Verify(x => x.ValidateAndReadToken(command.EncryptedCode), Times.Once);
        _userCodeServiceMock.Verify(x => x.IsPasswordResetTokenUsedAsync(tokenData.Email, tokenData.TokenId, CancellationToken.None), Times.Once);
        _userServiceMock.Verify(x => x.ConfirmUserAsync(tokenData.Email, tokenData.CognitoCode), Times.Once);
        _userCodeServiceMock.Verify(x => x.MarkPasswordResetTokenAsUsedAsync(tokenData.Email, tokenData.TokenId, tokenData.ExpiresAt, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ShouldFailWhenTokenValidationFails()
    {
        // Given: an invalid encrypted token
        VerifyUserCommand command = new("bad-token");
        _passwordResetTokenServiceMock.Setup(x => x.ValidateAndReadToken(command.EncryptedCode))
            .Returns(Result.Failure<PasswordResetTokenData>(UserErrors.InvalidPasswordResetToken));

        // When: the command handler is executed
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: the error is propagated and no downstream calls are made
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPasswordResetToken));
        _userCodeServiceMock.Verify(x => x.IsPasswordResetTokenUsedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _userServiceMock.Verify(x => x.ConfirmUserAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _userCodeServiceMock.Verify(x => x.MarkPasswordResetTokenAsUsedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldFailWhenTokenIsExpired()
    {
        // Given: an expired token
        VerifyUserCommand command = new("expired-token");
        _passwordResetTokenServiceMock.Setup(x => x.ValidateAndReadToken(command.EncryptedCode))
            .Returns(Result.Failure<PasswordResetTokenData>(UserErrors.PasswordResetTokenExpired));

        // When: the command handler is executed
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: the expiry error is returned
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.PasswordResetTokenExpired));
        _userServiceMock.Verify(x => x.ConfirmUserAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ShouldFailWhenTokenWasAlreadyUsed()
    {
        // Given: a valid token that is already marked as consumed
        VerifyUserCommand command = new("encrypted-token");
        PasswordResetTokenData tokenData = new("user@test.com", "123456", "token-id", DateTimeOffset.UtcNow.AddMinutes(20));

        _passwordResetTokenServiceMock.Setup(x => x.ValidateAndReadToken(command.EncryptedCode))
            .Returns(Result.Success(tokenData));
        _userCodeServiceMock.Setup(x => x.IsPasswordResetTokenUsedAsync(tokenData.Email, tokenData.TokenId, CancellationToken.None))
            .ReturnsAsync(Result.Success(true));

        // When: the command handler is executed
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: replay is blocked and Cognito is not called
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.PasswordResetTokenAlreadyUsed));
        _userServiceMock.Verify(x => x.ConfirmUserAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _userCodeServiceMock.Verify(x => x.MarkPasswordResetTokenAsUsedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldFailWhenCognitoConfirmationFails()
    {
        // Given: a valid token but Cognito rejects the confirmation code
        VerifyUserCommand command = new("encrypted-token");
        PasswordResetTokenData tokenData = new("user@test.com", "123456", "token-id", DateTimeOffset.UtcNow.AddMinutes(20));

        _passwordResetTokenServiceMock.Setup(x => x.ValidateAndReadToken(command.EncryptedCode))
            .Returns(Result.Success(tokenData));
        _userCodeServiceMock.Setup(x => x.IsPasswordResetTokenUsedAsync(tokenData.Email, tokenData.TokenId, CancellationToken.None))
            .ReturnsAsync(Result.Success(false));
        _userServiceMock.Setup(x => x.ConfirmUserAsync(tokenData.Email, tokenData.CognitoCode))
            .ReturnsAsync(Result.Failure<Unit>(UserErrors.WrongVerificationCode));

        // When: the command handler is executed
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: the failure is propagated and replay marker is not stored
        Assert.That(result.IsFailure, Is.True);
        _userCodeServiceMock.Verify(x => x.MarkPasswordResetTokenAsUsedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldReturnSuccessWhenReplayMarkerStorageFailsAfterCognitoSuccess()
    {
        // Given: Cognito confirmation succeeds but persistence of replay marker fails
        VerifyUserCommand command = new("encrypted-token");
        PasswordResetTokenData tokenData = new("user@test.com", "123456", "token-id", DateTimeOffset.UtcNow.AddMinutes(20));

        _passwordResetTokenServiceMock.Setup(x => x.ValidateAndReadToken(command.EncryptedCode))
            .Returns(Result.Success(tokenData));
        _userCodeServiceMock.Setup(x => x.IsPasswordResetTokenUsedAsync(tokenData.Email, tokenData.TokenId, CancellationToken.None))
            .ReturnsAsync(Result.Success(false));
        _userServiceMock.Setup(x => x.ConfirmUserAsync(tokenData.Email, tokenData.CognitoCode))
            .ReturnsAsync(Result.Success(Unit.Value));
        _userCodeServiceMock.Setup(x => x.MarkPasswordResetTokenAsUsedAsync(tokenData.Email, tokenData.TokenId, tokenData.ExpiresAt, CancellationToken.None))
            .ReturnsAsync(Result.Failure<Unit>(UserErrors.UnexpectedError));

        // When: the command handler is executed
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: result is still success because replay-marker persistence is best-effort
        Assert.That(result.IsSuccess, Is.True);
        _userServiceMock.Verify(x => x.ConfirmUserAsync(tokenData.Email, tokenData.CognitoCode), Times.Once);
        _userCodeServiceMock.Verify(x => x.MarkPasswordResetTokenAsUsedAsync(tokenData.Email, tokenData.TokenId, tokenData.ExpiresAt, CancellationToken.None), Times.Once);
    }
}
