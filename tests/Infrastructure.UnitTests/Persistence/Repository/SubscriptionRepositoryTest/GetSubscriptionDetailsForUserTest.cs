using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SubscriptionRepositoryTest;

[TestFixture]
public class GetSubscriptionDetailsForUserTest : GenericSubscriptionRepositoryTest
{
    [Test]
    public async Task ShouldReturnMappedSubscriptionWhenRecordExists()
    {
        // Given
        SubscriptionDBModel dbModel = new()
        {
            SubscriptionID = "sub-1",
            UserID = "user-1",
            ExternalSubscriptionItemID = "price-1",
            ExternalCustomerID = "cus-1",
            Status = OrderStatus.Pending,
            SubscriptionStatus = SubscriptionStatus.Created
        };

        Mock<IAsyncSearch<SubscriptionDBModel>> searchMock = new();
        searchMock.Setup(x => x.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([dbModel]);

        ContextMock.Setup(x => x.QueryAsync<SubscriptionDBModel>("user-1", It.IsAny<QueryConfig>()))
            .Returns(searchMock.Object);

        // When
        Result<SubscriptionEntity> result = await Repository.GetSubscriptionDetailsForUser("user-1", CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.SubscriptionID, Is.EqualTo("sub-1"));
        Assert.That(result.Value.UserID, Is.EqualTo("user-1"));
    }

    [Test]
    public async Task ShouldReturnNoSubscriptionFoundWhenNoRecordsExist()
    {
        // Given
        Mock<IAsyncSearch<SubscriptionDBModel>> searchMock = new();
        searchMock.Setup(x => x.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        ContextMock.Setup(x => x.QueryAsync<SubscriptionDBModel>("user-1", It.IsAny<QueryConfig>()))
            .Returns(searchMock.Object);

        // When
        Result<SubscriptionEntity> result = await Repository.GetSubscriptionDetailsForUser("user-1", CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SubscriptionErrors.NoSubscriptionFound));
    }

    [Test]
    public async Task ShouldReturnUnknownErrorWhenQueryThrowsUnexpectedException()
    {
        // Given
        ContextMock.Setup(x => x.QueryAsync<SubscriptionDBModel>("user-1", It.IsAny<QueryConfig>()))
            .Throws(new Exception("boom"));

        // When
        Result<SubscriptionEntity> result = await Repository.GetSubscriptionDetailsForUser("user-1", CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AppErrors.UnknownError));
    }
}
