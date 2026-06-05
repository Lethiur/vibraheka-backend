using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace Infrastructure.Persistence.Catalog.Models;

[DynamoDBTable("Catalog-Products")]
public class ProductDBModel : BaseAuditableDBModel
{

    [DynamoDBIgnore]
    public string ID { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Name { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Description { get; set; } = string.Empty;

    [DynamoDBProperty]
    public bool IsActive { get; set; }
}
