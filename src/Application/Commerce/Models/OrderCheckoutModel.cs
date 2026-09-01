namespace VibraHeka.Application.Commerce.Models;

public class OrderCheckoutModel
{
    public string CheckoutURL { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAtUTC { get; set; }
}
