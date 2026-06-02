using Infrastructure.Persistence.Catalog.Adapters;
using Infrastructure.Persistence.Catalog.Repositories;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using static VibraHeka.Domain.Catalog.Entities.SellableItemType;
using static VibraHeka.Domain.Catalog.Enums.PriceKind;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Adapters.SellableItemAdapterTest;

public abstract class GenericSellableItemAdapterTest
{
    protected Mock<ISellableItemRepository> RepositoryMock = default!;
    protected Mock<ISellableItemPriceRepository> PriceRepositoryMock = default!;
    protected SellableItemAdapter Adapter = default!;

    [SetUp]
    public virtual void SetUp()
    {
        RepositoryMock = new Mock<ISellableItemRepository>();
        PriceRepositoryMock = new Mock<ISellableItemPriceRepository>();
        Adapter = new SellableItemAdapter(RepositoryMock.Object, PriceRepositoryMock.Object);
    }

    protected static SellableItemEntity BuildDefaultSellableItemEntity(string referenceId = "ref-id-001") =>
        new()
        {
            SellableItemID = "sellable-item-unit-001",
            Type = Product,
            ReferenceID = referenceId,
            Name = "Test Sellable Item Adapter",
            IsActive = true,
        };

    protected static SellableItemPriceEntity BuildDefaultSellableItemPriceEntity(string sellableItemId) =>
        new()
        {
            SellableItemPriceID = "price-unit-001",
            SellableItemID = sellableItemId,
            Amount = new NMoneys.Money(9.99m, NMoneys.Currency.Usd),
            Kind = OneTime,
            ExternalProductID = "ext-prod-001",
            ExternalPriceID = "ext-price-001",
        };
}
