using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.VerificationCode;
using VibraHeka.Domain.Common.Interfaces.User;

namespace VibraHeka.Application.UnitTests.Users.Commands.VerifyUser;

public class VerifyUserCommandHandlerTest
{
    private IRequestHandler<VerifyUserCommand, Result<Unit>> _handler;
    private Mock<IUserService> _cognitoServiceMock;

    [SetUp]
    public void Setup()
    {
        _cognitoServiceMock = new Mock<IUserService>();
        _handler = new VerifyUserCommandHandler(_cognitoServiceMock.Object);
    }
    
    [Test]
    public async Task ShouldVerifyUserSuccessfully()
    {
        // Given: Some mocking
        _cognitoServiceMock.Setup(service => service.ConfirmUserAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Success(Unit.Value));
        
        // When: User is confirmed
        Result<Unit> result = await _handler.Handle(new VerifyUserCommand("test", "test"), CancellationToken.None);
        
        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);
        
        _cognitoServiceMock.Verify(service => service.ConfirmUserAsync("test","test"), Times.Once);
    }

    [Test]
    public async Task ShouldPropagateErrorFromService()
    {
        // Given: Some mocking to return error
        _cognitoServiceMock.Setup(service => service.ConfirmUserAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<Unit>(UserErrors.UnexpectedError));
        // When: User is confirmed
        Result<Unit> result = await _handler.Handle(new VerifyUserCommand("test", "test"), CancellationToken.None);
        
        // Then: Should return failure
        Assert.That(result.IsSuccess, Is.False);
        
        // And: With the expected error
        Assert.That(result.Error, Is.EqualTo(UserErrors.UnexpectedError));
        
        _cognitoServiceMock.Verify(service => service.ConfirmUserAsync("test","test"), Times.Once);
    }
}
