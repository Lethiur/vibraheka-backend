using NMoneys;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Domain.Commerce.Entities;

public class OrderLineComponentEntity : BaseAuditableEntity
{
    public string OrderLineComponentID { get; set; } = string.Empty;

    public string OrderLineID { get; set; } = string.Empty;

    public string ReferenceID { get; set; } = string.Empty;

    public string ProductNameSnapshot { get; set; } = string.Empty;

    public ProductType ProductTypeSnapshot { get; set; }

    public int Quantity { get; set; }

    public Money AllocatedAmount { get; set; }
}
