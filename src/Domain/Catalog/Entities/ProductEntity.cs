namespace VibraHeka.Domain.Catalog.Entities;

/// <summary>
/// Entidad basica del catalog, define que es lo que se puede vender
/// </summary>
public class ProductEntity : BaseAuditableEntity
{
    public string ID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
