using NMoneys;

namespace VibraHeka.Domain.Subscriptions.Entities;

public class SubscriptionBillingRecordEntity : BaseAuditableEntity
{
    public string SubscriptionBillingRecordID { get; private set; } = string.Empty; 

    public string SubscriptionID { get; private set; } = string.Empty;
    public string UserID { get; private set; } = string.Empty;

    public string InvoiceID { get; private set; } = string.Empty;
    public string PaymentIntentID { get; private set; } = string.Empty;

    public Money AmountPaid { get; private set; }

    public DateTimeOffset PeriodStart { get; private set; }
    public DateTimeOffset PeriodEnd { get; private set; }

    public DateTimeOffset PaidAt { get; private set; }
}
