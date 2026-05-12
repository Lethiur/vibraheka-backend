namespace VibraHeka.Domain.Catalog.Entities;

public class SellableItemEntity : BaseAuditableEntity
{
    public string SellableItemID { get; private set; } = string.Empty;

    public SellableItemType Type { get; private set; }

    public Guid ReferenceId { get; private set; }
    // ProductId, BundleId o SubscriptionPlanId

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public List<SellableItemPriceEntity> Prices { get; private set; } = [];
}
