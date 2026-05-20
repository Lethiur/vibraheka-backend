using System.ComponentModel;
using NMoneys;
using NUnit.Framework;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Commerce.Entities;
using OrderLineEntityFactoryClass = VibraHeka.Application.Commerce.Factories.OrderLineEntityFactory;

namespace VibraHeka.Domain.UnitTests.Commerce.Factories.OrderLineEntityFactory.FromSellableInformation;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class FromSellableInformationTest : GenericOrderLineEntityFactoryFromSellableInformationTest
{
    [Test]
    [DisplayName("Should set SellableItemID from the SellableItemEntity")]
    public void ShouldSetSellableItemIDFromSellableItem()
    {
        // Given: a valid SellableItemEntity and SellableItemPriceEntity
        SellableItemEntity sellableItem = BuildSellableItem();
        SellableItemPriceEntity sellableItemPrice = BuildSellableItemPrice();

        // When: FromSellableInformation is called
        OrderLineEntity result = OrderLineEntityFactoryClass.FromSellableInformation(
            sellableItem, sellableItemPrice, FakeUserId);

        // Then: SellableItemID matches the entity's ID
        Assert.That(result.SellableItemID, Is.EqualTo(FakeSellableItemId),
            $"Expected SellableItemID='{FakeSellableItemId}' but got '{result.SellableItemID}'");
    }

    [Test]
    [DisplayName("Should set NameSnapshot from the SellableItemEntity Name")]
    public void ShouldSetNameSnapshotFromSellableItemName()
    {
        // Given
        SellableItemEntity sellableItem = BuildSellableItem();
        SellableItemPriceEntity sellableItemPrice = BuildSellableItemPrice();

        // When
        OrderLineEntity result = OrderLineEntityFactoryClass.FromSellableInformation(
            sellableItem, sellableItemPrice, FakeUserId);

        // Then
        Assert.That(result.NameSnapshot, Is.EqualTo(FakeItemName),
            $"Expected NameSnapshot='{FakeItemName}' but got '{result.NameSnapshot}'");
    }

    [Test]
    [DisplayName("Should set Total and UnitPrice from the SellableItemPriceEntity Amount")]
    public void ShouldSetTotalAndUnitPriceFromAmount()
    {
        // Given
        SellableItemEntity sellableItem = BuildSellableItem();
        SellableItemPriceEntity sellableItemPrice = BuildSellableItemPrice();

        // When
        OrderLineEntity result = OrderLineEntityFactoryClass.FromSellableInformation(
            sellableItem, sellableItemPrice, FakeUserId);

        // Then
        Assert.That(result.Total, Is.EqualTo(Money.Zero()),
            "Expected Total to equal the SellableItemPriceEntity.Amount (Money.Zero)");
        Assert.That(result.UnitPrice, Is.EqualTo(Money.Zero()),
            "Expected UnitPrice to equal the SellableItemPriceEntity.Amount (Money.Zero)");
    }

    [Test]
    [DisplayName("Should set PaymentGatewayPriceIDSnapshot from the SellableItemPriceEntity ExternalPriceID")]
    public void ShouldSetPaymentGatewayPriceIdSnapshot()
    {
        // Given
        SellableItemEntity sellableItem = BuildSellableItem();
        SellableItemPriceEntity sellableItemPrice = BuildSellableItemPrice();

        // When
        OrderLineEntity result = OrderLineEntityFactoryClass.FromSellableInformation(
            sellableItem, sellableItemPrice, FakeUserId);

        // Then
        Assert.That(result.PaymentGatewayPriceIDSnapshot, Is.EqualTo(FakeExternalPriceId),
            $"Expected PaymentGatewayPriceIDSnapshot='{FakeExternalPriceId}' but got '{result.PaymentGatewayPriceIDSnapshot}'");
    }

