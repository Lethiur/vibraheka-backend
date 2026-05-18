using NMoneys;
using VibraHeka.Domain.Payments.Enums;

namespace VibraHeka.Domain.Payments.Entities;

public class PaymentAttemptEntity : BaseAuditableEntity
{
    public string PaymentAttemptID { get; set; } = string.Empty;

    public string OrderId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;

    public PaymentsProviders Provider { get; set; }
    public PaymentsStatus Status { get; set; }

    public Money Amount { get; set; }

    public string PaymentGatewayCheckoutSessionID { get; set; } = string.Empty;
    public string PaymentGatewayCheckoutURL { get; set; } = string.Empty;
    public string PaymentGatewayIntentID { get; set; } = string.Empty;
    public string PaymentGatewayInvoiceID { get; set; } = string.Empty;
    public string PaymentGatewaySubscriptionID { get; set; } = string.Empty;

    public bool IsExpired { get; private set; }
    public DateTimeOffset SucceededAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; } = DateTimeOffset.UtcNow.AddHours(23);
}
