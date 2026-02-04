using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.UserRepositoryTest;

[TestFixture]
public class GetByEmailAsyncTest : GenericUserRepositoryTest
{
    
    [Test]
    [DisplayName("Should return true when user exists by email")]
    public async Task ShouldReturnTrueWhenUserExistsByEmail()
    {
        // Given: An email that exists in DB
        const string email = "exists@test.com";
        List<UserDBModel>
            models = [new() { Email = email }]; // ExistsByEmailAsync usa List<User> según tu código

        Mock<IAsyncSearch<UserDBModel>> searchMock = new Mock<IAsyncSearch<UserDBModel>>();
        searchMock.Setup(s => s.GetRemainingAsync(CancellationToken.None)).ReturnsAsync(models);

        ContextMock.Setup(x => x.QueryAsync<UserDBModel>(email, It.IsAny<QueryConfig>()))
            .Returns(searchMock.Object);

        // When: Checking existence
        Result<bool> result = await Repository.ExistsByEmailAsync(email);

        // Then: Should return true
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);
    }

    [Test]
    [DisplayName("Should return false when user does not exist by email")]
    public async Task ShouldReturnFalseWhenUserDoesNotExistByEmail()
    {
        // Given: An email not in DB
        const string email = "none@test.com";
        Mock<IAsyncSearch<UserDBModel>> searchMock = new Mock<IAsyncSearch<UserDBModel>>();
        searchMock.Setup(s => s.GetRemainingAsync(CancellationToken.None)).ReturnsAsync(new List<UserDBModel>());

        ContextMock.Setup(x => x.QueryAsync<UserDBModel>(email, It.IsAny<QueryConfig>()))
            .Returns(searchMock.Object);

        // When: Checking existence
        Result<bool> result = await Repository.ExistsByEmailAsync(email);

        // Then: Should return false
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.False);
    }
}
