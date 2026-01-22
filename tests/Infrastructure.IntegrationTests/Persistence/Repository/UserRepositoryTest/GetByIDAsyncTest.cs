using System.ComponentModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.UserRepositoryTest;

[TestFixture]
public class GetByIDAsyncTest : GenericUserRepositoryTest
{
     [Test]
    [DisplayName("Should return user when a valid ID is provided")]
    public async Task ShouldReturnUserWhenValidIdProvided()
    {
        // Given: A user already persisted in the database
        User originalUser = CreateValidUser();
        await _userRepository.AddAsync(originalUser);

        // When: Retrieving the user by ID
        Result<User> result = await _userRepository.GetByIdAsync(originalUser.Id);

        // Then: The operation should be successful and data should match
        Assert.That(result.IsSuccess, Is.True, "The retrieval should be successful");
        Assert.That(result.Value.Id, Is.EqualTo(originalUser.Id));
        Assert.That(result.Value.Email, Is.EqualTo(originalUser.Email));
        Assert.That(result.Value.FullName, Is.EqualTo(originalUser.FullName));

        // Cleanup
        await CleanupUser(originalUser.Id);
    }

    [Test]
    [DisplayName("Should return failure when user ID does not exist")]
    public async Task ShouldReturnFailureWhenUserIdDoesNotExist()
    {
        // Given: An ID that is not in the database
        string nonExistentId = Guid.NewGuid().ToString();

        // When: Attempting to retrieve the user
        Result<User> result = await _userRepository.GetByIdAsync(nonExistentId);

        // Then: It should return a failure with UserNotFound error
        Assert.That(result.IsFailure, Is.True, "The operation should fail for a non-existent ID");
        Assert.That(result.Error, Is.EqualTo(InfrastructureUserErrors.UserNotFound));
    }

    [Test]
    [DisplayName("Should handle special characters in ID correctly")]
    public async Task ShouldHandleSpecialCharactersInIdCorrectly()
    {
        // Given: A user with a complex ID (if business logic allows it, otherwise just a Guid string)
        string complexId = $"user#test#{Guid.NewGuid()}";
        User user = new User(complexId, _faker.Internet.Email(), _faker.Person.FullName);
        await _userRepository.AddAsync(user);

        // When: Retrieving the user
        Result<User> result = await _userRepository.GetByIdAsync(complexId);

        // Then: It should find the user correctly
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(complexId));

        // Cleanup
        await CleanupUser(complexId);
    }
}
