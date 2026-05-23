using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Commerce.Mappers;
using Infrastructure.Persistence.Commerce.Repositories;
using Microsoft.Extensions.Logging;
using NMoneys;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Commerce.Enums;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Commerce.Repositories.OrderRepositoryTest;

public abstract class GenericOrderRepositoryIntegrationTest : TestBase
{
    protected OrderRepository OrderRepository = default!;
    protected IDynamoDBContext DynamoContext = default!;
    protected ILogger<OrderRepository> Logger = default!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        base.OneTimeSetUp();
        Logger = CreateTestLogger<OrderRepository>();
        DynamoContext = CreateDynamoDBContext();
        OrderRepository = new OrderRepository(
            _client,
            DynamoContext,
            new OrderMapper(),
            Logger);
    }

    [OneTimeTearDown]
    public void OneTimeTearDownContext()
    {
        DynamoContext.Dispose();
    }

    protected async Task CleanupOrder(string orderId)
    {
        try
        {
            await DynamoContext.DeleteAsync(orderId);
            Console.WriteLine($"Cleanup: Deleted Order {orderId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not cleanup Order {orderId}: {ex.Message}");
        }
    }

    protected OrderEntity CreateValidOrderEntity(string userId) =>
        new()
        {
            OrderID = Guid.NewGuid().ToString(),
            UserID = userId,
            Status = OrderStatus.Draft,
            Total = Money.Zero(),
            Subtotal = Money.Zero(),
            TaxTotal = Money.Zero(),
            DiscountTotal = Money.Zero(),
            PaidAt = DateTimeOffset.UtcNow,
            Lines = [],
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = userId,
            LastModifiedBy = userId
        };
}

