namespace VibraHeka.Domain.Commerce.Models;

public class CheckoutSessionCompletedModel
{
    public string CheckoutUrl { get; set; } = string.Empty;
    public string PaymentIntentID { get; set; } = string.Empty;

}
