namespace Infrastructure.Rest.Client.Stripe.Models;

public class CreateProductAndPriceResponse
{
    public string ProductID { get; set; } = string.Empty;
    public string PriceID { get; set; } = string.Empty;
}
