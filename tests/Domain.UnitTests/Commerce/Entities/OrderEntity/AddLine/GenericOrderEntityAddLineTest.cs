using NMoneys;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Commerce.Factories;

namespace VibraHeka.Domain.UnitTests.Commerce.Entities.OrderEntity.AddLine;

public abstract class GenericOrderEntityAddLineTest
{
    protected const string FakeUserId = "user-domain-test-001";
    protected const string FakeOrderLineId = "line-domain-test-001";

    protected static Domain.Commerce.Entities.OrderEntity BuildEmptyOrder() =>
        OrderFactory.ForUser(FakeUserId);

    protected static OrderLineEntity BuildOrderLineWithZeroAmounts() =>
        new()
        {
            OrderLineID = FakeOrderLineId,
            OrderID = "order-domain-test-001",
            SellableItemID = "item-domain-test-001",
            Quantity = 1,
            Total = Money.Zero(),
            Subtotal = Money.Zero(),
            TaxAmount = Money.Zero(),
            DiscountAmount = Money.Zero()
        };

    protected static OrderLineEntity BuildOrderLineWithId(string orderLineId) =>
        new()
        {
            OrderLineID = orderLineId,
            OrderID = "order-domain-test-001",
            SellableItemID = "item-domain-test-001",
            Quantity = 1,
            Total = Money.Zero(),
            Subtotal = Money.Zero(),
            TaxAmount = Money.Zero(),
            DiscountAmount = Money.Zero()
        };
}

