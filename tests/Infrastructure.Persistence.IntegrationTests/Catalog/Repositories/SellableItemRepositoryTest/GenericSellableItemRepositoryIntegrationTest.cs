using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using Infrastructure.Persistence.Catalog.Repositories;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.IntegrationTests.Catalog.Repositories.SellableItemRepositoryTest;

public abstract class GenericSellableItemRepositoryIntegrationTest : TestBase
{
    protected SellableItemRepository SellableItemRepository = default!;
    protected IDynamoDBContext DynamoContext = default!;
    protected ILogger<SellableItemRepository> Logger = default!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        base.OneTimeSetUp();
        Logger = CreateTestLogger<SellableItemRepository>();
        DynamoContext = CreateDynamoDBContext();
        SellableItemRepository = new SellableItemRepository(
            _client,
            DynamoContext,
            new SellableItemEntityMapper(),
            Logger);
    }

    [OneTimeTearDown]
    public void OneTimeTearDownContext()
    {
        DynamoContext.Dispose();
    }

    protected async Task CleanupSellableItem(string sellableItemId)
    {
        try
        {
            await DynamoContext.DeleteAsync<SellableItemDBModel>(sellableItemId);
            Console.WriteLine($"Cleanup: Deleted SellableItem {sellableItemId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not cleanup SellableItem {sellableItemId}: {ex.Message}");
        }
    }

    protected async Task<string> SeedSellableItemAsync(string referenceId)
    {
        SellableItemDBModel model = new SellableItemDBModel
        {
            SellableItemID = Guid.NewGuid().ToString(),
            ReferenceID = referenceId,
            Name = _faker.Commerce.ProductName(),
            IsActive = true,
            Type = VibraHeka.Domain.Catalog.Entities.SellableItemType.Product,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "integration-test",
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = "integration-test",
        };
        await DynamoContext.SaveAsync(model);
        Console.WriteLine($"Seeded SellableItem {model.SellableItemID} with ReferenceId {referenceId}");
        return model.SellableItemID;
    }
}
