using Bnaya.CodeGeneration.BuilderPatternGeneration;
using VibraHeka.Domain.Products.Enums;

namespace VibraHeka.Domain.Products.Models;

[GenerateBuilderPattern]
public partial class UpdateProductStatusModel
{
    public string ProductID { get; set; } = string.Empty;
    public ProductStatus Status { get; set; } = ProductStatus.MissingStock;
}
