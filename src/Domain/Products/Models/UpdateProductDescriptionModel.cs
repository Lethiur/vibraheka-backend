
namespace VibraHeka.Domain.Products.Models;

public class UpdateProductDescriptionModel
{
    public String ProductID { get; set; } = string.Empty;
    public String ProductDescription { get; set; } = string.Empty;
    public String ExternalProductID { get; set; } = string.Empty;
}
