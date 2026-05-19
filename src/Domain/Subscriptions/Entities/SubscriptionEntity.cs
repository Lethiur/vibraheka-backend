using VibraHeka.Domain.Common.Enums;

namespace VibraHeka.Domain.Subscriptions.Entities;

public class SubscriptionEntity : BaseAuditableEntity
{
    public string SubscriptionID { get; private set; } = string.Empty;

    public string UserId { get; private set; } = string.Empty;
    public string SubscriptionPlanId { get; private set; } = string.Empty;

    public string StripeCustomerId { get; private set; } = default!;
    public string StripeSubscriptionId { get; private set; } = default!;

    public SubscriptionStatus Status { get; private set; }

    public DateTimeOffset CurrentPeriodStart { get; private set; }
    public DateTimeOffset CurrentPeriodEnd { get; private set; }
}
