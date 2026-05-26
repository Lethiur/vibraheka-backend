using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Errors;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SellableItemPriceRepositoryTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetBySellableItemIdAsyncTest : GenericSellableItemPriceRepositoryTest
{
    [Test]
    [DisplayName("Should return list of mapped SellableItemPriceEntity when DynamoDB returns models for sellableItemId")]
    public async Task ShouldReturnMappedEntitiesWhenDynamoDbReturnsModels()
    {
        // Given: DynamoDB (via FindAllByIndexAsync) returns one model for the sellableItemId
        string sellableItemId = "sellable-item-price-repo-001";
        SellableItemPriceDBModel model = BuildDefaultSellableItemPriceDBModel(sellableItemId);

        Mock<IAsyncSearch<SellableItemPriceDBModel>> SearchMock = new();
        SearchMock
            .Setup(x => x.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([model]);

        ContextMock
            .Setup(x => x.QueryAsync<SellableItemPriceDBModel>(
                sellableItemId,
                It.IsAny<QueryConfig>()))
            .Returns(SearchMock.Object);

        // When: GetBySellableItemIdAsync is called
        Result<List<SellableItemPriceEntity>> result =
            await Repository.GetBySellableItemIdAsync(sellableItemId, CancellationToken.None);

        // Then: result should be success with a list containing the correctly mapped entity
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Has.Count.EqualTo(1),
            $"Expected 1 price entity but got {result.Value.Count}");
        Assert.That(result.Value[0].SellableItemPriceID, Is.EqualTo(model.SellableItemPriceID),
            $"Expected SellableItemPriceID '{model.SellableItemPriceID}' but got '{result.Value[0].SellableItemPriceID}'");

        ContextMock.Verify(
            x => x.QueryAsync<SellableItemPriceDBModel>(
                It.Is<string>(id => id == sellableItemId),
                It.Is<QueryConfig>(qc => qc.IndexName == "SellableItemID-Index")),
            Times.Once,
            $"Expected QueryAsync called once for sellableItemId='{sellableItemId}' on index='SellableItemID-Index'");

        SearchMock.Verify(
            x => x.GetRemainingAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetRemainingAsync called exactly once");

        ContextMock.VerifyNoOtherCalls();
        SearchMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return CAT-002 failure when DynamoDB returns empty list (no prices for sellableItemId)")]
    public async Task ShouldReturnCAT002FailureWhenDynamoDbReturnsEmptyList()
    {
        // Given: DynamoDB returns an empty list — no prices exist for the given sellableItemId
        string sellableItemId = "sellable-item-price-repo-002";

        Mock<IAsyncSearch<SellableItemPriceDBModel>> SearchMock = new();
        SearchMock
            .Setup(x => x.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        ContextMock
            .Setup(x => x.QueryAsync<SellableItemPriceDBModel>(
                sellableItemId,
                It.IsAny<QueryConfig>()))
            .Returns(SearchMock.Object);

        // When: GetBySellableItemIdAsync is called for a sellableItemId with no prices
        Result<List<SellableItemPriceEntity>> result =
            await Repository.GetBySellableItemIdAsync(sellableItemId, CancellationToken.None);

        // Then: result should be failure with CAT-002 (mapped from GPE-000 NoRecordsFound)
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure for empty result but got success with {(result.IsSuccess ? result.Value.Count : 0)} items");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.SellableItemPriceNotFound),
            $"Expected error '{CatalogErrors.SellableItemPriceNotFound}' (CAT-002) mapped from GPE-000, but got '{result.Error}'");

        ContextMock.Verify(
            x => x.QueryAsync<SellableItemPriceDBModel>(
                It.Is<string>(id => id == sellableItemId),
                It.Is<QueryConfig>(qc => qc.IndexName == "SellableItemID-Index")),
            Times.Once,
            "Expected QueryAsync called once before returning empty list");

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
        string sellableItemId = "sellable-item-price-repo-003";

        ContextMock
            .Setup(x => x.QueryAsync<SellableItemPriceDBModel>(
                sellableItemId,
                It.IsAny<QueryConfig>()))
            .Throws(new Exception("Unexpected DynamoDB connection error"));

        // When: GetBySellableItemIdAsync is called
        Result<List<SellableItemPriceEntity>> result =
            await Repository.GetBySellableItemIdAsync(sellableItemId, CancellationToken.None);

        // Then: result should be failure with CAT-004 (GPE-999 mapped by MapError — not NoRecordsFound)
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when DynamoDB throws an unexpected exception");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.FailedToQuerySellableItemPrice),
            $"Expected '{CatalogErrors.FailedToQuerySellableItemPrice}' (CAT-004) for general exception, but got '{result.Error}'");
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

