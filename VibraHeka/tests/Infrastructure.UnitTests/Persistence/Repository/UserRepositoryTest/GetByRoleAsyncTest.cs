using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using Moq;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.UserRepositoryTest;

[TestFixture]
public class GetByRoleAsyncTest
{
    private Mock<IDynamoDBContext> ContextMock;
    private Mock<IConfiguration> ConfigMock;
    private UserRepository Repository;

    [SetUp]
    public void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        ConfigMock = new Mock<IConfiguration>();

        ConfigMock.Setup(c => c["Dynamo:UsersTable"]).Returns("TestUsersTable");

        Repository = new UserRepository(ContextMock.Object, ConfigMock.Object);
    }
    
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

        Mock<IAsyncSearch<UserDBModel>> searchMock = new Mock<IAsyncSearch<UserDBModel>>();
        searchMock.Setup(s => s.GetRemainingAsync(default)).ReturnsAsync(models);

        ContextMock.Setup(x => x.QueryAsync<UserDBModel>(role, It.IsAny<QueryConfig>()))
            .Returns(searchMock.Object);

        // When: Getting by role
        Result<IEnumerable<User>> result = await Repository.GetByRoleAsync(role);

        // Then: Should return success with 2 users
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count(), Is.EqualTo(2));
    }

    [Test]
    [DisplayName("Should return empty list when no users have the role")]
    public async Task ShouldReturnEmptyListWhenNoUsersHaveRole()
    {
        // Given: A role with no users
        Mock<IAsyncSearch<UserDBModel>> searchMock = new Mock<IAsyncSearch<UserDBModel>>();
        searchMock.Setup(s => s.GetRemainingAsync(default)).ReturnsAsync(new List<UserDBModel>());

        ContextMock.Setup(x => x.QueryAsync<UserDBModel>(It.IsAny<UserRole>(), It.IsAny<QueryConfig>()))
            .Returns(searchMock.Object);

        // When: Getting by role
        Result<IEnumerable<User>> result = await Repository.GetByRoleAsync(UserRole.Admin);

        // Then: Should return success with empty collection
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Empty);
    }

    [Test]
    [DisplayName("Should return failure when QueryAsync throws exception")]
    public async Task ShouldReturnFailureWhenQueryAsyncThrows()
    {
        // Given: An exception
        ContextMock.Setup(x => x.QueryAsync<UserDBModel>(It.IsAny<UserRole>(), It.IsAny<QueryConfig>()))
            .Throws(new Exception("Query Error"));

        // When: Getting by role
        Result<IEnumerable<User>> result = await Repository.GetByRoleAsync(UserRole.Therapist);

        // Then: Should fail and contain the message
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("Query Error"));
    }

}
