using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Errors;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SellableItemPriceRepositoryTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetBySellableItemIdAndKindAsyncTest : GenericSellableItemPriceRepositoryTest
{
    [Test]
    [DisplayName("Should return mapped SellableItemPriceEntity when DynamoDB returns a matching model")]
    public async Task ShouldReturnMappedEntityWhenDynamoDbReturnsMatchingModel()
    {
        // Given: DynamoDB returns one SellableItemPriceDBModel matching the requested sellableItemId and kind
        string sellableItemId = "sellable-item-001";
        PriceKind kind = PriceKind.OneTime;
        SellableItemPriceDBModel model = BuildDefaultSellableItemPriceDBModel(sellableItemId, kind);

        Mock<IAsyncSearch<SellableItemPriceDBModel>> SearchMock = new();
        SearchMock
            .Setup(x => x.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([model]);

        ContextMock
            .Setup(x => x.QueryAsync<SellableItemPriceDBModel>(
                sellableItemId,
                It.IsAny<QueryConfig>()))
            .Returns(SearchMock.Object);

        // When: GetBySellableItemIdAndKindAsync is called
        Result<SellableItemPriceEntity> result =
            await Repository.GetBySellableItemIdAndKindAsync(sellableItemId, kind, CancellationToken.None);

        // Then: result should be success with a correctly mapped entity
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
                    qc.IndexName == "SellableItemID-Index")),
            Times.Once,
            $"Expected QueryAsync called once with indexName='SellableItemID-Index'");

        SearchMock.Verify(
            x => x.GetRemainingAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetRemainingAsync called exactly once");

        ContextMock.VerifyNoOtherCalls();
        SearchMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return CAT-002 failure when DynamoDB returns no prices for the sellable item")]
    public async Task ShouldReturnCAT002FailureWhenDynamoDbReturnsEmptyList()
    {
        // Given: DynamoDB returns an empty list — no prices for the sellable item
        string sellableItemId = "sellable-item-002";

        Mock<IAsyncSearch<SellableItemPriceDBModel>> SearchMock = new();
        SearchMock
            .Setup(x => x.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        ContextMock
            .Setup(x => x.QueryAsync<SellableItemPriceDBModel>(
                sellableItemId,
                It.IsAny<QueryConfig>()))
            .Returns(SearchMock.Object);

        // When: GetBySellableItemIdAndKindAsync is called for a non-existent sellable item
        Result<SellableItemPriceEntity> result =
            await Repository.GetBySellableItemIdAndKindAsync(sellableItemId, PriceKind.OneTime, CancellationToken.None);

        // Then: result should be failure with CAT-002 (mapped from GPE-000)
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure for empty list but got success with ID: '{(result.IsSuccess ? result.Value.SellableItemPriceID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.SellableItemPriceNotFound),
            $"Expected error '{CatalogErrors.SellableItemPriceNotFound}' (CAT-002) but got '{result.Error}'");

        ContextMock.Verify(
            x => x.QueryAsync<SellableItemPriceDBModel>(
                It.Is<string>(id => id == sellableItemId),
                It.Is<QueryConfig>(qc => qc.IndexName == "SellableItemID-Index")),
            Times.Once,
            "Expected QueryAsync called exactly once before returning empty results");

        SearchMock.Verify(
            x => x.GetRemainingAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetRemainingAsync called exactly once");

        ContextMock.VerifyNoOtherCalls();
        SearchMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return CAT-002 failure when prices exist but none match the requested PriceKind")]
    public async Task ShouldReturnCAT002FailureWhenNoPriceMatchesRequestedKind()
    {
        // Given: DynamoDB returns a Recurring price but the caller requests OneTime
        string sellableItemId = "sellable-item-003";
        SellableItemPriceDBModel recurringModel =
            BuildDefaultSellableItemPriceDBModel(sellableItemId, PriceKind.Recurring);

        Mock<IAsyncSearch<SellableItemPriceDBModel>> SearchMock = new();
        SearchMock
            .Setup(x => x.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([recurringModel]);

        ContextMock
            .Setup(x => x.QueryAsync<SellableItemPriceDBModel>(
                sellableItemId,
                It.IsAny<QueryConfig>()))
            .Returns(SearchMock.Object);

        // When: GetBySellableItemIdAndKindAsync is called requesting OneTime kind
        Result<SellableItemPriceEntity> result =
            await Repository.GetBySellableItemIdAndKindAsync(sellableItemId, PriceKind.OneTime, CancellationToken.None);

        // Then: result should be failure with CAT-002 (no matching kind found in returned list)
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure when kind does not match but got success with ID: '{(result.IsSuccess ? result.Value.SellableItemPriceID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.SellableItemPriceNotFound),
            $"Expected error '{CatalogErrors.SellableItemPriceNotFound}' (CAT-002) when no price matches the kind, but got '{result.Error}'");

        ContextMock.Verify(
            x => x.QueryAsync<SellableItemPriceDBModel>(
                It.Is<string>(id => id == sellableItemId),
                It.Is<QueryConfig>(qc => qc.IndexName == "SellableItemID-Index")),
            Times.Once,
            "Expected QueryAsync called once even when no matching kind is found");

        SearchMock.Verify(
            x => x.GetRemainingAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetRemainingAsync called exactly once");

        ContextMock.VerifyNoOtherCalls();
        SearchMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return CAT-004 failure when DynamoDB throws an unexpected exception")]
    public async Task ShouldReturnCAT004FailureWhenDynamoDbThrowsException()
    {
        // Given: DynamoDB throws an unexpected exception during the query
        string sellableItemId = "sellable-item-004";

        ContextMock
            .Setup(x => x.QueryAsync<SellableItemPriceDBModel>(
                sellableItemId,
                It.IsAny<QueryConfig>()))
            .Throws(new Exception("Unexpected DynamoDB connection error"));

        // When: GetBySellableItemIdAndKindAsync is called
        Result<SellableItemPriceEntity> result =
            await Repository.GetBySellableItemIdAndKindAsync(sellableItemId, PriceKind.OneTime, CancellationToken.None);

        // Then: result should be failure with CAT-004 (FailedToQuerySellableItemPrice)
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when DynamoDB throws an unexpected exception");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.FailedToQuerySellableItemPrice),
            $"Expected error '{CatalogErrors.FailedToQuerySellableItemPrice}' (CAT-004) for a general exception, but got '{result.Error}'");
        Assert.That(result.Error, Is.Not.EqualTo(CatalogErrors.SellableItemPriceNotFound),
            "General exceptions must NOT be mapped to CAT-002; they must map to CAT-004");

        ContextMock.Verify(
            x => x.QueryAsync<SellableItemPriceDBModel>(
                It.Is<string>(id => id == sellableItemId),
                It.Is<QueryConfig>(qc => qc.IndexName == "SellableItemID-Index")),
            Times.Once,
            "Expected QueryAsync called once before throwing the exception");

        ContextMock.VerifyNoOtherCalls();
    }
}
