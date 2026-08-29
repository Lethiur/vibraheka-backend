using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Commerce.Mappers;
using Infrastructure.Persistence.Commerce.Models;
using Infrastructure.Persistence.Commerce.Repositories;
using Microsoft.Extensions.Logging;
using NMoneys;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Commerce.Entities;

namespace Infrastructure.Persistence.IntegrationTests.Commerce.Repositories.OrderLineRepositoryTest;

public abstract class GenericOrderLineRepositoryIntegrationTest : TestBase
{
    protected OrderLineRepository OrderLineRepository = default!;
    protected IDynamoDBContext DynamoContext = default!;
    protected ILogger<OrderLineRepository> Logger = default!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        base.OneTimeSetUp();
        Logger = CreateTestLogger<OrderLineRepository>();
        DynamoContext = CreateDynamoDBContext();
        OrderLineRepository = new OrderLineRepository(
            _client,
            DynamoContext,
            new OrderLineMapper(),
            Logger);
    }

    [OneTimeTearDown]
    public void OneTimeTearDownContext()
    {
        DynamoContext.Dispose();
    }

    protected async Task CleanupOrderLine(string orderLineId)
    {
        try
        {
            await DynamoContext.DeleteAsync<OrderLineDBModel>(orderLineId);
            Console.WriteLine($"Cleanup: Deleted OrderLine {orderLineId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not cleanup OrderLine {orderLineId}: {ex.Message}");
        }
    }

    protected OrderLineEntity CreateValidOrderLineEntity(string orderId, string userId) =>
        new()
        {
            OrderLineID = Guid.NewGuid().ToString(),
            OrderID = orderId,
            SellableItemID = $"item-integration-{Guid.NewGuid()}",
            SellablePriceID = $"price-integration-{Guid.NewGuid()}",
            NameSnapshot = "Integration Test Item",
            Type = SellableItemType.Product,
            Quantity = 1,
            UnitPrice = Money.Zero(),
            Subtotal = Money.Zero(),
            DiscountAmount = Money.Zero(),
            TaxAmount = Money.Zero(),
            Total = Money.Zero(),
            PaymentGatewayPriceIDSnapshot = "price_stripe_integration",
            PaymentGatewayProductIDSnapshot = "prod_stripe_integration",
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = userId,
            LastModifiedBy = userId
        };
}

