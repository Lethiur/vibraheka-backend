using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using SellableItemType = VibraHeka.Domain.Catalog.Entities.SellableItemType;
namespace Infrastructure.Persistence.Catalog.Models;

[DynamoDBTable("Catalog-SellableItems")]
public class SellableItemDBModel : BaseAuditableDBModel
{
    [DynamoDBHashKey]
    public string SellableItemID { get; set; } = string.Empty;
    [DynamoDBProperty(typeof(EnumStringConverter<SellableItemType>))]
    public SellableItemType Type { get; set; }

    [DynamoDBGlobalSecondaryIndexHashKey("ReferenceID-Index")]
    [DynamoDBProperty]
    public string ReferenceID { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Name { get; set; } = string.Empty;
    [DynamoDBProperty]
    public bool IsActive { get; set; }
}
