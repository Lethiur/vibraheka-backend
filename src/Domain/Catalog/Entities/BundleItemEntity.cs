namespace VibraHeka.Domain.Catalog.Entities;

public class BundleItemEntity : BaseAuditableEntity
{
    public string BundleItemID { get; private set; } = string.Empty;
    public string BundleID { get; private set; } = string.Empty;
    public string ProductId { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
}
