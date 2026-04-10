using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Subscriptions.Entities;

namespace VibraHeka.Domain.Subscriptions.Factories;

public class SubscriptionEntityFactory
{
    public static SubscriptionEntity FromCheckoutSession(SubscriptionCheckoutSessionEntity checkoutSession)
    {
        return new SubscriptionEntity()
        {
            CheckoutSessionUrl = checkoutSession.Url,
            CheckoutSessionExpiresAt = checkoutSession.SessionExpiresAt,
            SubscriptionStatus = SubscriptionStatus.Created,
            Status = OrderStatus.Pending,
            StartDate = DateTime.UtcNow,
        };
    }
}
