namespace VibraHeka.Domain.Catalog.Entities;

public class SubscriptionPlan : ProductEntity
{
    public string SubscriptionPlanID { get => ID; set => ID = value; }
    public bool IncludesFullCatalog { get; set; }
}
