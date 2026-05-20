using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.UserRepositoryTest;

[TestFixture]
public class GetByRoleAsyncTest : GenericUserRepositoryTest
{

    [Test]
    [DisplayName("Should return list of users when role matches")]
    public async Task ShouldReturnListWhenRoleMatches()
    {
        // Given: A role with users
        const UserRole role = UserRole.Therapist;
        List<UserDBModel> models =
        [
            new() { Id = "1", Role = role },
            new() { Id = "2", Role = role }
        ];

        Mock<IAsyncSearch<UserDBModel>> searchMock = new();
        searchMock.Setup(s => s.GetRemainingAsync(CancellationToken.None)).ReturnsAsync(models);

        ContextMock.Setup(x => x.QueryAsync<UserDBModel>(role.ToString(), It.IsAny<QueryConfig>()))
            .Returns(searchMock.Object);

        // When: Getting by role
        Result<List<UserEntity>> result = await Repository.GetByRoleAsync(role);

        // Then: Should return success with 2 users
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count, Is.EqualTo(2));
        ContextMock.Verify(x => x.QueryAsync<UserDBModel>(role.ToString(), It.IsAny<QueryConfig>()), Times.Once);
        searchMock.Verify(s => s.GetRemainingAsync(CancellationToken.None), Times.Once);
    }

    [Test]
    [DisplayName("Should return empty list when no users have the role")]
    public async Task ShouldReturnEmptyListWhenNoUsersHaveRole()
    {
        // Given: A role with no users
        Mock<IAsyncSearch<UserDBModel>> searchMock = new();
        searchMock.Setup(s => s.GetRemainingAsync(CancellationToken.None)).ReturnsAsync([]);

        ContextMock.Setup(x => x.QueryAsync<UserDBModel>(nameof(UserRole.Admin), It.IsAny<QueryConfig>()))
            .Returns(searchMock.Object);

        // When: Getting by role
        Result<List<UserEntity>> result = await Repository.GetByRoleAsync(UserRole.Admin);

        // Then: Should return success with empty collection
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));
        ContextMock.Verify(x => x.QueryAsync<UserDBModel>("Admin", It.IsAny<QueryConfig>()), Times.Once);
        searchMock.Verify(s => s.GetRemainingAsync(CancellationToken.None), Times.Once);
    }

    [Test]
    [DisplayName("Should return failure when QueryAsync throws exception")]
    public async Task ShouldReturnFailureWhenQueryAsyncThrows()
    {
        // Given: An exception
        ContextMock.Setup(x => x.QueryAsync<UserDBModel>(It.IsAny<string>(), It.IsAny<QueryConfig>()))
            .Throws(new Exception("Query Error"));

        // When: Getting by role
        Result<List<UserEntity>> result = await Repository.GetByRoleAsync(UserRole.Therapist);

        // Then: Should fail and contain the message
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));
        ContextMock.Verify(x => x.QueryAsync<UserDBModel>(nameof(UserRole.Therapist), It.IsAny<QueryConfig>()), Times.Once);
    }

}
