using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Catalog.Adapters.SellableItemPriceAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetSellableItemPriceByIdTest : GenericSellableItemPriceAdapterTest
{
    [Test]
    [DisplayName("Should return Result.Success propagated from repository without alteration when price entity is found")]
    public async Task ShouldReturnSuccessWhenRepositoryReturnsPriceEntity()
    {
        // Given: repository returns a valid price entity for the requested priceId
        string priceId = "sip-adapter-success-001";
        SellableItemPriceEntity entity = BuildDefaultSellableItemPriceEntity();

        RepositoryMock
            .Setup(x => x.GetBySellableItemPriceIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entity));

        // When: GetSellableItemPriceById is called on the adapter
        Result<SellableItemPriceEntity> result =
            await Adapter.GetSellableItemPriceById(priceId, CancellationToken.None);

        // Then: result is Success with entity data matching — adapter propagates without alteration
        Assert.That(
            result.IsSuccess,
            Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(
            result.Value.SellableItemPriceID,
            Is.EqualTo(entity.SellableItemPriceID),
            $"Expected SellableItemPriceID '{entity.SellableItemPriceID}' but got '{result.Value.SellableItemPriceID}'");

        RepositoryMock.Verify(
            x => x.GetBySellableItemPriceIdAsync(
                It.Is<string>(id => id == priceId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected GetBySellableItemPriceIdAsync called once with priceId='{priceId}'");

        RepositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return Result.Failure propagated from repository without alteration when price entity is not found")]
    public async Task ShouldReturnFailureWhenRepositoryReturnsFailure()
    {
        // Given: repository returns failure — no price entity exists for the requested priceId
        string priceId = "sip-adapter-notfound-002";

        RepositoryMock
            .Setup(x => x.GetBySellableItemPriceIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<SellableItemPriceEntity>(GenericPersistenceErrors.NoRecordsFound));

        // When: GetSellableItemPriceById is called on the adapter
        Result<SellableItemPriceEntity> result =
            await Adapter.GetSellableItemPriceById(priceId, CancellationToken.None);

        // Then: result is Failure — adapter propagates the error without modification
        Assert.That(
            result.IsFailure,
            Is.True,
            $"Expected failure but got success with ID: '{(result.IsSuccess ? result.Value.SellableItemPriceID : "N/A")}'");
        Assert.That(
            result.Error,
            Is.EqualTo(GenericPersistenceErrors.NoRecordsFound),
            $"Expected error '{GenericPersistenceErrors.NoRecordsFound}' (GPE-000) but got '{result.Error}'");

        RepositoryMock.Verify(
            x => x.GetBySellableItemPriceIdAsync(
                It.Is<string>(id => id == priceId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetBySellableItemPriceIdAsync called exactly once before returning failure");

        RepositoryMock.VerifyNoOtherCalls();
    }
}

