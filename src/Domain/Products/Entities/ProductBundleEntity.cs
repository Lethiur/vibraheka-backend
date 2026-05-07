using Bnaya.CodeGeneration.BuilderPatternGeneration;

namespace VibraHeka.Domain.Products.Entities;
[GenerateBuilderPattern]
public partial class ProductBundleEntity : ProductEntity
{
    public List<String> ProductIDs = [];
}
