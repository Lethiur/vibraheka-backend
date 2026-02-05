using System.ComponentModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.UserRepositoryTest;

[TestFixture]
public class GetByRoleAsyncTest : GenericUserRepositoryTest
{
    [Test]
    [DisplayName("Should return a list of users when users with the specified role exist")]
    public async Task ShouldReturnListOfUsersWhenUsersWithRoleExist()
    {
        // Given: Multiple users with the same role persisted in DynamoDB
        UserRole role = UserRole.Therapist;
        UserEntity user1 = CreateValidUser();
        user1.Role = role;
        UserEntity user2 = CreateValidUser();
        user2.Role = role;

        await _userRepository.AddAsync(user1);
        await _userRepository.AddAsync(user2);

        // When: Retrieving users by role
        Result<IEnumerable<UserEntity>> result = await _userRepository.GetByRoleAsync(role);

        // Then: The operation should be successful and contain at least our two users
        Assert.That(result.IsSuccess, Is.True);
        List<UserEntity> users = result.Value.ToList();
        Assert.That(users.Any(u => u.Id == user1.Id), Is.True, "Should contain the first user");
        Assert.That(users.Any(u => u.Id == user2.Id), Is.True, "Should contain the second user");
        Assert.That(users.All(u => u.Role == role), Is.True, "All returned users should have the requested role");

        // Cleanup
        await CleanupUser(user1.Id);
        await CleanupUser(user2.Id);
    }
    
    [Test]
    [DisplayName("Should correctly map all domain properties when retrieving by role")]
    public async Task ShouldCorrectlyMapAllPropertiesWhenRetrievingByRole()
    {
        // Given: A user with full data
        UserEntity userEntity = CreateValidUser();
        userEntity.Role = UserRole.Therapist;
        await _userRepository.AddAsync(userEntity);

        // When: Retrieving by role
        Result<IEnumerable<UserEntity>> result = await _userRepository.GetByRoleAsync(UserRole.Therapist);

        // Then: The specific user should have all properties correctly mapped
        UserEntity retrievedUserEntity = result.Value.First(u => u.Id == userEntity.Id);
        Assert.That(retrievedUserEntity.FirstName, Is.EqualTo(userEntity.FirstName));
        Assert.That(retrievedUserEntity.Email, Is.EqualTo(userEntity.Email));
        Assert.That(retrievedUserEntity.Role, Is.EqualTo(UserRole.Therapist));

        // Cleanup
        await CleanupUser(userEntity.Id);
    }
}
