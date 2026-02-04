using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.UserRepositoryTest;

[TestFixture]
public class GetByIDAsyncTest: GenericUserRepositoryTest
{

    [Test]
    [DisplayName("Should return successful result when user is found in DynamoDB")]
    public async Task ShouldReturnSuccessfulResultWhenUserIsFound()
    {
        // Given: A valid ID and a corresponding model in DynamoDB
        const string userId = "test-id";
        UserDBModel userModel = new UserDBModel 
        { 
            Id = userId, 
            Email = "test@example.com", 
            FullName = "Test User",
            Role = UserRole.Therapist
        };

        ContextMock.Setup(x => x.LoadAsync<UserDBModel>(userId, It.IsAny<LoadConfig>(), CancellationToken.None))
            .ReturnsAsync(userModel);

        // When: Getting the user by ID
        Result<User> result = await Repository.GetByIdAsync(userId);

        // Then: Result should be success and domain entity should match
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(userId));
        Assert.That(result.Value.Email, Is.EqualTo(userModel.Email));
        Assert.That(result.Value.Role, Is.EqualTo(UserRole.Therapist));
    }

    [Test]
    [DisplayName("Should return failure when DynamoDB returns null")]
    public async Task ShouldReturnFailureWhenUserNotFound()
    {
        // Given: An ID that doesn't exist in DynamoDB
        const string userId = "non-existent";
        ContextMock.Setup(x => x.LoadAsync<UserDBModel>(userId, It.IsAny<LoadConfig>(), CancellationToken.None))
            .ReturnsAsync((UserDBModel)null!);

        // When: Getting the user by ID
        Result<User> result = await Repository.GetByIdAsync(userId);

        // Then: Result should be failure with UserNotFound error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureUserErrors.UserNotFound));
    }

    [Test]
    [DisplayName("Should return failure when DynamoDB throws an exception")]
    public async Task ShouldReturnFailureWhenExceptionOccurs()
    {
        // Given: A database error
        const string userId = "any-id";
        const string errorMessage = "DynamoDB Connection Error";
        ContextMock.Setup(x => x.LoadAsync<UserDBModel>(userId, It.IsAny<LoadConfig>(), CancellationToken.None))
            .ThrowsAsync(new Exception(errorMessage));

        // When: Getting the user by ID
        Result<User> result = await Repository.GetByIdAsync(userId);

        // Then: Result should be failure and contain the exception message
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(errorMessage));
    }
}
