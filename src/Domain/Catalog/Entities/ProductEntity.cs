using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Domain.Catalog.Entities;

/// <summary>
/// Entidad basica del catalog, define que es lo que se puede vender
/// </summary>
public class ProductEntity : BaseAuditableEntity
{
    public string ProductID { get; private set; } = string.Empty;
    public string Name { get;  set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProductType Type { get; private set; }
    public bool IsActive { get; private set; }
}
