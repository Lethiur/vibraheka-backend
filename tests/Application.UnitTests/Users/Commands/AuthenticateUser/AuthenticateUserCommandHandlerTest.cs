using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Commands.AuthenticateUsers;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Application.UnitTests.Users.Commands.AuthenticateUser;

[TestFixture]
public class AuthenticateUserCommandHandlerTest
{
    private Mock<IUserService> _cognitoServiceMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private AuthenticateUserCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _cognitoServiceMock = new Mock<IUserService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        
        _handler = new AuthenticateUserCommandHandler(_cognitoServiceMock.Object, _userRepositoryMock.Object);
    }

    [Test]
    public async Task ShouldCallCognitoServiceWithCorrectParameters()
    {
        // Arrange
        AuthenticateUserCommand command = new("test@example.com", "Password123!" );
        Result<AuthenticationResult> expectedResult = Result.Success(new AuthenticationResult("userId", "access", "refresh"));

        _cognitoServiceMock
            .Setup(s => s.AuthenticateUserAsync(command.Email, command.Password))
            .ReturnsAsync(expectedResult);

        _userRepositoryMock.Setup(repository => repository.GetByIdAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(Result.Of(new UserEntity()));
        
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
        AuthenticateUserCommand command = new("test@example.com", "Password123!" );
        string expectedError = UserErrors.InvalidPassword;

        _cognitoServiceMock
            .Setup(s => s.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<AuthenticationResult>(expectedError));
        
        _userRepositoryMock.Setup(repository => repository.GetByIdAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(Result.Of(new UserEntity()));

        // Act
        Result<AuthenticationResult> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(expectedError));
    }

    [Test]
    public async Task ShouldPropagateErrorFromRepository()
    {
        // Given: Some mocking to return error
        _userRepositoryMock.Setup(repository => repository.GetByIdAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(Result.Failure<UserEntity>(UserErrors.UserNotFound));
        
        _cognitoServiceMock.Setup(s => s.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(Result.Success(new AuthenticationResult()));
        
        // And: Some command
        AuthenticateUserCommand command = new("test@example.com", "Password123!" );
        
        // When: The command is invoked
        Result<AuthenticationResult> result = await _handler.Handle(command, CancellationToken.None);
        
        // Then: Should return the error from the repository
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));
    }

    [Test]
    public async Task ShouldReturnRoleOfTheUser()
    {
        // Arrange
        AuthenticateUserCommand command = new("test@example.com", "Password123!" );
        Result<AuthenticationResult> expectedResult = Result.Success(new AuthenticationResult("userId", "access", "refresh"));

        _cognitoServiceMock
            .Setup(s => s.AuthenticateUserAsync(command.Email, command.Password))
            .ReturnsAsync(expectedResult);

        _userRepositoryMock.Setup(repository => repository.GetByIdAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(Result.Of(new UserEntity()
            {
                Role = UserRole.Therapist
            }));
        
        // Act
        Result<AuthenticationResult> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Role, Is.EqualTo(expectedResult.Value.Role));
        
        _cognitoServiceMock.Verify(s => 
            s.AuthenticateUserAsync(command.Email, command.Password), Times.Once);
    }
    
    
}
