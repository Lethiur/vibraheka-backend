using Bnaya.CodeGeneration.BuilderPatternGeneration;

namespace VibraHeka.Domain.Products.Models;

[GenerateBuilderPattern]
public partial class UpdateProductDescriptionModel
{
    public String ProductID { get; set; } = string.Empty;
    public String ProductDescription { get; set; } = string.Empty;
    public String ExternalProductID { get; set; } = string.Empty;
}
