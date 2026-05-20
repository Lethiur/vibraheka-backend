using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Commerce.Adapters;
using Infrastructure.Persistence.Commerce.Mappers;
using Infrastructure.Persistence.Commerce.Models;
using Moq;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Commerce.Enums;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Commerce.Adapters.OrderWriteAdapterTest;

public abstract class GenericOrderWriteAdapterTest
{
    protected Mock<IDynamoDBContext> ContextMock = default!;
    protected Mock<ITransactWrite<OrderDBModel>> TransactWriteMock = default!;
    protected AWSConfig Config = default!;
    protected OrderMapper Mapper = default!;
    protected OrderWriteAdapter Adapter = default!;

    [SetUp]
    public virtual void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        TransactWriteMock = new Mock<ITransactWrite<OrderDBModel>>();
        Config = new AWSConfig { OrdersTable = "unit-test-orders-table" };
        Mapper = new OrderMapper();
        Adapter = new OrderWriteAdapter(Mapper, Config, ContextMock.Object);

        ContextMock
            .Setup(x => x.CreateTransactWrite<OrderDBModel>(It.IsAny<TransactWriteConfig>()))
            .Returns(TransactWriteMock.Object);
    }

    protected static OrderEntity BuildDefaultOrderEntity() =>
        new()
        {
            OrderID = "order-unit-test-001",
            UserID = "user-unit-test-001",
            Status = OrderStatus.Draft,
            Lines = []
        };
}

