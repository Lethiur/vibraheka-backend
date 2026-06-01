using System.ComponentModel;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Repositories.SellableItemRepositoryTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class FindByIDAsyncTest : GenericSellableItemRepositoryTest
{
    [Test]
    [DisplayName("Should return mapped SellableItemEntity when DynamoDB returns a model for the sellableItemId")]
    public async Task ShouldReturnMappedEntityWhenDynamoDbReturnsModel()
    {
        // Given: DynamoDB returns a valid SellableItemDBModel for the requested sellableItemId
        string sellableItemId = "sellable-item-findbyid-success-001";
        SellableItemDBModel model = BuildDefaultSellableItemDBModel();

        ContextMock
            .Setup(x => x.LoadAsync<SellableItemDBModel>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        // When: FindByIDAsync is called
        Result<SellableItemEntity> result =
            await Repository.FindByIDAsync(sellableItemId, CancellationToken.None);

        // Then: result should be success with a correctly mapped entity
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.SellableItemID, Is.EqualTo(model.SellableItemID),
            $"Expected SellableItemID '{model.SellableItemID}' but got '{result.Value.SellableItemID}'");
        Assert.That(result.Value.Name, Is.EqualTo(model.Name),
            $"Expected Name '{model.Name}' but got '{result.Value.Name}'");

        ContextMock.Verify(
            x => x.LoadAsync<SellableItemDBModel>(
                It.Is<string>(id => id == sellableItemId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected LoadAsync called once with sellableItemId='{sellableItemId}'");

        ContextMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return GPE-000 failure when DynamoDB returns null (entity not found by ID)")]
    public async Task ShouldReturnGPE000FailureWhenDynamoDbReturnsNull()
    {
        // Given: DynamoDB returns null — no entity exists for the given sellableItemId
        string sellableItemId = "sellable-item-findbyid-notfound-002";

        ContextMock
            .Setup(x => x.LoadAsync<SellableItemDBModel>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SellableItemDBModel)null!);

        // When: FindByIDAsync is called with a non-existent ID
        Result<SellableItemEntity> result =
            await Repository.FindByIDAsync(sellableItemId, CancellationToken.None);

        // Then: result should be failure with GPE-000 (no MapError applied — raw persistence error propagates)
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with SellableItemID: '{(result.IsSuccess ? result.Value.SellableItemID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.NoRecordsFound),
            $"Expected error '{GenericPersistenceErrors.NoRecordsFound}' (GPE-000) since no MapError is applied, but got '{result.Error}'");

        ContextMock.Verify(
            x => x.LoadAsync<SellableItemDBModel>(
                It.Is<string>(id => id == sellableItemId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected LoadAsync to be called exactly once before returning null");

        ContextMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return GPE-999 failure when DynamoDB throws an unexpected exception")]
    public async Task ShouldReturnGPE999FailureWhenDynamoDbThrowsException()
    {
        // Given: DynamoDB throws an unexpected exception for the sellableItemId lookup
        string sellableItemId = "sellable-item-findbyid-exception-003";

        ContextMock
            .Setup(x => x.LoadAsync<SellableItemDBModel>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected DynamoDB connection error"));

        // When: FindByIDAsync is called
        Result<SellableItemEntity> result =
            await Repository.FindByIDAsync(sellableItemId, CancellationToken.None);

        // Then: result should be failure with GPE-999 (general error, no MapError applied)
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when DynamoDB throws an unexpected exception");
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError),
            $"Expected '{GenericPersistenceErrors.GeneralError}' (GPE-999) for unexpected exception, but got '{result.Error}'");
        Assert.That(result.Error, Is.Not.EqualTo(GenericPersistenceErrors.NoRecordsFound),
            "Unexpected exceptions must NOT return GPE-000 (not-found)");

        ContextMock.Verify(
            x => x.LoadAsync<SellableItemDBModel>(
                It.Is<string>(id => id == sellableItemId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected LoadAsync to be called once before throwing");

        ContextMock.VerifyNoOtherCalls();
    }
}

