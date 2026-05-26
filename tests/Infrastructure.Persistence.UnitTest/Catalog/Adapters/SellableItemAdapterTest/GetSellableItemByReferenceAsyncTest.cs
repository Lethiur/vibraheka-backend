using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Errors;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Catalog.Adapters.SellableItemAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetSellableItemByReferenceAsyncTest : GenericSellableItemAdapterTest
{
    [Test]
    [DisplayName("Should return Result.Success with prices populated when repository returns entity and prices")]
    public async Task ShouldReturnSuccessWhenRepositoryReturnsEntityWithPrices()
    {
        // Given: repository returns a valid entity and price repository returns prices for it
        string referenceId = "ref-id-success-adapter-001";
        SellableItemEntity entity = BuildDefaultSellableItemEntity(referenceId);
        SellableItemPriceEntity priceEntity = BuildDefaultSellableItemPriceEntity(entity.SellableItemID);

        RepositoryMock
            .Setup(x => x.GetByReferenceIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entity));

        PriceRepositoryMock
            .Setup(x => x.GetBySellableItemIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<SellableItemPriceEntity> { priceEntity }));

        // When: GetSellableItemByReferenceAsync is called on the adapter
        Result<SellableItemEntity> result =
            await Adapter.GetSellableItemByReferenceAsync(referenceId, CancellationToken.None);

        // Then: result is Success and the entity data matches — adapter propagates without alteration
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.SellableItemID, Is.EqualTo(entity.SellableItemID),
            $"Expected SellableItemID '{entity.SellableItemID}' but got '{result.Value.SellableItemID}'");
        Assert.That(result.Value.Name, Is.EqualTo(entity.Name),
            $"Expected Name '{entity.Name}' but got '{result.Value.Name}'");
        Assert.That(result.Value.Prices, Has.Count.EqualTo(1),
            $"Expected 1 price but got {result.Value.Prices.Count}");

        RepositoryMock.Verify(
            x => x.GetByReferenceIdAsync(
                It.Is<string>(id => id == referenceId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected GetByReferenceIdAsync called once with referenceId='{referenceId}'");

        PriceRepositoryMock.Verify(
            x => x.GetBySellableItemIdAsync(
                It.Is<string>(id => id == entity.SellableItemID),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected GetBySellableItemIdAsync called once with sellableItemId='{entity.SellableItemID}'");

        RepositoryMock.VerifyNoOtherCalls();
        PriceRepositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return Result.Failure propagated from repository when entity is not found")]
    public async Task ShouldReturnFailureWhenRepositoryReturnsNoEntity()
    {
        // Given: repository returns failure — no entity exists for the given referenceId
        string referenceId = "ref-id-notfound-adapter-002";

        RepositoryMock
            .Setup(x => x.GetByReferenceIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<SellableItemEntity>(CatalogErrors.SellableItemNotFound));

        // When: GetSellableItemByReferenceAsync is called on the adapter
        Result<SellableItemEntity> result =
            await Adapter.GetSellableItemByReferenceAsync(referenceId, CancellationToken.None);

        // Then: result is Failure with the exact error code — adapter does not modify it
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with SellableItemID: '{(result.IsSuccess ? result.Value.SellableItemID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.SellableItemNotFound),
            $"Expected error '{CatalogErrors.SellableItemNotFound}' (CAT-001) but got '{result.Error}'");

        RepositoryMock.Verify(
            x => x.GetByReferenceIdAsync(
                It.Is<string>(id => id == referenceId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByReferenceIdAsync called exactly once before returning not-found failure");

        PriceRepositoryMock.Verify(
            x => x.GetBySellableItemIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Expected GetBySellableItemIdAsync NOT called when item lookup fails");

        RepositoryMock.VerifyNoOtherCalls();
        PriceRepositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return Result.Failure propagated from price repository when prices cannot be fetched after entity is found")]
    public async Task ShouldReturnFailureWhenPriceRepositoryFailsAfterEntityFound()
    {
        // Given: repository returns a valid entity but price repository returns failure for its prices
        string referenceId = "ref-id-pricefail-adapter-003";
        SellableItemEntity entity = BuildDefaultSellableItemEntity(referenceId);

        RepositoryMock
            .Setup(x => x.GetByReferenceIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entity));

        PriceRepositoryMock
            .Setup(x => x.GetBySellableItemIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<SellableItemPriceEntity>>(CatalogErrors.SellableItemPriceNotFound));

        // When: GetSellableItemByReferenceAsync is called on the adapter
        Result<SellableItemEntity> result =
            await Adapter.GetSellableItemByReferenceAsync(referenceId, CancellationToken.None);

        // Then: result is Failure with the price error propagated — adapter does not silence infrastructure errors
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure when prices cannot be fetched, but got success with SellableItemID: '{(result.IsSuccess ? result.Value.SellableItemID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.SellableItemPriceNotFound),
            $"Expected error '{CatalogErrors.SellableItemPriceNotFound}' (CAT-002) propagated from price repository but got '{result.Error}'");

        RepositoryMock.Verify(
            x => x.GetByReferenceIdAsync(
                It.Is<string>(id => id == referenceId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected GetByReferenceIdAsync called once with referenceId='{referenceId}'");

        PriceRepositoryMock.Verify(
            x => x.GetBySellableItemIdAsync(
                It.Is<string>(id => id == entity.SellableItemID),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected GetBySellableItemIdAsync called once with sellableItemId='{entity.SellableItemID}' after entity found");

        RepositoryMock.VerifyNoOtherCalls();
        PriceRepositoryMock.VerifyNoOtherCalls();
    }
}
