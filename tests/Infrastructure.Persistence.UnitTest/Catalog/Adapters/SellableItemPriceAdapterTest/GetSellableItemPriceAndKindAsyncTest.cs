using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Errors;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Catalog.Adapters.SellableItemPriceAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetSellableItemPriceAndKindAsyncTest : GenericSellableItemPriceAdapterTest
{
    [Test]
    [DisplayName("Should return Result.Success propagated from repository without alteration when a matching price is found")]
    public async Task ShouldReturnSuccessWhenRepositoryReturnsMatchingPrice()
    {
        // Given: DynamoDB context returns a price model matching the requested sellableItemId and kind
        string sellableItemId = "sellable-item-adapter-success-001";
        PriceKind kind = PriceKind.OneTime;
        SellableItemPriceDBModel model = BuildDefaultSellableItemPriceDBModel(sellableItemId, kind);

        Mock<IAsyncSearch<SellableItemPriceDBModel>> SearchMock = new();
        SearchMock
            .Setup(x => x.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([model]);

        ContextMock
            .Setup(x => x.QueryAsync<SellableItemPriceDBModel>(sellableItemId, It.IsAny<QueryConfig>()))
            .Returns(SearchMock.Object);

        // When: GetSellableItemPriceAndKindAsync is called on the adapter
        Result<SellableItemPriceEntity> result =
            await Adapter.GetSellableItemPriceAndKindAsync(sellableItemId, kind, CancellationToken.None);

        // Then: result is Success and the entity data matches the model — no alteration by the adapter
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.SellableItemPriceID, Is.EqualTo(model.SellableItemPriceID),
            $"Expected SellableItemPriceID '{model.SellableItemPriceID}' but got '{result.Value.SellableItemPriceID}'");
        Assert.That(result.Value.Kind, Is.EqualTo(kind),
            $"Expected Kind '{kind}' but got '{result.Value.Kind}'");

        ContextMock.Verify(
            x => x.QueryAsync<SellableItemPriceDBModel>(
                It.Is<string>(id => id == sellableItemId),
                It.Is<QueryConfig>(qc =>
                    qc.IndexName == "SellableItemID-Index" &&
                    qc.OverrideTableName == Config.SellableItemPricesTable)),
            Times.Once,
            $"Expected QueryAsync called once with sellableItemId='{sellableItemId}' and table='{Config.SellableItemPricesTable}'");

        SearchMock.Verify(
            x => x.GetRemainingAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetRemainingAsync called exactly once");

        ContextMock.VerifyNoOtherCalls();
        SearchMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return Result.Failure propagated from repository without alteration when no price is found")]
    public async Task ShouldReturnFailureWhenRepositoryReturnsNoMatchingPrice()
    {
        // Given: DynamoDB context returns an empty list — no prices exist for the given sellable item
        string sellableItemId = "sellable-item-adapter-notfound-002";

        Mock<IAsyncSearch<SellableItemPriceDBModel>> SearchMock = new();
        SearchMock
            .Setup(x => x.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        ContextMock
            .Setup(x => x.QueryAsync<SellableItemPriceDBModel>(sellableItemId, It.IsAny<QueryConfig>()))
            .Returns(SearchMock.Object);

        // When: GetSellableItemPriceAndKindAsync is called on the adapter
        Result<SellableItemPriceEntity> result =
            await Adapter.GetSellableItemPriceAndKindAsync(sellableItemId, PriceKind.OneTime, CancellationToken.None);

        // Then: result is Failure with the exact error code the repository produces — adapter does not modify it
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with ID: '{(result.IsSuccess ? result.Value.SellableItemPriceID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.SellableItemPriceNotFound),
            $"Expected error '{CatalogErrors.SellableItemPriceNotFound}' (CAT-002) but got '{result.Error}'");

        ContextMock.Verify(
            x => x.QueryAsync<SellableItemPriceDBModel>(
                It.Is<string>(id => id == sellableItemId),
                It.Is<QueryConfig>(qc => qc.IndexName == "SellableItemID-Index")),
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

