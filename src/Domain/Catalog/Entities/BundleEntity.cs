namespace VibraHeka.Domain.Catalog.Entities;

public class BundleEntity : BaseAuditableEntity
{
    public string BundleID { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public List<BundleItemEntity> Items { get; private set; } = [];
}
