using NMoneys;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Domain.Catalog.Entities;

public class SellableItemPriceEntity : BaseAuditableEntity
{
    public string SellableItemPriceID { get; set; } = string.Empty;

    public string SellableItemID { get; set; } = string.Empty;

    public Money Amount { get; set; }

    public PriceKind Kind { get; set; }
    // OneTime o Recurring

    public BillingInterval? BillingInterval { get; set; }
    // Month, Year. Solo para suscripciones.

    public string ExternalProductID { get; set; } = string.Empty;
    public string ExternalPriceID { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
