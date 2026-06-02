using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Errors;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Repositories.SellableItemRepositoryTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetByReferenceIdAsyncTest : GenericSellableItemRepositoryTest
{
    [Test]
    [DisplayName("Should return mapped SellableItemEntity when DynamoDB returns a model for the reference ID")]
    public async Task ShouldReturnMappedEntityWhenDynamoDbReturnsModel()
    {
        // Given: DynamoDB returns a valid SellableItemDBModel for the requested reference ID
        string referenceId = Guid.NewGuid().ToString();
        SellableItemDBModel model = BuildDefaultSellableItemDBModel(referenceId);

        Mock<IAsyncSearch<SellableItemDBModel>> SearchMock = new();
        SearchMock
            .Setup(x => x.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([model]);

        ContextMock
            .Setup(x => x.QueryAsync<SellableItemDBModel>(
                referenceId,
                It.IsAny<QueryConfig>()))
            .Returns(SearchMock.Object);

        // When: GetByReferenceIdAsync is called
        Result<SellableItemEntity> result = await Repository.GetByReferenceIdAsync(referenceId, CancellationToken.None);

        // Then: result should be success with a correctly mapped entity
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.SellableItemID, Is.EqualTo(model.SellableItemID),
            $"Expected SellableItemID '{model.SellableItemID}' but got '{result.Value.SellableItemID}'");
        Assert.That(result.Value.Name, Is.EqualTo(model.Name),
            $"Expected Name '{model.Name}' but got '{result.Value.Name}'");
        Assert.That(result.Value.IsActive, Is.EqualTo(model.IsActive),
            $"Expected IsActive '{model.IsActive}' but got '{result.Value.IsActive}'");

        ContextMock.Verify(
            x => x.QueryAsync<SellableItemDBModel>(
                It.Is<string>(id => id == referenceId),
                It.Is<QueryConfig>(qc =>
                    qc.IndexName == "ReferenceID-Index")),
            Times.Once,
            $"Expected QueryAsync called once with indexName='ReferenceId-Index'");

        SearchMock.Verify(
            x => x.GetRemainingAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetRemainingAsync called exactly once");

        ContextMock.VerifyNoOtherCalls();
        SearchMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return CAT-001 failure when DynamoDB returns an empty list (not found)")]
    public async Task ShouldReturnCAT001FailureWhenDynamoDbReturnsEmptyList()
    {
        // Given: DynamoDB returns an empty list — no SellableItem with that reference ID
        string referenceId = Guid.NewGuid().ToString();

        Mock<IAsyncSearch<SellableItemDBModel>> SearchMock = new();
        SearchMock
            .Setup(x => x.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        ContextMock
            .Setup(x => x.QueryAsync<SellableItemDBModel>(
                referenceId,
                It.IsAny<QueryConfig>()))
            .Returns(SearchMock.Object);

        // When: GetByReferenceIdAsync is called with a referenceId that has no results
        Result<SellableItemEntity> result = await Repository.GetByReferenceIdAsync(referenceId, CancellationToken.None);

        // Then: result should be failure with CAT-001 (mapped from GPE-000 NoRecordsFound)
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with SellableItemID: '{(result.IsSuccess ? result.Value.SellableItemID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.SellableItemNotFound),
            $"Expected error '{CatalogErrors.SellableItemNotFound}' (CAT-001) mapped from GPE-000, but got '{result.Error}'");

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

    [Test]
    [DisplayName("Should return CAT-003 failure when DynamoDB throws an unexpected exception")]
    public async Task ShouldReturnCAT003FailureWhenDynamoDbThrowsException()
    {
        // Given: DynamoDB throws an unexpected exception during the query
        string referenceId = Guid.NewGuid().ToString();

        ContextMock
            .Setup(x => x.QueryAsync<SellableItemDBModel>(
                referenceId,
                It.IsAny<QueryConfig>()))
            .Throws(new Exception("Unexpected DynamoDB connection error"));

        // When: GetByReferenceIdAsync is called
        Result<SellableItemEntity> result = await Repository.GetByReferenceIdAsync(referenceId, CancellationToken.None);

        // Then: result should be failure with CAT-003 (FailedToQuerySellableItem — non-NotFound errors map here)
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when DynamoDB throws an unexpected exception");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.FailedToQuerySellableItem),
            $"Expected error '{CatalogErrors.FailedToQuerySellableItem}' (CAT-003) for general DynamoDB exception, but got '{result.Error}'");
        Assert.That(result.Error, Is.Not.EqualTo(CatalogErrors.SellableItemNotFound),
            "General exceptions must NOT be mapped to CAT-001; they must map to CAT-003");

        ContextMock.Verify(
            x => x.QueryAsync<SellableItemDBModel>(
                It.Is<string>(id => id == referenceId),
                It.Is<QueryConfig>(qc => qc.IndexName == "ReferenceID-Index")),
            Times.Once,
            "Expected QueryAsync called once before throwing the exception");

        ContextMock.VerifyNoOtherCalls();
    }
}

