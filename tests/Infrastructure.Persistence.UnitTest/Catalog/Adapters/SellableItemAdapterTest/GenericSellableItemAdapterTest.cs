using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Catalog.Adapters;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using Infrastructure.Persistence.Catalog.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Catalog.Adapters.SellableItemAdapterTest;

public abstract class GenericSellableItemAdapterTest
{
    protected Mock<IDynamoDBContext> ContextMock = default!;
    protected Mock<IAmazonDynamoDB> DynamoDbClientMock = default!;
    protected Mock<ILogger<SellableItemRepository>> LoggerMock = default!;
    protected SellableItemRepository Repository = default!;
    protected SellableItemAdapter Adapter = default!;

    [SetUp]
    public virtual void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        DynamoDbClientMock = new Mock<IAmazonDynamoDB>();
        LoggerMock = new Mock<ILogger<SellableItemRepository>>();
        Repository = new SellableItemRepository(
            DynamoDbClientMock.Object,
            ContextMock.Object,
            new SellableItemEntityMapper(),
            LoggerMock.Object);
        Adapter = new SellableItemAdapter(Repository);
    }

    protected static SellableItemDBModel BuildDefaultSellableItemDBModel(string referenceId = "ref-id-001") =>
        new()
        {
            SellableItemID = "sellable-item-unit-001",
            Type = VibraHeka.Domain.Catalog.Entities.SellableItemType.Product,
            ReferenceID = referenceId,
            Name = "Test Sellable Item Adapter",
            IsActive = true,
        };
}

