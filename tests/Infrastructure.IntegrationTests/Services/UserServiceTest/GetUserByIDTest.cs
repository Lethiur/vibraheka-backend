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
        UserProfileEntity originalUserProfile = new(
            Guid.NewGuid().ToString(),
            _faker.Internet.Email(),
            _faker.Person.FullName)
        {
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        await UserRepository.AddAsync(originalUserProfile);

        // When: Requesting user by id through the service
        Result<UserProfileEntity> result = await UserService.GetUserByID(originalUserProfile.Id, CancellationToken.None);

        // Then: It should succeed and return the same user
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Id, Is.EqualTo(originalUserProfile.Id));
        Assert.That(result.Value.Email, Is.EqualTo(originalUserProfile.Email));
        Assert.That(result.Value.FirstName, Is.EqualTo(originalUserProfile.FirstName));

    }

    [TestCase(null, TestName = "Null user id")]
    [TestCase("", TestName = "Empty user id")]
    [TestCase("   ", TestName = "Whitespace user id")]
    [DisplayName("Should fail with InvalidUserID when user id is invalid")]
    public async Task ShouldFailWithInvalidUserIdWhenUserIdIsInvalid(string? userId)
    {
        // When
        Result<UserProfileEntity> result = await UserService.GetUserByID(userId!, CancellationToken.None);

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
        Result<UserProfileEntity> result = await UserService.GetUserByID(nonExistentId, CancellationToken.None);

        // Then: depending on repository behavior, the error may come from service or repo
        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.AnyOf(UserErrors.UserNotFound, InfrastructureUserErrors.UserNotFound)
        );
    }
}
