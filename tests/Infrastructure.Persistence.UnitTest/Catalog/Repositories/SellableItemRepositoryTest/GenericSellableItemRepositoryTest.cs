using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using Infrastructure.Persistence.Catalog.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Repositories.SellableItemRepositoryTest;

public abstract class GenericSellableItemRepositoryTest
{
    protected Mock<IDynamoDBContext> ContextMock = default!;
    protected Mock<IAmazonDynamoDB> DynamoDbClientMock = default!;
    protected Mock<ILogger<SellableItemRepository>> LoggerMock = default!;
    protected SellableItemRepository Repository = default!;

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
    }

    protected static SellableItemDBModel BuildDefaultSellableItemDBModel(string referenceId = "ref-id-001") =>
        new()
        {
            SellableItemID = "sellable-item-001",
            Type = SellableItemType.Product,
            ReferenceID = referenceId,
            Name = "Test Sellable Item",
            IsActive = true,
        };
}

