using VibraHeka.Domain.Common.Enums;

namespace VibraHeka.Domain.Entities;

public class OrderEntity : BaseAuditableEntity
{
    public string OrderID { get; set; } = string.Empty;
    
    public string UserID { get; set; } = string.Empty;
    
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    public string ExternalReference { get; set; } = string.Empty;

    public OrderType OrderType { get; set; } = OrderType.OneOff;
    
    public string ExternalCustomerId { get; set; } = string.Empty;
    
    public string ExternalPaymentSessionId { get; set; } = string.Empty;
    
    public string SubscriptionID { get; set; } = string.Empty;
    
    public string ItemID { get; set; } = string.Empty;
    
    public int Quantity { get; set; } = 1;
    
    public string InvoiceUrl { get; set; } = string.Empty;
    
}
