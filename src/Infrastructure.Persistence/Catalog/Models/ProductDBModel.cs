using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using ProductType = VibraHeka.Domain.Catalog.Enums.ProductType;

namespace Infrastructure.Persistence.Catalog.Models;

[DynamoDBTable("Products")]
public class ProductDBModel : BaseAuditableDBModel
{
    [DynamoDBHashKey]
    public string ProductID { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Name { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Description { get; set; } = string.Empty;

    [DynamoDBProperty(typeof(EnumStringConverter<ProductType>))]
    public ProductType Type { get; set; }

    [DynamoDBProperty]
    public bool IsActive { get; set; }
}
