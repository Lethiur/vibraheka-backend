using System.ComponentModel;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Bogus;
using CSharpFunctionalExtensions;
using DotEnv.Core;
using Microsoft.Extensions.Logging.Abstractions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.PrivilegeServiceTest;

[TestFixture]
public class HasRoleAsyncTest : GenericPrivilegeServiceTest
{
    #region HasRoleAsync Tests

    [Test]
    [DisplayName("Should return true when user has the specified role in database")]
    public async Task ShouldReturnTrueWhenUserHasRole()
    {
        // Given: A user persisted in the database with a specific role
        string userId = Guid.NewGuid().ToString();
        User user = new User
        {
            Id = userId,
            Email = "therapist@test.com",
            FullName = "Test Therapist",
            Role = UserRole.Therapist,
            CognitoId = "cognito-123"
        };
        await _userRepository.AddAsync(user);

        // When: Checking if the user has the Therapist role
        Result<bool> result = await PrivilegeService.HasRoleAsync(userId, UserRole.Therapist);

        // Then: The result should be success and true
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);
    }

    [Test]
    [DisplayName("Should return false when user has a different role in database")]
    public async Task ShouldReturnFalseWhenUserHasDifferentRole()
    {
        // Given: An admin user in the database
        string userId = Guid.NewGuid().ToString();
        User user = new User
        {
            Id = userId,
            Email = "admin@test.com",
            FullName = "Test Admin",
            Role = UserRole.Admin,
            CognitoId = "cognito-456"
        };
        await _userRepository.AddAsync(user);
        
        // When: Checking if this admin has the Therapist role
        Result<bool> result = await PrivilegeService.HasRoleAsync(userId, UserRole.Therapist);
        
        // Then: The result should be success but false
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.False);
    }

    [Test]
    [DisplayName("Should return failure when user does not exist in database")]
    public async Task ShouldReturnFailureWhenUserDoesNotExist()
    {
        // Given: A non-existent user ID
        string nonExistentUserId = "ghost-id";

        // When: Checking privileges for a user that isn't in the DB
        Result<bool> result = await PrivilegeService.HasRoleAsync(nonExistentUserId, UserRole.Therapist);
        
        // Then: Should return failure (bubbled up from Repository.GetByIdAsync)
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureUserErrors.UserNotFound));
    }

    #endregion
}
