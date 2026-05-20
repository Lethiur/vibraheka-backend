using System.ComponentModel;
using NMoneys;
using NUnit.Framework;
using VibraHeka.Domain.Payments.Enums;

namespace VibraHeka.Domain.UnitTests.Commerce.Entities.PaymentAttemptEntity.LinkOrder;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class LinkOrderTest : GenericPaymentAttemptEntityLinkOrderTest
{
    [Test]
    [DisplayName("Should set OrderId from the order entity after LinkOrder is called")]
    public void ShouldSetOrderIdFromOrderEntity()
    {
        // Given: an empty payment attempt and an order with a known OrderID
        Domain.Payments.Entities.PaymentAttemptEntity attempt = BuildEmptyPaymentAttemptEntity();
        Domain.Commerce.Entities.OrderEntity order = BuildOrderEntityForLinking();

        // When: LinkOrder is called
        attempt.LinkOrder(order);

        // Then: OrderId matches the order's OrderID
        Assert.That(attempt.OrderId, Is.EqualTo(FakeOrderId),
            $"Expected OrderId='{FakeOrderId}' but got '{attempt.OrderId}'");
    }

    [Test]
    [DisplayName("Should set UserId from the order entity after LinkOrder is called")]
    public void ShouldSetUserIdFromOrderEntity()
    {
        // Given: an empty payment attempt and an order with a known UserId
        Domain.Payments.Entities.PaymentAttemptEntity attempt = BuildEmptyPaymentAttemptEntity();
        Domain.Commerce.Entities.OrderEntity order = BuildOrderEntityForLinking();

        // When: LinkOrder is called
        attempt.LinkOrder(order);

        // Then: UserId matches the order's UserId
        Assert.That(attempt.UserId, Is.EqualTo(FakeUserId),
            $"Expected UserId='{FakeUserId}' but got '{attempt.UserId}'");
    }

    [Test]
    [DisplayName("Should set Amount from the order entity Total after LinkOrder is called")]
    public void ShouldSetAmountFromOrderTotal()
    {
        // Given: an order with zero Total and an empty payment attempt
        Domain.Payments.Entities.PaymentAttemptEntity attempt = BuildEmptyPaymentAttemptEntity();
        Domain.Commerce.Entities.OrderEntity order = BuildOrderEntityForLinking();

        // When: LinkOrder is called
        attempt.LinkOrder(order);

        // Then: Amount matches the order Total
        Assert.That(attempt.Amount, Is.EqualTo(Money.Zero()),
            $"Expected Amount=Money.Zero() but got '{attempt.Amount}'");
    }

    [Test]
    [DisplayName("Should set Status to Pending after LinkOrder is called")]
    public void ShouldSetStatusToPending()
    {
        // Given: an empty payment attempt and a valid order
        Domain.Payments.Entities.PaymentAttemptEntity attempt = BuildEmptyPaymentAttemptEntity();
        Domain.Commerce.Entities.OrderEntity order = BuildOrderEntityForLinking();

        // When: LinkOrder is called
        attempt.LinkOrder(order);

        // Then: Status is Pending
        Assert.That(attempt.Status, Is.EqualTo(PaymentsStatus.Pending),
            $"Expected Status=Pending but got '{attempt.Status}'");
    }

    [Test]
    [DisplayName("Should generate a non-empty PaymentAttemptID after LinkOrder is called")]
    public void ShouldGenerateNonEmptyPaymentAttemptId()
    {
        // Given: an empty payment attempt and a valid order
        Domain.Payments.Entities.PaymentAttemptEntity attempt = BuildEmptyPaymentAttemptEntity();
        Domain.Commerce.Entities.OrderEntity order = BuildOrderEntityForLinking();

        // When: LinkOrder is called
        attempt.LinkOrder(order);

        // Then: PaymentAttemptID is a non-empty GUID string
        Assert.That(attempt.PaymentAttemptID, Is.Not.Null.And.Not.Empty,
            "Expected PaymentAttemptID to be a non-empty GUID after LinkOrder");
    }

    [Test]
    [DisplayName("Should set CreatedBy and LastModifiedBy from the order UserId after LinkOrder is called")]
    public void ShouldSetAuditFieldsFromOrderUserId()
    {
        // Given: an empty payment attempt and a valid order
        Domain.Payments.Entities.PaymentAttemptEntity attempt = BuildEmptyPaymentAttemptEntity();
        Domain.Commerce.Entities.OrderEntity order = BuildOrderEntityForLinking();

        // When: LinkOrder is called
        attempt.LinkOrder(order);

        // Then: audit fields are set from the order's UserId
        Assert.That(attempt.CreatedBy, Is.EqualTo(FakeUserId),
            $"Expected CreatedBy='{FakeUserId}' but got '{attempt.CreatedBy}'");
        Assert.That(attempt.LastModifiedBy, Is.EqualTo(FakeUserId),
            $"Expected LastModifiedBy='{FakeUserId}' but got '{attempt.LastModifiedBy}'");
    }
}


