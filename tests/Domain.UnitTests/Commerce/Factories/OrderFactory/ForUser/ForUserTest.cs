using System.ComponentModel;
using NMoneys;
using NUnit.Framework;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Commerce.Enums;
using OrderFactoryClass = VibraHeka.Domain.Commerce.Factories.OrderFactory;

namespace VibraHeka.Domain.UnitTests.Commerce.Factories.OrderFactory.ForUser;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class ForUserTest : GenericOrderFactoryForUserTest
{
    [Test]
    [DisplayName("Should set UserId from the provided parameter")]
    public void ShouldSetUserIdFromParameter()
    {
        // Given: a known user ID (FakeUserId from base class)

        // When: ForUser factory method is called
        OrderEntity order = OrderFactoryClass.ForUser(FakeUserId);

        // Then: UserId matches the provided value
        Assert.That(order.UserID, Is.EqualTo(FakeUserId),
            $"Expected UserId='{FakeUserId}' but got '{order.UserID}'");
    }

    [Test]
    [DisplayName("Should initialize Status to Draft")]
    public void ShouldInitializeStatusToDraft()
    {
        // Given / When: ForUser factory method is called
        OrderEntity order = OrderFactoryClass.ForUser(FakeUserId);

        // Then: Status is Draft
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft),
            $"Expected Status=Draft but got '{order.Status}'");
    }

    [Test]
    [DisplayName("Should initialize all monetary amounts to Money.Zero")]
    public void ShouldInitializeAllMonetaryAmountsToZero()
    {
        // Given / When: ForUser factory method is called
        OrderEntity order = OrderFactoryClass.ForUser(FakeUserId);

        // Then: all monetary amounts are Money.Zero
        Assert.That(order.Total, Is.EqualTo(Money.Zero()),
            "Expected Total=Money.Zero() on new order");
        Assert.That(order.Subtotal, Is.EqualTo(Money.Zero()),
            "Expected Subtotal=Money.Zero() on new order");
        Assert.That(order.TaxTotal, Is.EqualTo(Money.Zero()),
            "Expected TaxTotal=Money.Zero() on new order");
        Assert.That(order.DiscountTotal, Is.EqualTo(Money.Zero()),
            "Expected DiscountTotal=Money.Zero() on new order");
    }

    [Test]
    [DisplayName("Should initialize Lines to an empty collection")]
    public void ShouldInitializeLinesAsEmptyCollection()
    {
        // Given / When: ForUser factory method is called
        OrderEntity order = OrderFactoryClass.ForUser(FakeUserId);

        // Then: Lines collection is empty
        Assert.That(order.Lines, Is.Empty,
            $"Expected Lines to be empty but got {order.Lines.Count} items");
    }

    [Test]
    [DisplayName("Should generate a non-empty unique OrderID on each call")]
    public void ShouldGenerateNonEmptyUniqueOrderId()
    {
        // Given / When: ForUser factory method is called twice
        OrderEntity firstOrder = OrderFactoryClass.ForUser(FakeUserId);
        OrderEntity secondOrder = OrderFactoryClass.ForUser(FakeUserId);

        // Then: OrderID is non-empty and each call produces a distinct ID
        Assert.That(firstOrder.OrderID, Is.Not.Null.And.Not.Empty,
            "Expected OrderID to be non-empty after ForUser");
        Assert.That(secondOrder.OrderID, Is.Not.Null.And.Not.Empty,
            "Expected OrderID to be non-empty after second ForUser call");
        Assert.That(firstOrder.OrderID, Is.Not.EqualTo(secondOrder.OrderID),
            "Expected each ForUser call to produce a distinct OrderID");
    }

    [Test]
    [DisplayName("Should set CreatedBy and LastModifiedBy from the provided userId")]
    public void ShouldSetAuditFieldsFromUserId()
    {
        // Given / When: ForUser factory method is called
        OrderEntity order = OrderFactoryClass.ForUser(FakeUserId);

        // Then: audit fields are set to the provided userId
        Assert.That(order.CreatedBy, Is.EqualTo(FakeUserId),
            $"Expected CreatedBy='{FakeUserId}' but got '{order.CreatedBy}'");
        Assert.That(order.LastModifiedBy, Is.EqualTo(FakeUserId),
            $"Expected LastModifiedBy='{FakeUserId}' but got '{order.LastModifiedBy}'");
    }
}

