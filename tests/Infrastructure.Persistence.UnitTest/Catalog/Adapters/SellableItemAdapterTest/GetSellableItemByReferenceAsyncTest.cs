using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using NUnit.Framework;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Errors;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Catalog.Adapters.SellableItemAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetSellableItemByReferenceAsyncTest : GenericSellableItemAdapterTest
{
    [Test]
    [DisplayName("Should return Result.Success propagated from repository without alteration when entity is found")]
    public async Task ShouldReturnSuccessWhenRepositoryReturnsEntity()
    {
        // Given: DynamoDB context returns a valid SellableItemDBModel for the given referenceId
        string referenceId = "ref-id-success-adapter-001";
        SellableItemDBModel model = BuildDefaultSellableItemDBModel(referenceId);

        Mock<IAsyncSearch<SellableItemDBModel>> SearchMock = new();
        SearchMock
            .Setup(x => x.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([model]);

        ContextMock
            .Setup(x => x.QueryAsync<SellableItemDBModel>(referenceId, It.IsAny<QueryConfig>()))
            .Returns(SearchMock.Object);

        // When: GetSellableItemByReferenceAsync is called on the adapter
        Result<SellableItemEntity> result =
            await Adapter.GetSellableItemByReferenceAsync(referenceId, CancellationToken.None);

        // Then: result is Success and the entity data matches the model — no alteration by the adapter
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.SellableItemID, Is.EqualTo(model.SellableItemID),
            $"Expected SellableItemID '{model.SellableItemID}' but got '{result.Value.SellableItemID}'");
        Assert.That(result.Value.Name, Is.EqualTo(model.Name),
            $"Expected Name '{model.Name}' but got '{result.Value.Name}'");

        ContextMock.Verify(
            x => x.QueryAsync<SellableItemDBModel>(
                It.Is<string>(id => id == referenceId),
                It.Is<QueryConfig>(qc =>
                    qc.IndexName == "ReferenceID-Index" &&
                    qc.OverrideTableName == Config.SellableItemsTable)),
            Times.Once,
            $"Expected QueryAsync called once with referenceId='{referenceId}' and table='{Config.SellableItemsTable}'");

        SearchMock.Verify(
            x => x.GetRemainingAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetRemainingAsync called exactly once");

        ContextMock.VerifyNoOtherCalls();
        SearchMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return Result.Failure propagated from repository without alteration when entity is not found")]
    public async Task ShouldReturnFailureWhenRepositoryReturnsNoEntity()
    {
        // Given: DynamoDB context returns an empty list — no entity exists for the given referenceId
        string referenceId = "ref-id-notfound-adapter-002";

        Mock<IAsyncSearch<SellableItemDBModel>> SearchMock = new();
        SearchMock
            .Setup(x => x.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        ContextMock
            .Setup(x => x.QueryAsync<SellableItemDBModel>(referenceId, It.IsAny<QueryConfig>()))
            .Returns(SearchMock.Object);

        // When: GetSellableItemByReferenceAsync is called on the adapter
        Result<SellableItemEntity> result =
            await Adapter.GetSellableItemByReferenceAsync(referenceId, CancellationToken.None);

        // Then: result is Failure with the exact error code the repository produces — adapter does not modify it
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with SellableItemID: '{(result.IsSuccess ? result.Value.SellableItemID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.SellableItemNotFound),
            $"Expected error '{CatalogErrors.SellableItemNotFound}' (CAT-001) but got '{result.Error}'");

        ContextMock.Verify(
            x => x.QueryAsync<SellableItemDBModel>(
                It.Is<string>(id => id == referenceId),
                It.Is<QueryConfig>(qc => qc.IndexName == "ReferenceID-Index")),
            Times.Once,
            "Expected QueryAsync called exactly once before returning empty list");

        SearchMock.Verify(
            x => x.GetRemainingAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetRemainingAsync called exactly once");

        ContextMock.VerifyNoOtherCalls();
        SearchMock.VerifyNoOtherCalls();
    }
}

