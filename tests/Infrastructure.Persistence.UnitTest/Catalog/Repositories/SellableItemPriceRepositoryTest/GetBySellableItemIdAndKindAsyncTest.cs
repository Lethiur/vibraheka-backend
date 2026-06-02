using System.ComponentModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Repositories.SellableItemPriceRepositoryTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetBySellableItemIdAndKindAsyncTest : GenericSellableItemPriceRepositoryTest
{
    // ──────────────────────────────────────────────────────────────────────────
    // NOTE: GetBySellableItemIdAndKindAsync uses QueryIndexAsync internally,
    // which calls IAmazonDynamoDB.QueryAsync (raw AWS client) + GetTargetTable +
    // FromDocument — NOT IDynamoDBContext.QueryAsync<T>.
    // Index used: "SellableItemID-Kind-Index" (compound GSI on SellableItemID + Kind).
    // Kind filtering is server-side; the application layer only checks items.Count.
    // ──────────────────────────────────────────────────────────────────────────

    [Test]
    [DisplayName("Should return mapped SellableItemPriceEntity when DynamoDB returns a matching model")]
    public async Task ShouldReturnMappedEntityWhenDynamoDbReturnsMatchingModel()
    {
        // Given: DynamoDB (via raw client) returns one item for sellableItemId + OneTime compound key
        string sellableItemId = "sellable-item-001";
        PriceKind kind = PriceKind.OneTime;
        SellableItemPriceDBModel model = BuildDefaultSellableItemPriceDBModel(sellableItemId, kind);

        ContextMock
            .Setup(x => x.GetTargetTable<SellableItemPriceDBModel>())
            .Returns(BuildFakeTable());

        DynamoDbClientMock
            .Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = [new Dictionary<string, AttributeValue>()] });

        ContextMock
            .Setup(x => x.FromDocument<SellableItemPriceDBModel>(It.IsAny<Document>()))
            .Returns(model);

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
            x => x.GetTargetTable<SellableItemPriceDBModel>(),
            Times.Once,
            "Expected GetTargetTable called once to resolve the DynamoDB table name");

        DynamoDbClientMock.Verify(
            x => x.QueryAsync(
                It.Is<QueryRequest>(r =>
                    r.IndexName == "SellableItemID-Kind-Index"
                    && r.KeyConditionExpression == "#sid = :sid AND #kind = :kind"),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected QueryAsync called once with IndexName='SellableItemID-Kind-Index'");

        ContextMock.Verify(
            x => x.FromDocument<SellableItemPriceDBModel>(It.IsAny<Document>()),
            Times.Once,
            "Expected FromDocument called once to deserialise the raw DynamoDB item");


        ContextMock.VerifyNoOtherCalls();
        DynamoDbClientMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return CAT-002 failure when DynamoDB returns no items for the compound key")]
    public async Task ShouldReturnCAT002FailureWhenDynamoDbReturnsEmptyList()
    {
        // Given: DynamoDB returns an empty list — no price exists for sellableItemId + OneTime
        string sellableItemId = "sellable-item-002";

        ContextMock
            .Setup(x => x.GetTargetTable<SellableItemPriceDBModel>())
            .Returns(BuildFakeTable());

        DynamoDbClientMock
            .Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = [] });

        // When: GetBySellableItemIdAndKindAsync is called for a non-existent compound key
        Result<SellableItemPriceEntity> result =
            await Repository.GetBySellableItemIdAndKindAsync(sellableItemId, PriceKind.OneTime, CancellationToken.None);

        // Then: result should be failure with CAT-002
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure for empty result but got success with ID: '{(result.IsSuccess ? result.Value.SellableItemPriceID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.SellableItemPriceNotFound),
            $"Expected error '{CatalogErrors.SellableItemPriceNotFound}' (CAT-002) but got '{result.Error}'");

        ContextMock.Verify(
            x => x.GetTargetTable<SellableItemPriceDBModel>(),
            Times.Once,
            "Expected GetTargetTable called once before querying");

        DynamoDbClientMock.Verify(
            x => x.QueryAsync(
                It.Is<QueryRequest>(r => r.IndexName == "SellableItemID-Kind-Index"),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected QueryAsync called exactly once with IndexName='SellableItemID-Kind-Index'");


        ContextMock.VerifyNoOtherCalls();
        DynamoDbClientMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return CAT-002 failure when DynamoDB compound index returns no items for the requested PriceKind")]
    public async Task ShouldReturnCAT002FailureWhenCompoundIndexReturnsNoItemsForRequestedKind()
    {
        // Given: DynamoDB (via SellableItemID-Kind-Index) returns empty — Kind filtering is server-side;
        //        no item exists for sellableItemId + Recurring compound key
        string sellableItemId = "sellable-item-003";

        ContextMock
            .Setup(x => x.GetTargetTable<SellableItemPriceDBModel>())
            .Returns(BuildFakeTable());

        DynamoDbClientMock
            .Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = [] });

        // When: GetBySellableItemIdAndKindAsync is called requesting Recurring kind (not present)
        Result<SellableItemPriceEntity> result =
            await Repository.GetBySellableItemIdAndKindAsync(sellableItemId, PriceKind.Recurring, CancellationToken.None);

        // Then: result should be failure with CAT-002 — compound GSI returns empty when Kind does not match
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure when compound index returns no items for the requested kind, but got success with ID: '{(result.IsSuccess ? result.Value.SellableItemPriceID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.SellableItemPriceNotFound),
            $"Expected '{CatalogErrors.SellableItemPriceNotFound}' (CAT-002) when compound GSI returns empty, but got '{result.Error}'");

        ContextMock.Verify(
            x => x.GetTargetTable<SellableItemPriceDBModel>(),
            Times.Once,
            "Expected GetTargetTable called once");

        DynamoDbClientMock.Verify(
            x => x.QueryAsync(
                It.Is<QueryRequest>(r => r.IndexName == "SellableItemID-Kind-Index"),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected QueryAsync called once with IndexName='SellableItemID-Kind-Index'");


        ContextMock.VerifyNoOtherCalls();
        DynamoDbClientMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return GPE-999 failure when DynamoDB throws an unexpected exception")]
    public async Task ShouldReturnGPE999FailureWhenDynamoDbThrowsException()
    {
        // Given: DynamoDB raw client throws an unexpected exception during the query
        string sellableItemId = "sellable-item-004";

        ContextMock
            .Setup(x => x.GetTargetTable<SellableItemPriceDBModel>())
            .Returns(BuildFakeTable());

        DynamoDbClientMock
            .Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected DynamoDB connection error"));

        // When: GetBySellableItemIdAndKindAsync is called
        Result<SellableItemPriceEntity> result =
            await Repository.GetBySellableItemIdAndKindAsync(sellableItemId, PriceKind.OneTime, CancellationToken.None);

        // Then: result should be failure with GPE-999 (GeneralError)
        //       GetBySellableItemIdAndKindAsync uses QueryIndexAsync which propagates raw persistence
        //       errors without MapError; domain error mapping is not applied here.
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when DynamoDB throws an unexpected exception");
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError),
            $"Expected '{GenericPersistenceErrors.GeneralError}' (GPE-999) since QueryIndexAsync maps generic exceptions to GeneralError and GetBySellableItemIdAndKindAsync does not apply MapError, but got '{result.Error}'");
        Assert.That(result.Error, Is.Not.EqualTo(CatalogErrors.SellableItemPriceNotFound),
            "General exceptions must NOT be mapped to CAT-002 (not-found)");

        ContextMock.Verify(
            x => x.GetTargetTable<SellableItemPriceDBModel>(),
            Times.Once,
            "Expected GetTargetTable called once before the exception was thrown");

        DynamoDbClientMock.Verify(
            x => x.QueryAsync(
                It.Is<QueryRequest>(r => r.IndexName == "SellableItemID-Kind-Index"),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected QueryAsync called once before throwing the exception");


        ContextMock.VerifyNoOtherCalls();
        DynamoDbClientMock.VerifyNoOtherCalls();
    }
}
