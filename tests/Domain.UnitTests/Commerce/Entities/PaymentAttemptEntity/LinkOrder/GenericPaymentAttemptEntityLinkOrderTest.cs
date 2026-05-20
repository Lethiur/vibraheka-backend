using NMoneys;
using VibraHeka.Domain.Commerce.Enums;

namespace VibraHeka.Domain.UnitTests.Commerce.Entities.PaymentAttemptEntity.LinkOrder;

public abstract class GenericPaymentAttemptEntityLinkOrderTest
{
    protected const string FakeOrderId = "order-domain-pa-test-001";
    protected const string FakeUserId = "user-domain-pa-test-001";

    protected static Domain.Commerce.Entities.OrderEntity BuildOrderEntityForLinking() =>
        new()
        {
            OrderID = FakeOrderId,
            UserID = FakeUserId,
            Status = OrderStatus.Draft,
            Total = Money.Zero(),
            Subtotal = Money.Zero(),
            TaxTotal = Money.Zero(),
            DiscountTotal = Money.Zero(),
            Lines = []
        };

    protected static Domain.Payments.Entities.PaymentAttemptEntity BuildEmptyPaymentAttemptEntity() =>
        new();
}

