using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using Infrastructure.Persistence.Catalog.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using NMoneys;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SellableItemPriceRepositoryTest;

public abstract class GenericSellableItemPriceRepositoryTest
{
    protected Mock<IDynamoDBContext> ContextMock = default!;
    protected Mock<IAmazonDynamoDB> DynamoDbClientMock = default!;
    protected Mock<ILogger<SellableItemPriceRepository>> LoggerMock = default!;
    protected SellableItemPriceRepository Repository = default!;

    [SetUp]
    public virtual void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        DynamoDbClientMock = new Mock<IAmazonDynamoDB>();
        LoggerMock = new Mock<ILogger<SellableItemPriceRepository>>();
        Repository = new SellableItemPriceRepository(
            DynamoDbClientMock.Object,
            ContextMock.Object,
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

    /// <summary>
    /// Creates a <see cref="Table"/> instance via reflection (internal constructor) so that
    /// <c>IDynamoDBContext.GetTargetTable&lt;T&gt;()</c> can be mocked for tests that exercise
    /// <see cref="VibraHeka.Infrastructure.Persistence.Repository.GenericDynamoRepository{T}.QueryIndexAsync"/>
    /// (which uses the raw AWS client).
    /// A dedicated throwaway client mock is used so that construction-time accesses to
    /// <c>IAmazonService.Config</c> do not pollute <see cref="DynamoDbClientMock"/>.
    /// </summary>
    protected static Table BuildFakeTable(string tableName = "test-table")
    {
        Mock<IAmazonDynamoDB> fakeClientForTable = new();
        TableConfig config = new(tableName);
        ConstructorInfo ctor = typeof(Table).GetConstructors(
            BindingFlags.Instance | BindingFlags.NonPublic)[0];
        return (Table)ctor.Invoke([fakeClientForTable.Object, config]);
    }
}
