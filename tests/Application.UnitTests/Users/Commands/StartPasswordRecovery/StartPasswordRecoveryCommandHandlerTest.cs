using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.StartPasswordRecovery;
using VibraHeka.Domain.Common.Interfaces.User;

namespace VibraHeka.Application.UnitTests.Users.Commands.StartPasswordRecovery;

[TestFixture]
public class StartPasswordRecoveryCommandHandlerTest
{
    private Mock<IUserService> _userServiceMock;
    private Mock<ILogger<StartPasswordRecoveryCommandHandler>> _loggerMock;
    private IRequestHandler<StartPasswordRecoveryCommand, Result<Unit>> _handler;

    [SetUp]
    public void SetUp()
    {
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<StartPasswordRecoveryCommandHandler>>();
        _handler = new StartPasswordRecoveryCommandHandler(_userServiceMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task ShouldCallUserServiceWithExpectedEmailWhenCommandIsHandled()
    {
        // Given: a valid recovery command and a successful service response
        StartPasswordRecoveryCommand command = new("user@test.com");
        _userServiceMock.Setup(x => x.StartPasswordRecoveryAsync(command.Email))
            .ReturnsAsync(Result.Success(Unit.Value));

        // When: the command handler is executed
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: the operation succeeds and the service is called once with the same email
        Assert.That(result.IsSuccess, Is.True);
        _userServiceMock.Verify(x => x.StartPasswordRecoveryAsync(command.Email), Times.Once);
    }

    [Test]
    public async Task ShouldReturnSuccessWhenUserIsNotFoundToAvoidUserEnumeration()
    {
        // Given: a command for a non-existing user
        StartPasswordRecoveryCommand command = new("missing@test.com");
        _userServiceMock.Setup(x => x.StartPasswordRecoveryAsync(command.Email))
            .ReturnsAsync(Result.Failure<Unit>(UserErrors.UserNotFound));

        // When: the command handler is executed
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: the operation is compensated as success
        Assert.That(result.IsSuccess, Is.True);
        _userServiceMock.Verify(x => x.StartPasswordRecoveryAsync(command.Email), Times.Once);
    }

    [Test]
    public async Task ShouldReturnFailureWhenServiceReturnsNonCompensatedError()
    {
        // Given: a command and a non-compensated downstream error
        StartPasswordRecoveryCommand command = new("user@test.com");
        _userServiceMock.Setup(x => x.StartPasswordRecoveryAsync(command.Email))
            .ReturnsAsync(Result.Failure<Unit>(UserErrors.UnexpectedError));

        // When: the command handler is executed
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then: the handler propagates the failure
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UnexpectedError));
        _userServiceMock.Verify(x => x.StartPasswordRecoveryAsync(command.Email), Times.Once);
    }
}
