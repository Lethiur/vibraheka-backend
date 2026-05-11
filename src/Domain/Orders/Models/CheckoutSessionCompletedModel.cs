namespace VibraHeka.Domain.Orders.Models;

public class CheckoutSessionCompletedModel
{
    public string PaymentIntentID { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
}
