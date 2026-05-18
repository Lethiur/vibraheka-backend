namespace VibraHeka.Domain.Commerce.Models;

public class CheckoutProductModel
{
    public string OrderID { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string ProductGatewayID { get; set; } = string.Empty;
    public string ProductPriceGatewayID { get; set; } = string.Empty;
    public string CustomerID { get; set; } = string.Empty;

    public string SuccessCallbackUrl { get; set; } = string.Empty;

    public string FailureCallbackUrl { get; set; } = string.Empty;
}
