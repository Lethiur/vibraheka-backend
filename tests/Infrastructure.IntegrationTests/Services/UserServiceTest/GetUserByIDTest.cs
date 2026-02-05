using System.ComponentModel;
using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.UserServiceTest;

[TestFixture]
public class GetUserByIDTest : GenericCognitoServiceTest
{
    [Test]
    [DisplayName("Should return user when user exists in DynamoDB")]
    public async Task ShouldReturnUserWhenUserExistsInDynamoDb()
    {
        // Given: A user persisted in DynamoDB
        UserEntity originalUser = new UserEntity(
            Guid.NewGuid().ToString(),
            _faker.Internet.Email(),
            _faker.Person.FullName)
        {
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        await _userRepository.AddAsync(originalUser);

        // When: Requesting user by id through the service
        Result<UserEntity> result = await _userService.GetUserByID(originalUser.Id, CancellationToken.None);

        // Then: It should succeed and return the same user
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Id, Is.EqualTo(originalUser.Id));
        Assert.That(result.Value.Email, Is.EqualTo(originalUser.Email));
        Assert.That(result.Value.FirstName, Is.EqualTo(originalUser.FirstName));

    }

    [TestCase(null, TestName = "Null user id")]
    [TestCase("", TestName = "Empty user id")]
    [TestCase("   ", TestName = "Whitespace user id")]
    [DisplayName("Should fail with InvalidUserID when user id is invalid")]
    public async Task ShouldFailWithInvalidUserIdWhenUserIdIsInvalid(string? userId)
    {
        // When
        Result<UserEntity> result = await _userService.GetUserByID(userId!, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidUserID));
    }

    [Test]
    [DisplayName("Should fail when user does not exist")]
    public async Task ShouldFailWhenUserDoesNotExist()
    {
        // Given: A valid but non-existent id
        string nonExistentId = Guid.NewGuid().ToString();

        // When
        Result<UserEntity> result = await _userService.GetUserByID(nonExistentId, CancellationToken.None);

        // Then: depending on repository behavior, the error may come from service or repo
        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.AnyOf(UserErrors.UserNotFound, InfrastructureUserErrors.UserNotFound)
        );
    }
}
