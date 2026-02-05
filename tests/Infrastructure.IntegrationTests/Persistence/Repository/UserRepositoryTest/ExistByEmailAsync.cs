using System.ComponentModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.UserRepositoryTest;

[TestFixture]
public class ExistByEmailAsync : GenericUserRepositoryTest
{
    [Test]
    [DisplayName("Should return true when a user with the specified email exists")]
    public async Task ShouldReturnTrueWhenUserWithEmailExists()
    {
        // Given: A user persisted in DynamoDB
        UserEntity userEntity = CreateValidUser();
        await _userRepository.AddAsync(userEntity);

        // When: Checking if the email exists
        Result<bool> result = await _userRepository.ExistsByEmailAsync(userEntity.Email);

        // Then: The result should be true
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True, "Should find the user by their email");

        // Cleanup
        await CleanupUser(userEntity.Id);
    }

    [Test]
    [DisplayName("Should return false when no user with the specified email exists")]
    public async Task ShouldReturnFalseWhenUserWithEmailDoesNotExist()
    {
        // Given: An email that is not registered
        string nonExistentEmail = $"nonexistent.{Guid.NewGuid()}@test.com";

        // When: Checking if the email exists
        Result<bool> result = await _userRepository.ExistsByEmailAsync(nonExistentEmail);

        // Then: The result should be false
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.False, "Should not find a user with a fake email");
    }

    [Test]
    [DisplayName("Should handle case-sensitive or special character emails correctly")]
    public async Task ShouldHandleSpecialEmailsCorrectly()
    {
        // Given: A user with a complex email address
        string complexEmail = $"Test.User+Filter-{Guid.NewGuid()}@vibraheka.io";
        UserEntity userEntity = new UserEntity(Guid.NewGuid().ToString(), complexEmail, "Special Email User");
        await _userRepository.AddAsync(userEntity);

        // When: Checking existence
        Result<bool> result = await _userRepository.ExistsByEmailAsync(complexEmail);

        // Then: It should be found successfully
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);

        // Cleanup
        await CleanupUser(userEntity.Id);
    }
}
