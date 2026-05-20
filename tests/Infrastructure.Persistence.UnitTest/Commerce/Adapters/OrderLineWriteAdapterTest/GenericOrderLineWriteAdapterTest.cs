using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Commerce.Adapters;
using Infrastructure.Persistence.Commerce.Mappers;
using Infrastructure.Persistence.Commerce.Models;
using Moq;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Commerce.Adapters.OrderLineWriteAdapterTest;

public abstract class GenericOrderLineWriteAdapterTest
{
    protected Mock<IDynamoDBContext> ContextMock = default!;
    protected Mock<ITransactWrite<OrderLineDBModel>> TransactWriteMock = default!;
    protected AWSConfig Config = default!;
    protected OrderLineMapper Mapper = default!;
    protected OrderLineWriteAdapter Adapter = default!;

    [SetUp]
    public virtual void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        TransactWriteMock = new Mock<ITransactWrite<OrderLineDBModel>>();
        Config = new AWSConfig { OrderLineTable = "unit-test-order-lines-table" };
        Mapper = new OrderLineMapper();
        Adapter = new OrderLineWriteAdapter(Mapper, Config, ContextMock.Object);

        ContextMock
            .Setup(x => x.CreateTransactWrite<OrderLineDBModel>(It.IsAny<TransactWriteConfig>()))
            .Returns(TransactWriteMock.Object);
    }

    protected static OrderLineEntity BuildDefaultOrderLineEntity() =>
        new()
        {
            OrderLineID = "line-unit-test-001",
            OrderID = "order-unit-test-001",
            SellableItemID = "item-unit-test-001",
            Quantity = 2
        };
}

