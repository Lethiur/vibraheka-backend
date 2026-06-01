using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Adapters.SellableItemAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetSellableItemByIdAsyncTest : GenericSellableItemAdapterTest
{
    [Test]
    [DisplayName("Should return Result.Success propagated from repository without alteration when entity is found")]
    public async Task ShouldReturnSuccessWhenRepositoryReturnsEntity()
    {
        // Given: repository returns a valid entity for the requested sellableItemId
        string sellableItemId = "sellable-item-id-adapter-success-001";
        SellableItemEntity entity = BuildDefaultSellableItemEntity();

        RepositoryMock
            .Setup(x => x.FindByIDAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entity));

        // When: GetSellableItemByIdAsync is called on the adapter
        Result<SellableItemEntity> result =
            await Adapter.GetSellableItemByIdAsync(sellableItemId, CancellationToken.None);

        // Then: result is Success with entity data matching — adapter propagates without alteration
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.SellableItemID, Is.EqualTo(entity.SellableItemID),
            $"Expected SellableItemID '{entity.SellableItemID}' but got '{result.Value.SellableItemID}'");
        Assert.That(result.Value.Name, Is.EqualTo(entity.Name),
            $"Expected Name '{entity.Name}' but got '{result.Value.Name}'");

        RepositoryMock.Verify(
            x => x.FindByIDAsync(
                It.Is<string>(id => id == sellableItemId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected FindByIDAsync called once with sellableItemId='{sellableItemId}'");

        RepositoryMock.VerifyNoOtherCalls();
        PriceRepositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return Result.Failure propagated from repository without alteration when entity is not found")]
    public async Task ShouldReturnFailureWhenRepositoryReturnsFailure()
    {
        // Given: repository returns failure — no entity exists for the requested sellableItemId
        string sellableItemId = "sellable-item-id-adapter-notfound-002";

        RepositoryMock
            .Setup(x => x.FindByIDAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<SellableItemEntity>(GenericPersistenceErrors.NoRecordsFound));

        // When: GetSellableItemByIdAsync is called on the adapter
        Result<SellableItemEntity> result =
            await Adapter.GetSellableItemByIdAsync(sellableItemId, CancellationToken.None);

        // Then: result is Failure — adapter propagates the error without modification
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with SellableItemID: '{(result.IsSuccess ? result.Value.SellableItemID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.NoRecordsFound),
            $"Expected error '{GenericPersistenceErrors.NoRecordsFound}' (GPE-000) but got '{result.Error}'");

        RepositoryMock.Verify(
            x => x.FindByIDAsync(
                It.Is<string>(id => id == sellableItemId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected FindByIDAsync called exactly once before returning failure");

        RepositoryMock.VerifyNoOtherCalls();
        PriceRepositoryMock.VerifyNoOtherCalls();
    }
}

