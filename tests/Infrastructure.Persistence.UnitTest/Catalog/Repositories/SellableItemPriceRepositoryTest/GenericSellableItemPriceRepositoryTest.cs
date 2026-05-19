using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using Infrastructure.Persistence.Catalog.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using NMoneys;
using NUnit.Framework;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Infrastructure.Entities;
namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SellableItemPriceRepositoryTest;

public abstract class GenericSellableItemPriceRepositoryTest
{
    protected Mock<IDynamoDBContext> ContextMock = default!;
    protected Mock<IAmazonDynamoDB> DynamoDbClientMock = default!;
    protected Mock<ILogger<SellableItemPriceRepository>> LoggerMock = default!;
    protected AWSConfig Config = default!;
    protected SellableItemPriceRepository Repository = default!;
    [SetUp]
    public virtual void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        DynamoDbClientMock = new Mock<IAmazonDynamoDB>();
        LoggerMock = new Mock<ILogger<SellableItemPriceRepository>>();
        Config = new AWSConfig { SellableItemPricesTable = "unit-test-sellable-item-prices-table" };
        Repository = new SellableItemPriceRepository(
            DynamoDbClientMock.Object,
            ContextMock.Object,
            Config,
            new SellableItemPriceEntityMapper(),
            LoggerMock.Object);
    }
    protected static SellableItemPriceDBModel BuildDefaultSellableItemPriceDBModel(
        string sellableItemId = "sellable-item-001",
        PriceKind kind = PriceKind.OneTime) =>
        new()
        {
            SellableItemPriceID = Guid.NewGuid().ToString(),
            SellableItemID = sellableItemId,
            Amount = new Money(9.99m, CurrencyIsoCode.EUR),
            Kind = kind,
            ExternalProductID = "prod_test_abc",
            ExternalPriceID = "price_test_xyz",
            IsActive = true,
        };
}
