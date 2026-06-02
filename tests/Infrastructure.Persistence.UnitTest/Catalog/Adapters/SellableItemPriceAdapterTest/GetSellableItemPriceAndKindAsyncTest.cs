using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Errors;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Adapters.SellableItemPriceAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetSellableItemPriceAndKindAsyncTest : GenericSellableItemPriceAdapterTest
{
    [Test]
    [DisplayName("Should return Result.Success propagated from repository without alteration when a matching price is found")]
    public async Task ShouldReturnSuccessWhenRepositoryReturnsMatchingPrice()
    {
        // Given: repository returns a valid price entity for sellableItemId + OneTime
        string sellableItemId = "sellable-item-adapter-success-001";
        PriceKind kind = PriceKind.OneTime;
        SellableItemPriceEntity entity = BuildDefaultSellableItemPriceEntity(sellableItemId, kind);

        RepositoryMock
            .Setup(x => x.GetBySellableItemIdAndKindAsync(It.IsAny<string>(), It.IsAny<PriceKind>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entity));

        // When: GetSellableItemPriceAndKindAsync is called on the adapter
        Result<SellableItemPriceEntity> result =
            await Adapter.GetSellableItemPriceAndKindAsync(sellableItemId, kind, CancellationToken.None);

        // Then: result is Success with entity data matching the model — adapter propagates without alteration
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.SellableItemPriceID, Is.EqualTo(entity.SellableItemPriceID),
            $"Expected SellableItemPriceID '{entity.SellableItemPriceID}' but got '{result.Value.SellableItemPriceID}'");
        Assert.That(result.Value.Kind, Is.EqualTo(kind),
            $"Expected Kind '{kind}' but got '{result.Value.Kind}'");

        RepositoryMock.Verify(
            x => x.GetBySellableItemIdAndKindAsync(
                It.Is<string>(id => id == sellableItemId),
                It.Is<PriceKind>(k => k == kind),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected GetBySellableItemIdAndKindAsync called once with sellableItemId='{sellableItemId}' and kind='{kind}'");

        RepositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return Result.Failure propagated from repository without alteration when no price is found")]
    public async Task ShouldReturnFailureWhenRepositoryReturnsNoMatchingPrice()
    {
        // Given: repository returns failure — no price exists for the given sellableItemId + OneTime
        string sellableItemId = "sellable-item-adapter-notfound-002";

        RepositoryMock
            .Setup(x => x.GetBySellableItemIdAndKindAsync(It.IsAny<string>(), It.IsAny<PriceKind>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<SellableItemPriceEntity>(CatalogErrors.SellableItemPriceNotFound));

        // When: GetSellableItemPriceAndKindAsync is called on the adapter
        Result<SellableItemPriceEntity> result =
            await Adapter.GetSellableItemPriceAndKindAsync(sellableItemId, PriceKind.OneTime, CancellationToken.None);

        // Then: result is Failure with CAT-002 propagated from repository — adapter does not modify it
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with ID: '{(result.IsSuccess ? result.Value.SellableItemPriceID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.SellableItemPriceNotFound),
            $"Expected error '{CatalogErrors.SellableItemPriceNotFound}' (CAT-002) but got '{result.Error}'");

        RepositoryMock.Verify(
            x => x.GetBySellableItemIdAndKindAsync(
                It.Is<string>(id => id == sellableItemId),
                It.Is<PriceKind>(k => k == PriceKind.OneTime),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetBySellableItemIdAndKindAsync called exactly once before returning not-found failure");

        RepositoryMock.VerifyNoOtherCalls();
    }
}
