using Infrastructure.Rest.Client.Stripe.Enums;

namespace Infrastructure.Rest.Client.Stripe.Models;

public class CreateProductAndPriceRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long PriceInCents { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    public string Currency { get; set; } = "eur";
    public PaymentRecurringOptions? PaymentRecurringOptions { get; set; }
}
