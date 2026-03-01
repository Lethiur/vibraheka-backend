namespace VibraHeka.Domain.Entities;

public class SubscriptionCheckoutSessionEntity
{
    public string Url { get; set; } = string.Empty;

    public string PaymentSessionID { get; set; } = string.Empty;
    
    public string InternalPaymentID { get; set; } = string.Empty;
    
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow;
}
