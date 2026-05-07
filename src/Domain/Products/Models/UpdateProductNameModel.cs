using Bnaya.CodeGeneration.BuilderPatternGeneration;

namespace VibraHeka.Domain.Products.Models;

[GenerateBuilderPattern]
public partial class UpdateProductNameModel
{
    public String ProductID { get; set; } = string.Empty;
    public String ProductName { get; set; } = string.Empty;
    public String ExternalProductID { get; set; } = string.Empty;
}
