namespace Infrastructure.Rest.Client.Stripe.Models;

public class CheckoutResult
{
    public string Url { get; set; } = string.Empty;

    public string PaymentSessionID { get; set; } = string.Empty;

    public string InternalPaymentID { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow;
}
