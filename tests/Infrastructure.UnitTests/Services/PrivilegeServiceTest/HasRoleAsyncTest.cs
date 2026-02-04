using System.ComponentModel;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.PrivilegeServiceTest;

[TestFixture]
public class HasRoleAsyncTest
{
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IActionLogRepository> _actionLogRepositoryMock;
    private Mock<ILogger<IPrivilegeService>> _loggerMock;
    
    private PrivilegeService _service;

    [SetUp]
    public void SetUp()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<IPrivilegeService>>();
        _actionLogRepositoryMock = new Mock<IActionLogRepository>();
        _service = new PrivilegeService(_userRepositoryMock.Object, _actionLogRepositoryMock.Object, _loggerMock.Object);
    }

    [Test]
    [DisplayName("Should return true when user has the specified role")]
    public async Task ShouldReturnTrueWhenUserHasTheRole()
    {
        // Given: A user with Admin role in the repository
        const string userId = "admin-id";
        User user = new User { Id = userId, Role = UserRole.Admin };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(Result.Success(user));

        // When: Checking if the user has the Admin role
        Result<bool> result = await _service.HasRoleAsync(userId, UserRole.Admin);

        // Then: Should return success with true
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);
    }

    [Test]
    [DisplayName("Should return false when user does not have the specified role")]
    public async Task ShouldReturnFalseWhenUserDoesNotHaveTheRole()
    {
        // Given: A user with Therapist role
        const string userId = "therapist-id";
        User user = new User { Id = userId, Role = UserRole.Therapist };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(Result.Success(user));

        // When: Checking if the user has the Admin role
        Result<bool> result = await _service.HasRoleAsync(userId, UserRole.Admin);

        // Then: Should return success with false
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.False);
    }

    [Test]
    [DisplayName("Should return failure and log error when user is not found")]
    public async Task ShouldReturnFailureWhenUserNotFound()
    {
        // Given: Repository returns failure (user not found)
        const string userId = "unknown-id";
        const string errorMessage = "User not found";
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(Result.Failure<User>(errorMessage));

        // When: Checking the role
        Result<bool> result = await _service.HasRoleAsync(userId, UserRole.Admin);

        // Then: Should return failure with the repository error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(errorMessage));
        
    }
}
