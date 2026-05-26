using System.ComponentModel;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SellableItemPriceRepositoryTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetBySellableItemPriceIdAsyncTest : GenericSellableItemPriceRepositoryTest
{
    [Test]
    [DisplayName("Should return mapped SellableItemPriceEntity when DynamoDB returns a model for the price ID")]
    public async Task ShouldReturnMappedEntityWhenDynamoDbReturnsModel()
    {
        // Given: DynamoDB returns a valid SellableItemPriceDBModel for the requested priceId
        string priceId = "sip-repo-success-001";
        SellableItemPriceDBModel model = BuildDefaultSellableItemPriceDBModel();

        ContextMock
            .Setup(x => x.LoadAsync<SellableItemPriceDBModel>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        // When: GetBySellableItemPriceIdAsync is called
        Result<SellableItemPriceEntity> result =
            await Repository.GetBySellableItemPriceIdAsync(priceId, CancellationToken.None);

        // Then: result should be success with a correctly mapped entity
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.SellableItemPriceID, Is.EqualTo(model.SellableItemPriceID),
            $"Expected SellableItemPriceID '{model.SellableItemPriceID}' but got '{result.Value.SellableItemPriceID}'");

        ContextMock.Verify(
            x => x.LoadAsync<SellableItemPriceDBModel>(
                It.Is<string>(id => id == priceId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected LoadAsync called once with priceId='{priceId}'");

        ContextMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return GPE-000 failure when DynamoDB returns null (price not found by ID)")]
    public async Task ShouldReturnGPE000FailureWhenDynamoDbReturnsNull()
    {
        // Given: DynamoDB returns null — no price entity exists for the given ID
        string priceId = "sip-repo-notfound-002";

        ContextMock
            .Setup(x => x.LoadAsync<SellableItemPriceDBModel>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SellableItemPriceDBModel)null!);

        // When: GetBySellableItemPriceIdAsync is called with a non-existent price ID
        Result<SellableItemPriceEntity> result =
            await Repository.GetBySellableItemPriceIdAsync(priceId, CancellationToken.None);

        // Then: result should be failure with GPE-000 (no MapError applied — raw persistence error propagates)
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with ID: '{(result.IsSuccess ? result.Value.SellableItemPriceID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.NoRecordsFound),
            $"Expected error '{GenericPersistenceErrors.NoRecordsFound}' (GPE-000) since no MapError is applied, but got '{result.Error}'");

        ContextMock.Verify(
            x => x.LoadAsync<SellableItemPriceDBModel>(
                It.Is<string>(id => id == priceId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected LoadAsync to be called exactly once before returning null");

        ContextMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return GPE-999 failure when DynamoDB throws an unexpected exception")]
    public async Task ShouldReturnGPE999FailureWhenDynamoDbThrowsException()
    {
        // Given: DynamoDB throws an unexpected exception for the price ID lookup
        string priceId = "sip-repo-exception-003";

        ContextMock
            .Setup(x => x.LoadAsync<SellableItemPriceDBModel>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected DynamoDB connection error"));

        // When: GetBySellableItemPriceIdAsync is called
        Result<SellableItemPriceEntity> result =
            await Repository.GetBySellableItemPriceIdAsync(priceId, CancellationToken.None);

        // Then: result should be failure with GPE-999 (general error, no MapError applied)
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when DynamoDB throws an unexpected exception");
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError),
            $"Expected '{GenericPersistenceErrors.GeneralError}' (GPE-999) for unexpected exception, but got '{result.Error}'");
        Assert.That(result.Error, Is.Not.EqualTo(GenericPersistenceErrors.NoRecordsFound),
            "Unexpected exceptions must NOT return GPE-000 (not-found)");

        ContextMock.Verify(
            x => x.LoadAsync<SellableItemPriceDBModel>(
                It.Is<string>(id => id == priceId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected LoadAsync to be called once before throwing");

        ContextMock.VerifyNoOtherCalls();
    }
}

