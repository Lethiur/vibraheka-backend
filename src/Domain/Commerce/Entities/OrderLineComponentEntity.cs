using NMoneys;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Domain.Commerce.Entities;

public class OrderLineComponentEntity : BaseAuditableEntity
{
    public string OrderLineComponentID { get; private set; } = string.Empty;

    public string OrderLineID { get; private set; } = string.Empty;

    public string ReferenceID { get; private set; } = string.Empty;

    public string ProductNameSnapshot { get; private set; } = string.Empty;

    public ProductType ProductTypeSnapshot { get; private set; }

    public int Quantity { get; private set; }

    public Money AllocatedAmount { get; private set; }
}
