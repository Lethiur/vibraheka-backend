using CSharpFunctionalExtensions;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Errors;

namespace Infrastructure.Persistence.IntegrationTests.Catalog.Repositories.SellableItemPriceRepositoryTest;

[TestFixture]
[NUnit.Framework.Category("Integration")]
public sealed class GetBySellableItemIdAndKindAsyncIntegrationTest
    : GenericSellableItemPriceRepositoryIntegrationTest
{
    private string? LastCreatedSellableItemPriceId;

    [TearDown]
    public async Task TearDown()
    {
        if (LastCreatedSellableItemPriceId is not null)
        {
            await CleanupSellableItemPrice(LastCreatedSellableItemPriceId);
            LastCreatedSellableItemPriceId = null;
        }
    }

    [Test]
    [Description("Should return success with mapped SellableItemPriceEntity when a matching price exists")]
    public async Task ShouldReturnSuccessWhenMatchingPriceExistsForSellableItem()
    {
        // Given: a SellableItemPriceDBModel seeded with the expected sellableItemId and kind
        string sellableItemId = Guid.NewGuid().ToString();
        PriceKind kind = PriceKind.OneTime;
        LastCreatedSellableItemPriceId = await SeedSellableItemPriceAsync(sellableItemId, kind);

        // When: GetBySellableItemIdAndKindAsync is called with matching parameters
        Result<SellableItemPriceEntity> result =
            await SellableItemPriceRepository.GetBySellableItemIdAndKindAsync(
                sellableItemId, kind, CancellationToken.None);

        // Then: result should be success with the correctly mapped entity
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.SellableItemID, Is.EqualTo(sellableItemId),
            $"Expected SellableItemID '{sellableItemId}' but got '{result.Value.SellableItemID}'");
        Assert.That(result.Value.Kind, Is.EqualTo(kind),
            $"Expected Kind '{kind}' but got '{result.Value.Kind}'");
        Assert.That(result.Value.SellableItemPriceID, Is.EqualTo(LastCreatedSellableItemPriceId),
            $"Expected SellableItemPriceID '{LastCreatedSellableItemPriceId}' but got '{result.Value.SellableItemPriceID}'");
    }

    [Test]
    [Description("Should return CAT-002 failure when no price exists for the given sellable item ID")]
    public async Task ShouldReturnCAT002FailureWhenNoPriceExistsForSellableItem()
    {
        // Given: a sellableItemId that has no prices in DynamoDB
        string nonExistentSellableItemId = Guid.NewGuid().ToString();

        // When: GetBySellableItemIdAndKindAsync is called with a non-existent sellableItemId
        Result<SellableItemPriceEntity> result =
            await SellableItemPriceRepository.GetBySellableItemIdAndKindAsync(
                nonExistentSellableItemId, PriceKind.OneTime, CancellationToken.None);

        // Then: result should be failure with CAT-002 (SellableItemPriceNotFound)
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure for non-existent sellable item but got success with ID: '{(result.IsSuccess ? result.Value.SellableItemPriceID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.SellableItemPriceNotFound),
            $"Expected error '{CatalogErrors.SellableItemPriceNotFound}' (CAT-002) but got '{result.Error}'");
    }
}
