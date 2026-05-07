using VibraHeka.Domain.Products.Enums;

namespace VibraHeka.Domain.Products.Entities;

public record ProductEntity : BaseAuditableEntity
{
    public string ProductID { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public decimal Price { get; set; } = 0.0m;
    public decimal DiscountPrice { get; set; } = 0.0m;
    public String ExternalProductID { get; set; } = string.Empty;
    public ProductType ProductType { get; set; } = ProductType.Event;
    public ProductStatus Status { get; set; } = ProductStatus.MissingStock;
    public String ItemID { get; set; } = string.Empty;
    
    public bool CanBeDeleted() => Status != ProductStatus.Established;
    
    public bool CanBePurchased() => Status == ProductStatus.Established;
}
