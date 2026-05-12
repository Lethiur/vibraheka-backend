using NMoneys;
using VibraHeka.Domain.Commerce.Enums;

namespace VibraHeka.Domain.Commerce.Entities;

public class OrderEntity : BaseAuditableEntity
{
    public string OrderID { get; set; } = string.Empty;
 
    public string UserId { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public Money Subtotal { get; set; }
    public Money DiscountTotal { get; set; }
    public Money TaxTotal { get;  set; }
    public Money Total { get; set; }
    
    public DateTimeOffset PaidAt { get; set; }

    public List<OrderLineEntity> Lines { get; set; } = [];
}
