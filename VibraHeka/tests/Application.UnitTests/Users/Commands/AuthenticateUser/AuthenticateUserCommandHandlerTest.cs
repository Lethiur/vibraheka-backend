using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Application.Common.Models.Results;
using VibraHeka.Application.Users.Commands.AuthenticateUsers;

namespace VibraHeka.Application.UnitTests.Users.Commands.AuthenticateUser;

[TestFixture]
public class AuthenticateUserCommandHandlerTest
{
    private Mock<ICognitoService> _cognitoServiceMock;
    private AuthenticateUserCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _cognitoServiceMock = new Mock<ICognitoService>();
        _handler = new AuthenticateUserCommandHandler(_cognitoServiceMock.Object);
    }

    [Test]
    public async Task ShouldCallCognitoServiceWithCorrectParameters()
    {
        // Arrange
        AuthenticateUserCommand command = new AuthenticateUserCommand ("test@example.com", "Password123!" );
        Result<AuthenticationResult> expectedResult = Result.Success(new AuthenticationResult("userId", "access", "refresh"));

        _cognitoServiceMock
            .Setup(s => s.AuthenticateUserAsync(command.Email, command.Password))
            .ReturnsAsync(expectedResult);

        // Act
        Result<AuthenticationResult> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedResult.Value));
        
        _cognitoServiceMock.Verify(s => 
            s.AuthenticateUserAsync(command.Email, command.Password), Times.Once);
    }

    [Test]
    public async Task ShouldReturnFailure_WhenCognitoServiceFails()
    {
        // Arrange
        AuthenticateUserCommand command = new AuthenticateUserCommand ("test@example.com", "Password123!" );
        string expectedError = UserException.InvalidPassword;

        _cognitoServiceMock
            .Setup(s => s.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<AuthenticationResult>(expectedError));

        // Act
        Result<AuthenticationResult> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(expectedError));
    }
}
