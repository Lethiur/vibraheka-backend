using VibraHeka.Domain.Orders.Enums;

namespace VibraHeka.Domain.Orders.Models;

public class CheckoutProductModel
{
    public string OrderID { get; set; } = string.Empty;
    public string ProductRef  { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string CustomerID { get; set; } = string.Empty;
    public string SuccessCallbackUrl { get; set; } = string.Empty;
    public string FailureCallbackUrl { get; set; } = string.Empty;
    
}
