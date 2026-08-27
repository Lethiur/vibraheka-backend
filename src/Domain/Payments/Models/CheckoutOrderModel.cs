using VibraHeka.Domain.Commerce.Entities;

namespace VibraHeka.Domain.Payments.Models;

public class CheckoutOrderModel
{
    public OrderEntity Order { get; set; } = null!;
    public string CustomerID { get; set; } = string.Empty;
    public string SuccessCallbackUrl { get; set; } = string.Empty;
    public string CancelCallbackUrl { get; set; } = string.Empty;

    public List<string> PaymentMethodsAccepted { get; set; } = [];
}
