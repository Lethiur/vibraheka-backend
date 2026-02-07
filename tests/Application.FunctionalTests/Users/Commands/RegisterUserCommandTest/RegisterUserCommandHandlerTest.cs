using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Users.Commands.RegisterUser;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Application.FunctionalTests.Users;

[TestFixture]
public class RegisterUserCommandHandlerTest
{
    private Mock<IUserService> _userServiceMock = default!;
    private Mock<IUserRepository> _userRepositoryMock = default!;
    private RegisterUserCommandHandler _handler = default!;

    [SetUp]
    public void SetUp()
    {
        _userServiceMock = new Mock<IUserService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new RegisterUserCommandHandler(_userServiceMock.Object, _userRepositoryMock.Object);
    }

    [Test]
    public async Task ShouldRegisterAndPersistUserWhenCognitoSucceeds()
    {
        // Given
        RegisterUserCommand command = new("test@example.com", "Password123!", "John Doe", "Europe/Madrid");
        const string cognitoId = "cognito-123";

        _userServiceMock
            .Setup(x => x.RegisterUserAsync(command.Email, command.Password, command.FullName))
            .ReturnsAsync(Result.Success(cognitoId));

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync(Result.Success(cognitoId));

        // When
        Result<UserRegistrationResult> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserId, Is.EqualTo(cognitoId));
        Assert.That(result.Value.needsConfirmation, Is.True);

        _userRepositoryMock.Verify(x => x.AddAsync(It.Is<UserEntity>(u =>
            u.Id == cognitoId &&
            u.CognitoId == cognitoId &&
            u.Email == command.Email &&
            u.FirstName == command.FullName)), Times.Once);
    }

    [Test]
    public async Task ShouldReturnFailureWhenCognitoFails()
    {
        // Given
        RegisterUserCommand command = new("test@example.com", "Password123!", "John Doe","Europe/Madrid");
        _userServiceMock
            .Setup(x => x.RegisterUserAsync(command.Email, command.Password, command.FullName))
            .ReturnsAsync(Result.Failure<string>("E-002"));

        // When
        Result<UserRegistrationResult> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("E-002"));
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<UserEntity>()), Times.Never);
    }

    [Test]
    public async Task ShouldReturnFailureWhenRepositoryAddFails()
    {
        // Given
        RegisterUserCommand command = new("test@example.com", "Password123!", "John Doe", "Europe/Madrid");
        _userServiceMock
            .Setup(x => x.RegisterUserAsync(command.Email, command.Password, command.FullName))
            .ReturnsAsync(Result.Success("cognito-123"));

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync(Result.Failure<string>("DB-FAIL"));

        // When
        Result<UserRegistrationResult> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("DB-FAIL"));
    }
}

