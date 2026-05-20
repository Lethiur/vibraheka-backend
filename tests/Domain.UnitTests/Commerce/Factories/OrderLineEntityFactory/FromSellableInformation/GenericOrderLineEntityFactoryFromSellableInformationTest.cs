using NMoneys;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Domain.UnitTests.Commerce.Factories.OrderLineEntityFactory.FromSellableInformation;

public abstract class GenericOrderLineEntityFactoryFromSellableInformationTest
{
    protected const string FakeUserId = "user-linefactory-test-001";
    protected const string FakeSellableItemId = "item-linefactory-001";
    protected const string FakeSellableItemPriceId = "price-linefactory-001";
    protected const string FakeItemName = "Test Sellable Item Factory";
    protected const string FakeExternalPriceId = "price_stripe_linefactory_001";
    protected const string FakeExternalProductId = "prod_stripe_linefactory_001";

    protected static SellableItemEntity BuildSellableItem() =>
        new()
        {
            SellableItemID = FakeSellableItemId,
            Name = FakeItemName,
            IsActive = true
        };

    protected static SellableItemPriceEntity BuildSellableItemPrice() =>
        new()
        {
            SellableItemPriceID = FakeSellableItemPriceId,
            SellableItemID = FakeSellableItemId,
            Amount = Money.Zero(),
            ExternalPriceID = FakeExternalPriceId,
            ExternalProductID = FakeExternalProductId
        };
}

