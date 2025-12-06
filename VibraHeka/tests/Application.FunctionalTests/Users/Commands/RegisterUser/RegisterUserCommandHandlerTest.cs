using System.ComponentModel;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Application.Common.Models.Results;
using VibraHeka.Application.Users.Commands;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.FunctionalTests.Users.Commands.RegisterUser;

public class RegisterUserCommandHandlerTest
{
     private IRequestHandler<RegisterUserCommand, Result<UserRegistrationResult>> _handler;
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<ICognitoService> _cognitoServiceMock;
    private RegisterUserCommandValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _cognitoServiceMock = new Mock<ICognitoService>();
        _validator = new RegisterUserCommandValidator();
        
        _handler = new RegisterUserCommandHandler(
            _cognitoServiceMock.Object,
            _userRepositoryMock.Object);
    }

    [Test]
    [DisplayName("Should register user successfully with valid command")]
    public async Task ShouldRegisterUserSuccessfullyWhenValidCommandProvided()
    {
        // Given: Valid command and successful external services
        var command = new RegisterUserCommand("test@example.com", "Password123!", "John Doe");
        
        _cognitoServiceMock.Setup(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Success("user-sub-123"));
        
        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(Result.Success("user-id-123"));

        // When: Handling the command
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserId, Is.EqualTo("user-id-123"));
        
        // And: Should call external services
        _cognitoServiceMock.Verify(x => x.RegisterUserAsync("test@example.com", "Password123!", "John Doe"), Times.Once);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Test]
    [DisplayName("Should fail when user already exists")]
    public async Task ShouldFailWhenUserAlreadyExists()
    {
        // Given: Command for existing user
        var command = new RegisterUserCommand("existing@example.com", "Password123!", "John Doe");
        
        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync("existing@example.com"))
            .ReturnsAsync(Result.Success(true));

        // When: Handling the command
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then: Should fail with UserAlreadyExists
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserException.UserAlreadyExist));
        
        // And: Should not call Cognito
        _cognitoServiceMock.Verify(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
