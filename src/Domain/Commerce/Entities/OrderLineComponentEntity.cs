using NMoneys;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Domain.Commerce.Entities;

public class OrderLineComponentEntity : BaseAuditableEntity
{
    public Guid OrderLineComponentID { get; private set; }

    public Guid OrderLineID { get; private set; }

    public Guid ProductID { get; private set; }

    public string ProductNameSnapshot { get; private set; } = default!;
    public ProductType ProductTypeSnapshot { get; private set; }

    public int Quantity { get; private set; }

    public Money AllocatedAmount { get; private set; }
}
