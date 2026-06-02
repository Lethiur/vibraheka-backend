using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using Infrastructure.Persistence.Catalog.Repositories;
using Microsoft.Extensions.Logging;
using NMoneys;
using VibraHeka.Domain.Catalog.Enums;

namespace Infrastructure.Persistence.IntegrationTests.Catalog.Repositories.SellableItemPriceRepositoryTest;

public abstract class GenericSellableItemPriceRepositoryIntegrationTest : TestBase
{
    protected SellableItemPriceRepository SellableItemPriceRepository = default!;
    protected IDynamoDBContext DynamoContext = default!;
    protected ILogger<SellableItemPriceRepository> Logger = default!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        base.OneTimeSetUp();
        Logger = CreateTestLogger<SellableItemPriceRepository>();
        DynamoContext = CreateDynamoDBContext();
        SellableItemPriceRepository = new SellableItemPriceRepository(
            _client,
            DynamoContext,
            new SellableItemPriceEntityMapper(),
            Logger);
    }

    [OneTimeTearDown]
    public void OneTimeTearDownContext()
    {
        DynamoContext.Dispose();
    }

    protected async Task CleanupSellableItemPrice(string sellableItemPriceId)
    {
        try
        {
            await DynamoContext.DeleteAsync<SellableItemPriceDBModel>(sellableItemPriceId);
            Console.WriteLine($"Cleanup: Deleted SellableItemPrice {sellableItemPriceId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not cleanup SellableItemPrice {sellableItemPriceId}: {ex.Message}");
        }
    }

    protected async Task<string> SeedSellableItemPriceAsync(string sellableItemId, PriceKind kind)
    {
        SellableItemPriceDBModel model = new SellableItemPriceDBModel
        {
            SellableItemPriceID = Guid.NewGuid().ToString(),
            SellableItemID = sellableItemId,
            Amount = new Money(9.99m, CurrencyIsoCode.EUR),
            Kind = kind,
            ExternalProductID = "prod_integration_test",
            ExternalPriceID = "price_integration_test",
            IsActive = true,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "integration-test",
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = "integration-test",
        };
        await DynamoContext.SaveAsync(model);
        Console.WriteLine($"Seeded SellableItemPrice {model.SellableItemPriceID} for SellableItem {sellableItemId}");
        return model.SellableItemPriceID;
    }
}
