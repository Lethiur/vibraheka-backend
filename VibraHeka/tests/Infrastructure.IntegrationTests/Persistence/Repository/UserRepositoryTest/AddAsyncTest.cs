using System.ComponentModel;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Bogus;
using CSharpFunctionalExtensions;
using DotEnv.Core;
using Microsoft.Extensions.Configuration;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.UserRepositoryTest;

[TestFixture]
public class AddAsyncTest : GenericUserRepositoryTest
{
    
    #region AddAsync - Success Cases

    [Test]
    [DisplayName("Should add user successfully with valid data")]
    public async Task ShouldAddUserSuccessfullyWhenValidDataProvided()
    {
        // Given: A valid user entity
        User user = CreateValidUser();

        // When: Adding the user to repository
        Result<string> result = await _userRepository.AddAsync(user);

        // Then: Should return success with user ID
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(user.Id));
        Assert.That(result.Value, Is.Not.Null.And.Not.Empty);

    }

    [Test]
    [DisplayName("Should add user successfully with complex email")]
    public async Task ShouldAddUserSuccessfullyWhenComplexEmailProvided()
    {
        // Given: A user with complex email format
        string email = $"test.user+complex@{_faker.Internet.DomainName()}";
        User user = new User(Guid.NewGuid().ToString(), email, _faker.Person.FullName);

        // When: Adding the user
        Result<string> result = await _userRepository.AddAsync(user);

        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(user.Id));
        
    }

    [Test]
    [DisplayName("Should add user successfully with special characters in name")]
    public async Task ShouldAddUserSuccessfullyWhenSpecialCharactersInName()
    {
        // Given: A user with special characters in name
        User user = new User(
            Guid.NewGuid().ToString(), 
            _faker.Internet.Email(), 
            "José María O'Connor-Smith"
        );

        // When: Adding the user
        Result<string> result = await _userRepository.AddAsync(user);

        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(user.Id));

    }

    [Test]
    [DisplayName("Should add user successfully with long name")]
    public async Task ShouldAddUserSuccessfullyWhenLongNameProvided()
    {
        // Given: A user with a very long name
        string longName = new string('A', 100) + " " + new string('B', 100);
        User user = new User(Guid.NewGuid().ToString(), _faker.Internet.Email(), longName);

        // When: Adding the user
        Result<string> result = await _userRepository.AddAsync(user);

        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(user.Id));

    }

    #endregion

    #region AddAsync - Data Persistence Verification

    [Test]
    [DisplayName("Should persist user data correctly in DynamoDB")]
    public async Task ShouldPersistUserDataCorrectlyWhenUserAdded()
    {
        // Given: A user with specific data
        User originalUser = CreateValidUser();

        // When: Adding the user
        Result<string> addResult = await _userRepository.AddAsync(originalUser);
        Assert.That(addResult.IsSuccess, Is.True);

        // And: Retrieving the user directly from DynamoDB
        LoadConfig loadConfig = new()
        {
            OverrideTableName = _configuration.UsersTable
        };
        User? retrievedUser = await _dynamoContext.LoadAsync<User>(originalUser.Id, loadConfig);

        // Then: Retrieved user should match original data
        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser.Id, Is.EqualTo(originalUser.Id));
        Assert.That(retrievedUser.Email, Is.EqualTo(originalUser.Email));
        Assert.That(retrievedUser.FullName, Is.EqualTo(originalUser.FullName));

    }

    [Test]
    [DisplayName("Should verify user exists after adding")]
    public async Task ShouldVerifyUserExistsWhenUserAdded()
    {
        // Given: A new user
        User user = CreateValidUser();

        // When: Adding the user
        Result<string> addResult = await _userRepository.AddAsync(user);
        Assert.That(addResult.IsSuccess, Is.True);

        // And: Checking if user exists by email
        LoadConfig loadConfig = new()
        {
            OverrideTableName = _configuration.UsersTable
        };
        User? retrievedUser = await _dynamoContext.LoadAsync<User>(user.Id, loadConfig);

        // Then: User should exist
        Assert.That(retrievedUser, Is.Not.Null, "User should exist after adding");
        Assert.That(retrievedUser.Id, Is.EqualTo(user.Id), "User ID should match");
        Assert.That(retrievedUser.Email, Is.EqualTo(user.Email), "User email should match");

    }

    #endregion

    #region AddAsync - Duplicate User Cases

    [Test]
    [DisplayName("Should overwrite user when same user ID is added twice")]
    public async Task ShouldOverwriteUserWhenSameUserIdAddedTwice()
    {
        // Given: A user with original data
        string userId = Guid.NewGuid().ToString();
        User originalUser = new User(userId, "original@example.com", "Original Name");
        
        Result<string> firstResult = await _userRepository.AddAsync(originalUser);
        Assert.That(firstResult.IsSuccess, Is.True);

        // And: The same user ID but with different data
        User modifiedUser = new User(userId, "modified@example.com", "Modified Name");

        // When: Adding the user with same ID again
        Result<string> secondResult = await _userRepository.AddAsync(modifiedUser);

        // Then: Should succeed (DynamoDB overwrites by default)
        Assert.That(secondResult.IsSuccess, Is.True);
        Assert.That(secondResult.Value, Is.EqualTo(userId));

        // And: The user data should be updated to the new values
        LoadConfig loadConfig = new LoadConfig
        {
            OverrideTableName = _configuration.UsersTable
        };
        User? retrievedUser = await _dynamoContext.LoadAsync<User>(userId, loadConfig);

        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser.Id, Is.EqualTo(userId));
        Assert.That(retrievedUser.Email, Is.EqualTo("modified@example.com")); // Should be the new email
        Assert.That(retrievedUser.FullName, Is.EqualTo("Modified Name")); // Should be the new name
        Assert.That(retrievedUser.Email, Is.Not.EqualTo("original@example.com")); // Should not be the original email
    }

    [Test]
    [DisplayName("Should handle duplicate user with same email but different ID")]
    public async Task ShouldAllowDifferentUsersWithSameEmail()
    {
        // Given: Two different users with the same email
        string email = "duplicate@example.com";
        User firstUser = new User(Guid.NewGuid().ToString(), email, "First User");
        User secondUser = new User(Guid.NewGuid().ToString(), email, "Second User");

        // When: Adding both users
        Result<string> firstResult = await _userRepository.AddAsync(firstUser);
        Result<string> secondResult = await _userRepository.AddAsync(secondUser);

        // Then: Both should succeed (different IDs, same email is allowed in DynamoDB)
        Assert.That(firstResult.IsSuccess, Is.True);
        Assert.That(secondResult.IsSuccess, Is.True);
        Assert.That(firstResult.Value, Is.Not.EqualTo(secondResult.Value)); // Different IDs

        // And: Both users should exist in the database
        LoadConfig loadConfig = new LoadConfig
        {
            OverrideTableName = _configuration.UsersTable
        };
        
        User? retrievedFirstUser = await _dynamoContext.LoadAsync<User>(firstUser.Id, loadConfig);
        User? retrievedSecondUser = await _dynamoContext.LoadAsync<User>(secondUser.Id, loadConfig);

        Assert.That(retrievedFirstUser, Is.Not.Null);
        Assert.That(retrievedSecondUser, Is.Not.Null);
        Assert.That(retrievedFirstUser.Email, Is.EqualTo(email));
        Assert.That(retrievedSecondUser.Email, Is.EqualTo(email));
        Assert.That(retrievedFirstUser.FullName, Is.EqualTo("First User"));
        Assert.That(retrievedSecondUser.FullName, Is.EqualTo("Second User"));
    }

    [Test]
    [DisplayName("Should maintain consistency when adding duplicate user concurrently")]
    public async Task ShouldMaintainConsistencyWhenAddingDuplicateUserConcurrently()
    {
        // Given: The same user data for concurrent operations
        string userId = Guid.NewGuid().ToString();
        string userEmail = "concurrent@example.com";
        
        User user1 = new User(userId, userEmail, "Concurrent User 1");
        User user2 = new User(userId, userEmail, "Concurrent User 2");
        User user3 = new User(userId, userEmail, "Concurrent User 3");

        // When: Adding the same user ID concurrently (race condition scenario)
        Task<Result<string>>[] tasks =
        [
            _userRepository.AddAsync(user1),
            _userRepository.AddAsync(user2),
            _userRepository.AddAsync(user3)
        ];

        Result<string>[] results = await Task.WhenAll(tasks);

        // Then: All operations should succeed
        foreach (Result<string> result in results)
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(userId));
        }

        // And: One of the users should be persisted (last write wins)
        LoadConfig loadConfig = new LoadConfig
        {
            OverrideTableName = _configuration.UsersTable
        };
        User? retrievedUser = await _dynamoContext.LoadAsync<User>(userId, loadConfig);

        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser.Id, Is.EqualTo(userId));
        Assert.That(retrievedUser.Email, Is.EqualTo(userEmail));
        
        // The name should be one of the three (whichever won the race)
        string[] possibleNames = new[] { "Concurrent User 1", "Concurrent User 2", "Concurrent User 3" };
        Assert.That(possibleNames, Contains.Item(retrievedUser.FullName));
    }

    [Test]
    [DisplayName("Should verify data overwrite behavior with timestamps")]
    public async Task ShouldVerifyDataOverwriteBehaviorWithTimestamps()
    {
        // Given: A user added at time T1
        string userId = Guid.NewGuid().ToString();
        User firstUser = new User(userId, "first@example.com", "First Version");
        
        Result<string> firstResult = await _userRepository.AddAsync(firstUser);
        Assert.That(firstResult.IsSuccess, Is.True);

        // And: Wait a small amount to ensure different timestamps
        await Task.Delay(100);

        // And: The same user ID with updated data at time T2
        User secondUser = new User(userId, "second@example.com", "Second Version");

        // When: Adding the updated user
        Result<string> secondResult = await _userRepository.AddAsync(secondUser);

        // Then: Should succeed and overwrite
        Assert.That(secondResult.IsSuccess, Is.True);

        // And: Only the latest data should persist
        LoadConfig loadConfig = new LoadConfig
        {
            OverrideTableName = _configuration.UsersTable
        };
        User? finalUser = await _dynamoContext.LoadAsync<User>(userId, loadConfig);

        Assert.That(finalUser, Is.Not.Null);
        Assert.That(finalUser.Email, Is.EqualTo("second@example.com"));
        Assert.That(finalUser.FullName, Is.EqualTo("Second Version"));
        
        // Verify the original data is completely gone
        Assert.That(finalUser.Email, Is.Not.EqualTo("first@example.com"));
        Assert.That(finalUser.FullName, Is.Not.EqualTo("First Version"));
    }

    #endregion

    #region AddAsync - Concurrent Operations

    [Test]
    [DisplayName("Should handle concurrent user additions")]
    public async Task ShouldHandleConcurrentAdditionsWhenMultipleUsersAddedSimultaneously()
    {
        // Given: Multiple different users
        List<User> users = new List<User>();
        List<Task<Result<string>>> tasks = new List<Task<Result<string>>>();

        for (int i = 0; i < 5; i++)
        {
            User user = new User(
                Guid.NewGuid().ToString(),
                $"concurrent{i}@{_faker.Internet.DomainName()}",
                $"Concurrent User {i}"
            );
            users.Add(user);
            tasks.Add(_userRepository.AddAsync(user));
        }

        // When: Adding all users concurrently
        Result<string>[] results = await Task.WhenAll(tasks);

        // Then: All additions should succeed
        for (int i = 0; i < results.Length; i++)
        {
            Assert.That(results[i].IsSuccess, Is.True, $"User {i} addition should succeed");
            Assert.That(results[i].Value, Is.EqualTo(users[i].Id));
        }
    }

    #endregion

    #region AddAsync - Edge Cases

    [Test]
    [DisplayName("Should handle user with minimum required data")]
    public async Task ShouldHandleUserWithMinimumDataWhenOnlyRequiredFieldsProvided()
    {
        // Given: A user with minimum required data
        User user = new User(
            Guid.NewGuid().ToString(),
            "minimal@example.com",
            "M" // Single character name
        );

        // When: Adding the user
        Result<string> result = await _userRepository.AddAsync(user);

        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(user.Id));
    }

    [Test]
    [DisplayName("Should handle user with empty GUID")]
    public async Task ShouldHandleUserWhenEmptyGuidProvided()
    {
        // Given: A user with empty GUID as ID
        User user = new User(
            Guid.Empty.ToString(),
            _faker.Internet.Email(),
            _faker.Person.FullName
        );

        // When: Adding the user
        Result<string> result = await _userRepository.AddAsync(user);

        // Then: Should handle gracefully (might succeed with empty ID)
        Assert.That(result.IsFailure, Is.False, "User with empty GUID should be handled gracefully");
    }

    #endregion
}
