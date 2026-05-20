using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Catalog.Adapters;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Catalog.Adapters.SellableItemWriteAdapterTest;

public abstract class GenericSellableItemWriteAdapterTest
{
    protected Mock<IDynamoDBContext> ContextMock = default!;
    protected Mock<ITransactWrite<SellableItemDBModel>> TransactWriteMock = default!;
    protected AWSConfig Config = default!;
    protected SellableItemEntityMapper Mapper = default!;
    protected SellableItemWriteAdapter Adapter = default!;

    [SetUp]
    public virtual void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        TransactWriteMock = new Mock<ITransactWrite<SellableItemDBModel>>();
        Config = new AWSConfig { SellableItemsTable = "unit-test-sellable-items-write-table" };
        Mapper = new SellableItemEntityMapper();
        Adapter = new SellableItemWriteAdapter(Mapper, Config, ContextMock.Object);

        ContextMock
            .Setup(x => x.CreateTransactWrite<SellableItemDBModel>(It.IsAny<TransactWriteConfig>()))
            .Returns(TransactWriteMock.Object);
    }

    protected static SellableItemEntity BuildDefaultSellableItemEntity() =>
        new()
        {
            SellableItemID = "si-unit-write-test-001",
            Name = "Test Sellable Item Write Adapter",
            IsActive = true,
            Type = VibraHeka.Domain.Catalog.Entities.SellableItemType.Product,
            ReferenceID = "ref-write-test-001",
        };
}

