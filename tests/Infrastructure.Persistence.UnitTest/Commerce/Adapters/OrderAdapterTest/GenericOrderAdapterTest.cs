using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Commerce.Adapters;
using Infrastructure.Persistence.Commerce.Mappers;
using Infrastructure.Persistence.Commerce.Models;
using Infrastructure.Persistence.Commerce.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using NMoneys;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Commerce.Enums;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Commerce.Adapters.OrderAdapterTest;

public abstract class GenericOrderAdapterTest
{
    protected Mock<IDynamoDBContext> ContextMock = default!;
    protected Mock<IAmazonDynamoDB> DynamoDbClientMock = default!;
    protected Mock<ILogger<OrderRepository>> OrderRepositoryLoggerMock = default!;
    protected Mock<ILogger<OrderLineRepository>> OrderLineRepositoryLoggerMock = default!;
    protected Mock<ILogger<OrderAdapter>> AdapterLoggerMock = default!;
    protected Mock<IBatchWrite<OrderLineDBModel>> BatchWriteMock = default!;
    protected AWSConfig Config = default!;
    protected OrderAdapter Adapter = default!;

    [SetUp]
    public virtual void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        DynamoDbClientMock = new Mock<IAmazonDynamoDB>();
        OrderRepositoryLoggerMock = new Mock<ILogger<OrderRepository>>();
        OrderLineRepositoryLoggerMock = new Mock<ILogger<OrderLineRepository>>();
        AdapterLoggerMock = new Mock<ILogger<OrderAdapter>>();
        BatchWriteMock = new Mock<IBatchWrite<OrderLineDBModel>>();

        Config = new AWSConfig
        {
            OrdersTable = "unit-test-orders-table",
            OrderLineTable = "unit-test-order-lines-table",
            EmailTemplatesBucketName = "n/a",
            UserCodesTable = "n/a",
            EmailTemplatesTable = "n/a",
            UsersTable = "n/a",
            ClientId = "n/a",
            UserPoolId = "n/a",
            Location = "n/a",
            Profile = "n/a",
            ActionLogTable = "n/a",
            SubscriptionTable = "n/a",
            SubscriptionUserIdIndex = "n/a",
            RecordingsTierIndex = "n/a",
            SettingsNameSpace = "n/a",
            RecordingsBucketName = "n/a",
            RecordingsTable = "n/a",
            ProductTable = "n/a",
            SellableItemsTable = "n/a",
            SellableItemPricesTable = "n/a",
            SubscriptionPlansTable = "n/a",
            PaymentAttemptTable = "n/a"
        };

        OrderRepository orderRepository = new OrderRepository(
            DynamoDbClientMock.Object,
            ContextMock.Object,
            Config,
            new OrderMapper(),
            OrderRepositoryLoggerMock.Object);

        OrderLineRepository orderLineRepository = new OrderLineRepository(
            DynamoDbClientMock.Object,
            ContextMock.Object,
            Config,
            new OrderLineMapper(),
            OrderLineRepositoryLoggerMock.Object);

        Adapter = new OrderAdapter(
            orderLineRepository,
            orderRepository,
            AdapterLoggerMock.Object);

        // Default happy-path setup
        ContextMock
            .Setup(x => x.SaveAsync(It.IsAny<OrderDBModel>(), It.IsAny<SaveConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        ContextMock
            .Setup(x => x.CreateBatchWrite<OrderLineDBModel>(It.IsAny<BatchWriteConfig>()))
            .Returns(BatchWriteMock.Object);

        BatchWriteMock
            .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    protected static OrderEntity BuildValidOrderEntityNoLines(string? orderId = null) =>
        new()
        {
            OrderID = orderId ?? "order-adapter-unit-test-001",
            UserID = "user-adapter-unit-test-001",
            Status = OrderStatus.Draft,
            Total = Money.Zero(),
            Subtotal = Money.Zero(),
            TaxTotal = Money.Zero(),
            DiscountTotal = Money.Zero(),
            Lines = [],
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = "user-adapter-unit-test-001",
            LastModifiedBy = "user-adapter-unit-test-001"
        };

    protected static OrderLineEntity BuildValidOrderLine(string? lineId = null) =>
        new()
        {
            OrderLineID = lineId ?? "line-adapter-unit-test-001",
            OrderID = "order-adapter-unit-test-001",
            SellableItemID = "item-adapter-unit-test-001",
            Quantity = 1,
            Total = Money.Zero(),
            Subtotal = Money.Zero(),
            TaxAmount = Money.Zero(),
            DiscountAmount = Money.Zero()
        };
}


