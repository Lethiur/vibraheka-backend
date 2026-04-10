namespace VibraHeka.Domain.Subscriptions.Entities;

public class SubscriptionCheckoutSessionEntity
{
    public string Url { get; set; } = string.Empty;

    public string CheckoutSessionID { get; set; } = string.Empty;
    
    public string InternalReferenceID { get; set; } = string.Empty;
    
    public string ItemID { get; set; } = string.Empty;
    
    public DateTimeOffset SessionExpiresAt { get; set; } = DateTimeOffset.UtcNow;
    
    public static SubscriptionCheckoutSessionEntity Create(string url, string checkoutSessionID, string internalPaymentID, DateTimeOffset expiresAt, string itemID)
    {
        return new SubscriptionCheckoutSessionEntity
        {
            Url = url,
            CheckoutSessionID = checkoutSessionID,
            InternalReferenceID = internalPaymentID,
            SessionExpiresAt = expiresAt,
            ItemID = itemID
        };
    }
}
