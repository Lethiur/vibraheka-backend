using VibraHeka.Domain.Commerce.Enums;
using VibraHeka.Domain.Common.Enums;

namespace VibraHeka.Domain.Entities;

public class SubscriptionEntity : BaseAuditableEntity
{
    public string SubscriptionID { get; set; } = string.Empty;

    public string UserID { get; set; } = string.Empty;

    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset EndDate { get; set; } = DateTimeOffset.UtcNow;

    public string ExternalSubscriptionItemID { get; set; } = string.Empty;

    public string ExternalSubscriptionID { get; set; } = string.Empty;

    public string ExternalCustomerID { get; set; } = string.Empty;

    public string CheckoutSessionUrl { get; set; } = string.Empty;

    public DateTimeOffset CheckoutSessionExpiresAt { get; set; } = DateTimeOffset.UtcNow;

    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Created;

    public bool IsActive()
    {
        return SubscriptionStatus is SubscriptionStatus.Active or SubscriptionStatus.Trialing or SubscriptionStatus.ToBeCancelled;
    }
}
