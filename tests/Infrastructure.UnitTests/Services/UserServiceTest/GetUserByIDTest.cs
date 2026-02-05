using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserServiceTest;

[TestFixture]
public class GetUserByIDTests : GenericUserServiceTest
{
    [Test]
    [DisplayName("Should return user when repository returns an existing user")]
    public async Task ShouldReturnUserWhenRepositoryReturnsExistingUser()
    {
        // Given
        string userId = Guid.NewGuid().ToString();
        UserEntity user = new UserEntity(userId, "user@test.com", "John Doe");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, CancellationToken.None))
            .ReturnsAsync(Result.Success(user));

        // When
        Result<UserEntity> result = await _service.GetUserByID(userId, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Id, Is.EqualTo(userId));
        Assert.That(result.Value.Email, Is.EqualTo(user.Email));
        Assert.That(result.Value.FirstName, Is.EqualTo(user.FirstName));

        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId, CancellationToken.None), Times.Once);
    }

    [TestCase(null, TestName = "Null user id")]
    [TestCase("", TestName = "Empty user id")]
    [TestCase("   ", TestName = "Whitespace user id")]
    [DisplayName("Should fail with InvalidUserID and not call repository when user id is invalid")]
    public async Task ShouldFailWithInvalidUserIdAndNotCallRepositoryWhenUserIdIsInvalid(string? userId)
    {
        // When
        Result<UserEntity> result = await _service.GetUserByID(userId!, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidUserID));

        _userRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<string>(), CancellationToken.None), Times.Never);
    }

    [Test]
    [DisplayName("Should fail with UserNotFound when repository succeeds but returns null user")]
    public async Task ShouldFailWithUserNotFoundWhenRepositoryReturnsNullUser()
    {
        // Given
        string userId = Guid.NewGuid().ToString();

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, CancellationToken.None))
            .ReturnsAsync(Result.Success<UserEntity>(null!));

        // When
        Result<UserEntity> result = await _service.GetUserByID(userId, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));

        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId, CancellationToken.None), Times.Once);
    }

    [Test]
    [DisplayName("Should propagate repository failure when repository returns failure")]
    public async Task ShouldPropagateRepositoryFailureWhenRepositoryReturnsFailure()
    {
        // Given
        string userId = Guid.NewGuid().ToString();

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, CancellationToken.None))
            .ReturnsAsync(Result.Failure<UserEntity>(InfrastructureUserErrors.UserNotFound));

        // When
        Result<UserEntity> result = await _service.GetUserByID(userId, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.AnyOf(InfrastructureUserErrors.UserNotFound, UserErrors.UserNotFound)
        );

        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId, CancellationToken.None), Times.Once);
    }
}
