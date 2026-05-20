using NMoneys;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Domain.Commerce.Entities;

public class OrderLineEntity : BaseAuditableEntity
{
    public string OrderLineID { get; set; } = string.Empty;
    public string OrderID { get; set; } = string.Empty;

    public SellableItemType Type { get; set; }

    public string SellableItemID { get; set; } = string.Empty;
    public string SellablePriceID { get; set; } = string.Empty;

    public string NameSnapshot { get; set; } = string.Empty;

    public string PaymentGatewayPriceIDSnapshot { get; set; } = string.Empty!;
    public string PaymentGatewayProductIDSnapshot { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public Money UnitPrice { get; set; }
    public Money Subtotal { get; set; }
    public Money DiscountAmount { get; set; }
    public Money TaxAmount { get; set; }
    public Money Total { get; set; }

    public List<OrderLineComponentEntity> Components { get; set; } = [];
}
