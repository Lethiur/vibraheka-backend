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
        UserProfileEntity userProfile = new(userId, "user@test.com", "John Doe");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, CancellationToken.None))
            .ReturnsAsync(Result.Success(userProfile));

        // When
        Result<UserProfileEntity> result = await _service.GetUserByID(userId, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Id, Is.EqualTo(userId));
        Assert.That(result.Value.Email, Is.EqualTo(userProfile.Email));
        Assert.That(result.Value.FirstName, Is.EqualTo(userProfile.FirstName));

        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId, CancellationToken.None), Times.Once);
    }

    [TestCase(null, TestName = "Null user id")]
    [TestCase("", TestName = "Empty user id")]
    [TestCase("   ", TestName = "Whitespace user id")]
    [DisplayName("Should fail with InvalidUserID and not call repository when user id is invalid")]
    public async Task ShouldFailWithInvalidUserIdAndNotCallRepositoryWhenUserIdIsInvalid(string? userId)
    {
        // When
        Result<UserProfileEntity> result = await _service.GetUserByID(userId!, CancellationToken.None);

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
            .ReturnsAsync(Result.Success<UserProfileEntity>(null!));

        // When
        Result<UserProfileEntity> result = await _service.GetUserByID(userId, CancellationToken.None);

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
            .ReturnsAsync(Result.Failure<UserProfileEntity>(InfrastructureUserErrors.UserNotFound));

        // When
        Result<UserProfileEntity> result = await _service.GetUserByID(userId, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.AnyOf(InfrastructureUserErrors.UserNotFound, UserErrors.UserNotFound)
        );

        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId, CancellationToken.None), Times.Once);
    }
}
