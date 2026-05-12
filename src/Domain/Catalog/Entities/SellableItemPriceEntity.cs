using NMoneys;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Domain.Catalog.Entities;

public class SellableItemPriceEntity : BaseAuditableEntity
{
    public string SellableItemPriceID { get; private set; } = string.Empty;

    public string SellableItemID { get; private set; } = string.Empty;

    public Money Amount { get; private set; }

    public PriceKind Kind { get; private set; }
    // OneTime o Recurring

    public BillingInterval? BillingInterval { get; private set; }
    // Month, Year. Solo para suscripciones.

    public string ExternalProductID { get; private set; } = string.Empty;
    public string ExternalPriceID { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
