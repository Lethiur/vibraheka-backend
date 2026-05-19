using Infrastructure.Rest.Client.Stripe.Enums;

namespace Infrastructure.Rest.Client.Stripe.Models;

public class StartOrderRequest
{
    public List<OrderLine> OrderLines { get; set; } = [];
    public string OrderID { get; set; } = string.Empty;
    public string CustomerID { get; set; } = string.Empty;
    public List<string> PaymentMethodsAccepted { get; set; } = [];
    public string SuccessCallbackUrl { get; set; } = string.Empty;
    public string FailureCallbackUrl { get; set; } = string.Empty;
    
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    
}
