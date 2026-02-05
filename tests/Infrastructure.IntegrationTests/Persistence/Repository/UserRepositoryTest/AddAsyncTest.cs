using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

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
        UserEntity userEntity = CreateValidUser();

        // When: Adding the user to repository
        Result<string> result = await _userRepository.AddAsync(userEntity);

        // Then: Should return success with user ID
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(userEntity.Id));
        Assert.That(result.Value, Is.Not.Null.And.Not.Empty);

    }

    [Test]
    [DisplayName("Should add user successfully with complex email")]
    public async Task ShouldAddUserSuccessfullyWhenComplexEmailProvided()
    {
        // Given: A user with complex email format
        string email = $"test.user+complex@{_faker.Internet.DomainName()}";
        UserEntity userEntity = new UserEntity(Guid.NewGuid().ToString(), email, _faker.Person.FullName);

        // When: Adding the user
        Result<string> result = await _userRepository.AddAsync(userEntity);

        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(userEntity.Id));
        
    }

    [Test]
    [DisplayName("Should add user successfully with special characters in name")]
    public async Task ShouldAddUserSuccessfullyWhenSpecialCharactersInName()
    {
        // Given: A user with special characters in name
        UserEntity userEntity = new UserEntity(
            Guid.NewGuid().ToString(), 
            _faker.Internet.Email(), 
            "José María O'Connor-Smith"
        );

        // When: Adding the user
        Result<string> result = await _userRepository.AddAsync(userEntity);

        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(userEntity.Id));

    }

    [Test]
    [DisplayName("Should add user successfully with long name")]
    public async Task ShouldAddUserSuccessfullyWhenLongNameProvided()
    {
        // Given: A user with a very long name
        string longName = new string('A', 100) + " " + new string('B', 100);
        UserEntity userEntity = new UserEntity(Guid.NewGuid().ToString(), _faker.Internet.Email(), longName);

        // When: Adding the user
        Result<string> result = await _userRepository.AddAsync(userEntity);

        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(userEntity.Id));

    }

    #endregion

    #region AddAsync - Data Persistence Verification

    [Test]
    [DisplayName("Should persist user data correctly in DynamoDB")]
    public async Task ShouldPersistUserDataCorrectlyWhenUserAdded()
    {
        // Given: A user with specific data
        UserEntity originalUserEntity = CreateValidUser();

        // When: Adding the user
        Result<string> addResult = await _userRepository.AddAsync(originalUserEntity);
        Assert.That(addResult.IsSuccess, Is.True);

        // And: Retrieving the user directly from DynamoDB
        LoadConfig loadConfig = new()
        {
            OverrideTableName = _configuration.UsersTable
        };
        UserDBModel? retrievedUser = await _dynamoContext.LoadAsync<UserDBModel>(originalUserEntity.Id, loadConfig);

        // Then: Retrieved user should match original data
        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser.Id, Is.EqualTo(originalUserEntity.Id));
        Assert.That(retrievedUser.Email, Is.EqualTo(originalUserEntity.Email));
        Assert.That(retrievedUser.FirstName, Is.EqualTo(originalUserEntity.FirstName));

    }

    [Test]
    [DisplayName("Should verify user exists after adding")]
    public async Task ShouldVerifyUserExistsWhenUserAdded()
    {
        // Given: A new user
        UserEntity userEntity = CreateValidUser();

        // When: Adding the user
        Result<string> addResult = await _userRepository.AddAsync(userEntity);
        Assert.That(addResult.IsSuccess, Is.True);

        // And: Checking if user exists by email
        LoadConfig loadConfig = new()
        {
            OverrideTableName = _configuration.UsersTable
        };
        UserDBModel? retrievedUser = await _dynamoContext.LoadAsync<UserDBModel>(userEntity.Id, loadConfig);

        // Then: User should exist
        Assert.That(retrievedUser, Is.Not.Null, "User should exist after adding");
        Assert.That(retrievedUser.Id, Is.EqualTo(userEntity.Id), "User ID should match");
        Assert.That(retrievedUser.Email, Is.EqualTo(userEntity.Email), "User email should match");

    }

    #endregion

    #region AddAsync - Duplicate User Cases

    [Test]
    [DisplayName("Should overwrite user when same user ID is added twice")]
    public async Task ShouldOverwriteUserWhenSameUserIdAddedTwice()
    {
        // Given: A user with original data
        string userId = Guid.NewGuid().ToString();
        UserEntity originalUserEntity = new UserEntity(userId, "original@example.com", "Original Name");
        
        Result<string> firstResult = await _userRepository.AddAsync(originalUserEntity);
        Assert.That(firstResult.IsSuccess, Is.True);

        // And: The same user ID but with different data
        UserEntity modifiedUserEntity = new UserEntity(userId, "modified@example.com", "Modified Name");

        // When: Adding the user with same ID again
        Result<string> secondResult = await _userRepository.AddAsync(modifiedUserEntity);

        // Then: Should succeed (DynamoDB overwrites by default)
        Assert.That(secondResult.IsSuccess, Is.True);
        Assert.That(secondResult.Value, Is.EqualTo(userId));

        // And: The user data should be updated to the new values
        LoadConfig loadConfig = new LoadConfig
        {
            OverrideTableName = _configuration.UsersTable
        };
        UserDBModel? retrievedUser = await _dynamoContext.LoadAsync<UserDBModel>(userId, loadConfig);

        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser.Id, Is.EqualTo(userId));
        Assert.That(retrievedUser.Email, Is.EqualTo("modified@example.com")); // Should be the new email
        Assert.That(retrievedUser.FirstName, Is.EqualTo("Modified Name")); // Should be the new name
        Assert.That(retrievedUser.Email, Is.Not.EqualTo("original@example.com")); // Should not be the original email
    }

    [Test]
    [DisplayName("Should handle duplicate user with same email but different ID")]
    public async Task ShouldAllowDifferentUsersWithSameEmail()
    {
        // Given: Two different users with the same email
        string email = "duplicate@example.com";
        UserEntity firstUserEntity = new UserEntity(Guid.NewGuid().ToString(), email, "First User");
        UserEntity secondUserEntity = new UserEntity(Guid.NewGuid().ToString(), email, "Second User");

        // When: Adding both users
        Result<string> firstResult = await _userRepository.AddAsync(firstUserEntity);
        Result<string> secondResult = await _userRepository.AddAsync(secondUserEntity);

        // Then: Both should succeed (different IDs, same email is allowed in DynamoDB)
        Assert.That(firstResult.IsSuccess, Is.True);
        Assert.That(secondResult.IsSuccess, Is.True);
        Assert.That(firstResult.Value, Is.Not.EqualTo(secondResult.Value)); // Different IDs

        // And: Both users should exist in the database
        LoadConfig loadConfig = new LoadConfig
        {
            OverrideTableName = _configuration.UsersTable
        };
        
        UserDBModel? retrievedFirstUser = await _dynamoContext.LoadAsync<UserDBModel>(firstUserEntity.Id, loadConfig);
        UserDBModel? retrievedSecondUser = await _dynamoContext.LoadAsync<UserDBModel>(secondUserEntity.Id, loadConfig);

        Assert.That(retrievedFirstUser, Is.Not.Null);
        Assert.That(retrievedSecondUser, Is.Not.Null);
        Assert.That(retrievedFirstUser.Email, Is.EqualTo(email));
        Assert.That(retrievedSecondUser.Email, Is.EqualTo(email));
        Assert.That(retrievedFirstUser.FirstName, Is.EqualTo("First User"));
        Assert.That(retrievedSecondUser.FirstName, Is.EqualTo("Second User"));
    }

    [Test]
    [DisplayName("Should maintain consistency when adding duplicate user concurrently")]
    public async Task ShouldMaintainConsistencyWhenAddingDuplicateUserConcurrently()
    {
        // Given: The same user data for concurrent operations
        string userId = Guid.NewGuid().ToString();
        string userEmail = "concurrent@example.com";
        
        UserEntity user1 = new UserEntity(userId, userEmail, "Concurrent User 1");
        UserEntity user2 = new UserEntity(userId, userEmail, "Concurrent User 2");
        UserEntity user3 = new UserEntity(userId, userEmail, "Concurrent User 3");

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
        UserEntity? retrievedUser = await _dynamoContext.LoadAsync<UserEntity>(userId, loadConfig);

        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser.Id, Is.EqualTo(userId));
        Assert.That(retrievedUser.Email, Is.EqualTo(userEmail));
        
        // The name should be one of the three (whichever won the race)
        string[] possibleNames = new[] { "Concurrent User 1", "Concurrent User 2", "Concurrent User 3" };
        Assert.That(possibleNames, Contains.Item(retrievedUser.FirstName));
    }

    [Test]
    [DisplayName("Should verify data overwrite behavior with timestamps")]
    public async Task ShouldVerifyDataOverwriteBehaviorWithTimestamps()
    {
        // Given: A user added at time T1
        string userId = Guid.NewGuid().ToString();
        UserEntity firstUserEntity = new UserEntity(userId, "first@example.com", "First Version");
        
        Result<string> firstResult = await _userRepository.AddAsync(firstUserEntity);
        Assert.That(firstResult.IsSuccess, Is.True);

        // And: Wait a small amount to ensure different timestamps
        await Task.Delay(100);

        // And: The same user ID with updated data at time T2
        UserEntity secondUserEntity = new UserEntity(userId, "second@example.com", "Second Version");

        // When: Adding the updated user
        Result<string> secondResult = await _userRepository.AddAsync(secondUserEntity);

        // Then: Should succeed and overwrite
        Assert.That(secondResult.IsSuccess, Is.True);

        // And: Only the latest data should persist
        LoadConfig loadConfig = new LoadConfig
        {
            OverrideTableName = _configuration.UsersTable
        };
        UserDBModel? finalUser = await _dynamoContext.LoadAsync<UserDBModel>(userId, loadConfig);

        Assert.That(finalUser, Is.Not.Null);
        Assert.That(finalUser.Email, Is.EqualTo("second@example.com"));
        Assert.That(finalUser.FirstName, Is.EqualTo("Second Version"));
        
        // Verify the original data is completely gone
        Assert.That(finalUser.Email, Is.Not.EqualTo("first@example.com"));
        Assert.That(finalUser.FirstName, Is.Not.EqualTo("First Version"));
    }

    #endregion

    #region AddAsync - Concurrent Operations

    [Test]
    [DisplayName("Should handle concurrent user additions")]
    public async Task ShouldHandleConcurrentAdditionsWhenMultipleUsersAddedSimultaneously()
    {
        // Given: Multiple different users
        List<UserEntity> users = new List<UserEntity>();
        List<Task<Result<string>>> tasks = new List<Task<Result<string>>>();

        for (int i = 0; i < 5; i++)
        {
            UserEntity userEntity = new UserEntity(
                Guid.NewGuid().ToString(),
                $"concurrent{i}@{_faker.Internet.DomainName()}",
                $"Concurrent User {i}"
            );
            users.Add(userEntity);
            tasks.Add(_userRepository.AddAsync(userEntity));
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
        UserEntity userEntity = new UserEntity(
            Guid.NewGuid().ToString(),
            "minimal@example.com",
            "M" // Single character name
        );

        // When: Adding the user
        Result<string> result = await _userRepository.AddAsync(userEntity);

        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(userEntity.Id));
    }

    [Test]
    [DisplayName("Should handle user with empty GUID")]
    public async Task ShouldHandleUserWhenEmptyGuidProvided()
    {
        // Given: A user with empty GUID as ID
        UserEntity userEntity = new UserEntity(
            Guid.Empty.ToString(),
            _faker.Internet.Email(),
            _faker.Person.FullName
        );

        // When: Adding the user
        Result<string> result = await _userRepository.AddAsync(userEntity);

        // Then: Should handle gracefully (might succeed with empty ID)
        Assert.That(result.IsFailure, Is.False, "User with empty GUID should be handled gracefully");
    }

    #endregion
}
