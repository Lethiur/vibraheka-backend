using Infrastructure.Persistence.Catalog.Adapters;
using Infrastructure.Persistence.Catalog.Repositories;
using Moq;
using NMoneys;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Adapters.SellableItemPriceAdapterTest;

public abstract class GenericSellableItemPriceAdapterTest
{
    protected Mock<ISellableItemPriceRepository> RepositoryMock = default!;
    protected SellableItemPriceAdapter Adapter = default!;

    [SetUp]
    public virtual void SetUp()
    {
        RepositoryMock = new Mock<ISellableItemPriceRepository>();
        Adapter = new SellableItemPriceAdapter(RepositoryMock.Object);
    }

    protected static SellableItemPriceEntity BuildDefaultSellableItemPriceEntity(
        string sellableItemId = "sellable-item-unit-001",
        PriceKind kind = PriceKind.OneTime) =>
        new()
        {
            SellableItemPriceID = "sip-unit-test-001",
            SellableItemID = sellableItemId,
            Amount = new Money(9.99m, CurrencyIsoCode.EUR),
            Kind = kind,
            ExternalProductID = "ext-prod-adapter-test",
            ExternalPriceID = "ext-price-adapter-test",
        };
}
