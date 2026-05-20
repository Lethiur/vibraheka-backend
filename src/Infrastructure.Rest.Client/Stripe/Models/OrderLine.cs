namespace Infrastructure.Rest.Client.Stripe.Models;

public class OrderLine
{
    public string PriceRef { get; set; } = string.Empty;
    public int Quantity { get; set; }

    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
