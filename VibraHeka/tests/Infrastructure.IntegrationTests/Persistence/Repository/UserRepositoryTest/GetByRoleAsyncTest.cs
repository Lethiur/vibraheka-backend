using System.ComponentModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

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
        User user1 = CreateValidUser();
        user1.Role = role;
        User user2 = CreateValidUser();
        user2.Role = role;

        await _userRepository.AddAsync(user1);
        await _userRepository.AddAsync(user2);

        // When: Retrieving users by role
        Result<IEnumerable<User>> result = await _userRepository.GetByRoleAsync(role);

        // Then: The operation should be successful and contain at least our two users
        Assert.That(result.IsSuccess, Is.True);
        List<User> users = result.Value.ToList();
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
        User user = CreateValidUser();
        user.Role = UserRole.Therapist;
        await _userRepository.AddAsync(user);

        // When: Retrieving by role
        Result<IEnumerable<User>> result = await _userRepository.GetByRoleAsync(UserRole.Therapist);

        // Then: The specific user should have all properties correctly mapped
        User retrievedUser = result.Value.First(u => u.Id == user.Id);
        Assert.That(retrievedUser.FullName, Is.EqualTo(user.FullName));
        Assert.That(retrievedUser.Email, Is.EqualTo(user.Email));
        Assert.That(retrievedUser.Role, Is.EqualTo(UserRole.Therapist));

        // Cleanup
        await CleanupUser(user.Id);
    }
}
