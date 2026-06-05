using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Web.Entities;

public class ModifyProductVisibilityRequest
{
    public string ProductID { get; set; } = string.Empty;
    public ProductType ProductType  { get; set; }
}
