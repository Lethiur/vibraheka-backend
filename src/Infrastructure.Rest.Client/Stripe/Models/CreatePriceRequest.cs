using Infrastructure.Rest.Client.Stripe.Enums;

namespace Infrastructure.Rest.Client.Stripe.Models;

public class CreatePriceRequest
{
    public string ProductID { get; set; } = string.Empty;
    public long PriceInCents { get; set; }
    public string Currency { get; set; } = "eur";
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    public PaymentRecurringOptions? PaymentRecurringOptions { get; set; }
}
