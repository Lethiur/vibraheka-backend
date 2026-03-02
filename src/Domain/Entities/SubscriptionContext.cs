namespace VibraHeka.Domain.Entities;

/// <summary>
/// Represents the data prepared by the payment flow before persisting a pending subscription.
/// </summary>
public class SubscriptionContext
{
    /// <summary>
    /// Gets or sets the internal user identifier that owns the subscription.
    /// </summary>
    public string UserID { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the external customer identifier in Stripe.
    /// </summary>
    public string ExternalCustomerID { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the checkout session metadata returned by Stripe.
    /// </summary>
    public SubscriptionCheckoutSessionEntity CheckoutSession { get; set; } = new();
}
