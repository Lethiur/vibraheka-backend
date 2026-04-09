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
        UserProfileEntity originalUserProfileEntity = CreateValidUser();
        await _userRepository.AddAsync(originalUserProfileEntity);

        // When: Retrieving the user by ID
        Result<UserProfileEntity> result = await _userRepository.GetByIdAsync(originalUserProfileEntity.Id, CancellationToken.None);

        // Then: The operation should be successful and data should match
        Assert.That(result.IsSuccess, Is.True, "The retrieval should be successful");
        Assert.That(result.Value.Id, Is.EqualTo(originalUserProfileEntity.Id));
        Assert.That(result.Value.Email, Is.EqualTo(originalUserProfileEntity.Email));
        Assert.That(result.Value.FirstName, Is.EqualTo(originalUserProfileEntity.FirstName));

        // Cleanup
        await CleanupUser(originalUserProfileEntity.Id);
    }

    [Test]
    [DisplayName("Should return failure when user ID does not exist")]
    public async Task ShouldReturnFailureWhenUserIdDoesNotExist()
    {
        // Given: An ID that is not in the database
        string nonExistentId = Guid.NewGuid().ToString();

        // When: Attempting to retrieve the user
        Result<UserProfileEntity> result = await _userRepository.GetByIdAsync(nonExistentId, CancellationToken.None);

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
        UserProfileEntity userProfileEntity = new(complexId, _faker.Internet.Email(), _faker.Person.FullName);
        await _userRepository.AddAsync(userProfileEntity);

        // When: Retrieving the user
        Result<UserProfileEntity> result = await _userRepository.GetByIdAsync(complexId, CancellationToken.None);

        // Then: It should find the user correctly
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(complexId));

        // Cleanup
        await CleanupUser(complexId);
    }

    [Test]
    [DisplayName("Should return failure when operation is cancelled")]
    public async Task ShouldReturnFailureWhenOperationIsCancelled()
    {
        // Given: un token de cancelacion ya cancelado.
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // When: se intenta obtener usuario con operacion cancelada.
        Result<UserProfileEntity> result = await _userRepository.GetByIdAsync(Guid.NewGuid().ToString(), cts.Token);

        // Then: el repositorio debe devolver failure con mensaje de excepcion.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.Not.Null.And.Not.Empty);
    }
}
