namespace VibraHeka.Domain.Catalog.Entities;

public class SubscriptionPlanEntity : BaseAuditableEntity
{
    public string SubscriptionPlanID { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IncludesFullCatalog { get; private set; }
    public bool IsActive { get; private set; }
}
