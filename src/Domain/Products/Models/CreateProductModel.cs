using Bnaya.CodeGeneration.BuilderPatternGeneration;

namespace VibraHeka.Domain.Products.Models;

[GenerateBuilderPattern]
public partial class CreateProductModel
{
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public decimal Price { get; set; } = 0.0m;
    public decimal DiscountPrice { get; set; } = 0.0m;
    
}
