using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Users.Commands.VerificationCode;
using VibraHeka.Domain.Common.Interfaces.User;

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
        _userServiceMock
            .Setup(x => x.ConfirmUserAsync(command.Email, command.Code))
            .ReturnsAsync(Result.Success(Unit.Value));

        // When
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        _userServiceMock.Verify(x => x.ConfirmUserAsync(command.Email, command.Code), Times.Once);
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
        Result<Unit> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("E-009"));
    }
}

