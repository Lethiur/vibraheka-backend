using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Users.Commands.AuthenticateUsers;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Domain.User.Ports.Output;

namespace VibraHeka.Application.FunctionalTests.Users.Commands.AuthenticateUserCommandTest;

[TestFixture]
public class AuthenticateUserCommandHandlerTest
{
    private Mock<UserPort> _userServiceMock = default!;
    private Mock<UserProfilePort> _userRepositoryMock = default!;
    private AuthenticateUserCommandHandler _handler = default!;

    [SetUp]
    public void SetUp()
    {
        _userServiceMock = new Mock<UserPort>();
        _userRepositoryMock = new Mock<UserProfilePort>();
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
            .Setup(x => x.GetProfileByUserId(auth.UserID, CancellationToken.None))
            .ReturnsAsync(Result.Success(new UserProfileEntity { Role = UserRole.Admin }));

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
        _userRepositoryMock.Verify(x => x.GetProfileByUserId(It.IsAny<string>(), CancellationToken.None), Times.Never);
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
            .Setup(x => x.GetProfileByUserId(auth.UserID, CancellationToken.None))
            .ReturnsAsync(Result.Failure<UserProfileEntity>("DB-FAIL"));

        // When
        Result<AuthenticationResult> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("DB-FAIL"));
    }
}

