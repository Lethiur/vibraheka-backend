using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.VerificationCode;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Application.UnitTests.Users.Commands.VerifyUser;

public class VerifyUserCommandHandlerTest
{
    private IRequestHandler<VerifyUserCommand, Result<AuthenticationResult>> _handler;
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
        const string email = "test@test.com";
        const string code = "123456";
        AuthenticationResult authResult = new("user-id", "access-token", "refresh-token");

        _cognitoServiceMock.Setup(service => service.ConfirmUserAsync(email, code))
            .ReturnsAsync(Result.Success(Unit.Value));
        
        _cognitoServiceMock.Setup(service => service.AdminAuthUserAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(authResult));
        
        // When: User is confirmed
        Result<AuthenticationResult> result = await _handler.Handle(new VerifyUserCommand(email, code), CancellationToken.None);
        
        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(authResult));
        
        _cognitoServiceMock.Verify(service => service.ConfirmUserAsync(email, code), Times.Once);
        _cognitoServiceMock.Verify(service => service.AdminAuthUserAsync(email, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldPropagateErrorFromService()
    {
        // Given: Some mocking to return error
        const string email = "test@test.com";
        const string code = "123456";

        _cognitoServiceMock.Setup(service => service.ConfirmUserAsync(email, code))
            .ReturnsAsync(Result.Failure<Unit>(UserErrors.UnexpectedError));

        // When: User is confirmed
        Result<AuthenticationResult> result = await _handler.Handle(new VerifyUserCommand(email, code), CancellationToken.None);
        
        // Then: Should return failure
        Assert.That(result.IsSuccess, Is.False);
        
        // And: With the expected error
        Assert.That(result.Error, Is.EqualTo(UserErrors.UnexpectedError));
        
        _cognitoServiceMock.Verify(service => service.ConfirmUserAsync(email, code), Times.Once);
        _cognitoServiceMock.Verify(service => service.AdminAuthUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldReturnFailureWhenAdminAuthFails()
    {
        // Given: ConfirmUser succeeds but AdminAuth fails
        const string email = "test@test.com";
        const string code = "123456";

        _cognitoServiceMock.Setup(service => service.ConfirmUserAsync(email, code))
            .ReturnsAsync(Result.Success(Unit.Value));

        _cognitoServiceMock.Setup(service => service.AdminAuthUserAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AuthenticationResult>(UserErrors.NotAuthorized));

        // When: Handling the command
        Result<AuthenticationResult> result = await _handler.Handle(new VerifyUserCommand(email, code), CancellationToken.None);

        // Then: Should return failure from AdminAuth
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.NotAuthorized));

        _cognitoServiceMock.Verify(service => service.ConfirmUserAsync(email, code), Times.Once);
        _cognitoServiceMock.Verify(service => service.AdminAuthUserAsync(email, It.IsAny<CancellationToken>()), Times.Once);
    }
}
