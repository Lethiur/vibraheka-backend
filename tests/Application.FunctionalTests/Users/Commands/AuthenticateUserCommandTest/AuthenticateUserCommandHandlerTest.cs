using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Users.Commands.AuthenticateUsers;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Application.FunctionalTests.Users;

[TestFixture]
public class AuthenticateUserCommandHandlerTest
{
    private Mock<IUserService> _userServiceMock = default!;
    private Mock<IUserRepository> _userRepositoryMock = default!;
    private AuthenticateUserCommandHandler _handler = default!;

    [SetUp]
    public void SetUp()
    {
        _userServiceMock = new Mock<IUserService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new AuthenticateUserCommandHandler(_userServiceMock.Object, _userRepositoryMock.Object);
    }

    [Test]
    public async Task ShouldReturnAuthResultWithRoleWhenUserExists()
    {
        // Given
        AuthenticateUserCommand command = new("user@test.com", "Password123!");
        AuthenticationResult auth = new("user-1", "access", "refresh");

        _userServiceMock
            .Setup(x => x.AuthenticateUserAsync(command.Email, command.Password))
            .ReturnsAsync(Result.Success(auth));

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(auth.UserID, CancellationToken.None))
            .ReturnsAsync(Result.Success(new UserEntity { Role = UserRole.Admin }));

        // When
        Result<AuthenticationResult> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserID, Is.EqualTo("user-1"));
        Assert.That(result.Value.Role, Is.EqualTo(UserRole.Admin));
    }

    [Test]
    public async Task ShouldReturnFailureWhenAuthenticationFails()
    {
        // Given
        AuthenticateUserCommand command = new("user@test.com", "Password123!");
        _userServiceMock
            .Setup(x => x.AuthenticateUserAsync(command.Email, command.Password))
            .ReturnsAsync(Result.Failure<AuthenticationResult>("E-013"));

        // When
        Result<AuthenticationResult> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("E-013"));
        _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<string>(), CancellationToken.None), Times.Never);
    }

    [Test]
    public async Task ShouldReturnFailureWhenRepositoryFails()
    {
        // Given
        AuthenticateUserCommand command = new("user@test.com", "Password123!");
        AuthenticationResult auth = new("user-1", "access", "refresh");

        _userServiceMock
            .Setup(x => x.AuthenticateUserAsync(command.Email, command.Password))
            .ReturnsAsync(Result.Success(auth));

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(auth.UserID, CancellationToken.None))
            .ReturnsAsync(Result.Failure<UserEntity>("DB-FAIL"));

        // When
        Result<AuthenticationResult> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("DB-FAIL"));
    }
}