    [Test]
    [DisplayName("Should set PaymentGatewayProductIDSnapshot from the SellableItemPriceEntity ExternalProductID")]
    public void ShouldSetPaymentGatewayProductIdSnapshot()
    {
        // Given
        SellableItemEntity sellableItem = BuildSellableItem();
        SellableItemPriceEntity sellableItemPrice = BuildSellableItemPrice();

        // When
        OrderLineEntity result = OrderLineEntityFactoryClass.FromSellableInformation(
            sellableItem, sellableItemPrice, FakeUserId);

        // Then
        Assert.That(result.PaymentGatewayProductIDSnapshot, Is.EqualTo(FakeExternalProductId),
            $"Expected PaymentGatewayProductIDSnapshot='{FakeExternalProductId}' but got '{result.PaymentGatewayProductIDSnapshot}'");
    }

    [Test]
    [DisplayName("Should set Type to Product")]
    public void ShouldSetTypeToProduct()
    {
        // Given
        SellableItemEntity sellableItem = BuildSellableItem();
        SellableItemPriceEntity sellableItemPrice = BuildSellableItemPrice();

        // When
        OrderLineEntity result = OrderLineEntityFactoryClass.FromSellableInformation(
            sellableItem, sellableItemPrice, FakeUserId);

        // Then
        Assert.That(result.Type, Is.EqualTo(SellableItemType.Product),
            $"Expected Type=Product but got '{result.Type}'");
    }

    [Test]
    [DisplayName("Should set CreatedBy and LastModifiedBy from the provided userId")]
    public void ShouldSetAuditFieldsFromUserId()
    {
        // Given
        SellableItemEntity sellableItem = BuildSellableItem();
        SellableItemPriceEntity sellableItemPrice = BuildSellableItemPrice();

        // When
        OrderLineEntity result = OrderLineEntityFactoryClass.FromSellableInformation(
            sellableItem, sellableItemPrice, FakeUserId);

        // Then
        Assert.That(result.CreatedBy, Is.EqualTo(FakeUserId),
            $"Expected CreatedBy='{FakeUserId}' but got '{result.CreatedBy}'");
        Assert.That(result.LastModifiedBy, Is.EqualTo(FakeUserId),
            $"Expected LastModifiedBy='{FakeUserId}' but got '{result.LastModifiedBy}'");
    }

    [Test]
    [DisplayName("Should generate a non-empty OrderLineID on each call")]
    public void ShouldGenerateNonEmptyOrderLineId()
    {
        // Given
        SellableItemEntity sellableItem = BuildSellableItem();
        SellableItemPriceEntity sellableItemPrice = BuildSellableItemPrice();

        // When: factory called twice
        OrderLineEntity firstResult = OrderLineEntityFactoryClass.FromSellableInformation(
            sellableItem, sellableItemPrice, FakeUserId);
        OrderLineEntity secondResult = OrderLineEntityFactoryClass.FromSellableInformation(
            sellableItem, sellableItemPrice, FakeUserId);

        // Then: each call generates a unique non-empty OrderLineID
        Assert.That(firstResult.OrderLineID, Is.Not.Null.And.Not.Empty,
            "Expected OrderLineID to be non-empty after FromSellableInformation");
        Assert.That(firstResult.OrderLineID, Is.Not.EqualTo(secondResult.OrderLineID),
            "Expected each call to generate a distinct OrderLineID");
    }

    [Test]
    [DisplayName("Should set DiscountAmount and TaxAmount to Money.Zero")]
    public void ShouldSetDiscountAndTaxAmountsToZero()
    {
        // Given
        SellableItemEntity sellableItem = BuildSellableItem();
        SellableItemPriceEntity sellableItemPrice = BuildSellableItemPrice();

        // When
        OrderLineEntity result = OrderLineEntityFactoryClass.FromSellableInformation(
            sellableItem, sellableItemPrice, FakeUserId);

        // Then
        Assert.That(result.DiscountAmount, Is.EqualTo(Money.Zero()),
            "Expected DiscountAmount=Money.Zero() but got non-zero discount");
        Assert.That(result.TaxAmount, Is.EqualTo(Money.Zero()),
            "Expected TaxAmount=Money.Zero() but got non-zero tax");
    }
}


