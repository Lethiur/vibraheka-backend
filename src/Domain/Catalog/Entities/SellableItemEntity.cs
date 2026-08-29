using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Domain.Catalog.Entities;

public class SellableItemEntity : BaseAuditableEntity
{
    public string SellableItemID { get; set; } = string.Empty;

    public SellableItemType Type { get; set; }

    public string ReferenceID { get; set; } = string.Empty;
    // ProductId, BundleId o SubscriptionPlanId

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string ExternalProductID { get; set; } = string.Empty;

    public List<SellableItemPriceEntity> Prices { get; set; } = [];
}
