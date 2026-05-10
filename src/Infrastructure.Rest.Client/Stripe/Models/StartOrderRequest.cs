using Infrastructure.Rest.Client.Stripe.Enums;

namespace Infrastructure.Rest.Client.Stripe.Models;

public class StartOrderRequest
{
    public string ProductRef { get; set; } = string.Empty;
    public string OrderID { get; set; } = string.Empty;
    public string CustomerID { get; set; } = string.Empty;
    public List<string> PaymentMethodsAccepted { get; set; } = [];
    public decimal? Price { get; set; }
    public string SuccessCallbackUrl { get; set; } = string.Empty;
    public string FailureCallbackUrl { get; set; } = string.Empty;
    public PaymentMethodCollection PaymentMethodCollection { get; set; }
    public int OrderQuantity { get; set; }
}
