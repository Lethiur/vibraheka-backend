using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using Moq;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.UserRepositoryTest;

[TestFixture]
public class ExistByEmailAsyncTest: GenericUserRepositoryTest
{
   
     [Test]
    [DisplayName("Should return true when user exists by email in DynamoDB")]
    public async Task ShouldReturnTrueWhenUserExistsByEmail()
    {
        // Given: An email and a search that returns at least one result
        const string email = "exists@test.com";
        List<UserDBModel> models = new List<UserDBModel> { new() { Email = email } };

        Mock<IAsyncSearch<UserDBModel>> searchMock = new Mock<IAsyncSearch<UserDBModel>>();
        searchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(models);
        
        ContextMock.Setup(x => x.QueryAsync<UserDBModel>(email, It.IsAny<QueryConfig>()))
            .Returns(searchMock.Object);

        // When: Checking if the email exists
        Result<bool> result = await Repository.ExistsByEmailAsync(email);

        // Then: The result value should be true
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);
        ContextMock.Verify(x => x.QueryAsync<UserDBModel>(email, It.Is<QueryConfig>(q => q.IndexName == "EmailIndex")), Times.Once);
    }

    [Test]
    [DisplayName("Should return false when user does not exist by email")]
    public async Task ShouldReturnFalseWhenUserDoesNotExistByEmail()
    {
        // Given: An email and a search that returns no results
        const string email = "none@test.com";
        Mock<IAsyncSearch<UserDBModel>> searchMock = new Mock<IAsyncSearch<UserDBModel>>();
        searchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserDBModel>());
        
        ContextMock.Setup(x => x.QueryAsync<UserDBModel>(email, It.IsAny<QueryConfig>()))
            .Returns(searchMock.Object);

        // When: Checking if the email exists
        Result<bool> result = await Repository.ExistsByEmailAsync(email);

        // Then: The result value should be false
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.False);
    }

    [Test]
    [DisplayName("Should return false when DynamoDB result is null")]
    public async Task ShouldReturnFalseWhenDynamoResultIsNull()
    {
        // Given: A search that returns a null list
        const string email = "null@test.com";
        Mock<IAsyncSearch<UserDBModel>> searchMock = new Mock<IAsyncSearch<UserDBModel>>();
        searchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<UserDBModel>)null!);
        
        ContextMock.Setup(x => x.QueryAsync<UserDBModel>(email, It.IsAny<QueryConfig>()))
            .Returns(searchMock.Object);

        // When: Checking if the email exists
        Result<bool> result = await Repository.ExistsByEmailAsync(email);

        // Then: Should handle null gracefully and return false
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.False);
    }

}
