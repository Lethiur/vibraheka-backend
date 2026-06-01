using CSharpFunctionalExtensions;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Errors;

namespace Infrastructure.Persistence.IntegrationTests.Catalog.Repositories.SellableItemRepositoryTest;

[TestFixture]
[NUnit.Framework.Category("Integration")]
public sealed class GetByReferenceIdAsyncIntegrationTest : GenericSellableItemRepositoryIntegrationTest
{
    private string? LastCreatedSellableItemId;

    [TearDown]
    public async Task TearDown()
    {
        if (LastCreatedSellableItemId is not null)
        {
            await CleanupSellableItem(LastCreatedSellableItemId);
            LastCreatedSellableItemId = null;
        }
    }

    [Test]
    [Description("Should return success with mapped SellableItemEntity when a SellableItem with that referenceId exists in DynamoDB")]
    public async Task ShouldReturnSuccessWhenSellableItemExistsForReferenceId()
    {
        // Given: a SellableItemDBModel seeded with a specific referenceId
        string referenceId = Guid.NewGuid().ToString();
        LastCreatedSellableItemId = await SeedSellableItemAsync(referenceId);

        // When: GetByReferenceIdAsync is called with the seeded referenceId
        Result<SellableItemEntity> result =
            await SellableItemRepository.GetByReferenceIdAsync(referenceId, CancellationToken.None);

        // Then: result should be success with the entity matching the seeded data
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.SellableItemID, Is.EqualTo(LastCreatedSellableItemId),
            $"Expected SellableItemID '{LastCreatedSellableItemId}' but got '{result.Value.SellableItemID}'");
        Assert.That(result.Value.IsActive, Is.True,
            "Expected IsActive to be true for the seeded entity");
    }

    [Test]
    [Description("Should return CAT-001 failure when no SellableItem exists for the given referenceId")]
    public async Task ShouldReturnCAT001FailureWhenNoSellableItemExistsForReferenceId()
    {
        // Given: a referenceId that does not exist in DynamoDB
        string nonExistentReferenceId = Guid.NewGuid().ToString();

        // When: GetByReferenceIdAsync is called with a non-existent referenceId
        Result<SellableItemEntity> result =
            await SellableItemRepository.GetByReferenceIdAsync(nonExistentReferenceId, CancellationToken.None);

        // Then: result should be failure with CAT-001 (SellableItemNotFound)
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure for non-existent referenceId but got success with ID: '{(result.IsSuccess ? result.Value.SellableItemID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(CatalogErrors.SellableItemNotFound),
            $"Expected error '{CatalogErrors.SellableItemNotFound}' (CAT-001) but got '{result.Error}'");
    }
}
