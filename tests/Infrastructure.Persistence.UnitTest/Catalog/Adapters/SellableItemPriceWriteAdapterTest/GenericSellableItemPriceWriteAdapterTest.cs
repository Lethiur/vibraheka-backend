using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Catalog.Adapters;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using NMoneys;
using NUnit.Framework;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Catalog.Adapters.SellableItemPriceWriteAdapterTest;

public abstract class GenericSellableItemPriceWriteAdapterTest
{
    protected Mock<IDynamoDBContext> ContextMock = default!;
    protected Mock<ITransactWrite<SellableItemPriceDBModel>> TransactWriteMock = default!;
    protected AWSConfig Config = default!;
    protected SellableItemPriceEntityMapper Mapper = default!;
    protected SellableItemPriceWriteAdapter Adapter = default!;

    [SetUp]
    public virtual void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        TransactWriteMock = new Mock<ITransactWrite<SellableItemPriceDBModel>>();
        Config = new AWSConfig { SellableItemPricesTable = "unit-test-sellable-item-prices-write-table" };
        Mapper = new SellableItemPriceEntityMapper();
        Adapter = new SellableItemPriceWriteAdapter(Mapper, Config, ContextMock.Object);

        ContextMock
            .Setup(x => x.CreateTransactWrite<SellableItemPriceDBModel>(It.IsAny<TransactWriteConfig>()))
            .Returns(TransactWriteMock.Object);
    }

    protected static SellableItemPriceEntity BuildDefaultSellableItemPriceEntity() =>
        new()
        {
            SellableItemPriceID = "sip-unit-write-test-001",
            SellableItemID = "si-unit-write-ref-001",
            Amount = new Money(19.99m, CurrencyIsoCode.EUR),
            Kind = PriceKind.Recurring,
            ExternalProductID = "ext-prod-write-test-001",
            ExternalPriceID = "ext-price-write-test-001",
        };
}

