using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Converters;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
namespace Infrastructure.Persistence.Catalog.Models;

[DynamoDBTable("Catalog-SellableItemPrices")]
public class SellableItemPriceDBModel : BaseAuditableDBModel
{
    [DynamoDBHashKey]
    public string SellableItemPriceID { get; set; } = string.Empty;
    [DynamoDBGlobalSecondaryIndexHashKey("SellableItemID-Index")]
    public string SellableItemID { get; set; } = string.Empty;
    [DynamoDBProperty(typeof(MoneyConverter))]
    public NMoneys.Money Amount { get; set; }
    [DynamoDBProperty(typeof(EnumStringConverter<PriceKind>))]
    public PriceKind Kind { get; set; }

    [DynamoDBProperty(typeof(EnumStringConverter<BillingInterval>))]
    public BillingInterval? BillingIntervalValue { get; set; }
    [DynamoDBProperty]
    public string ExternalProductID { get; set; } = string.Empty;
    [DynamoDBProperty]
    public string ExternalPriceID { get; set; } = string.Empty;
    [DynamoDBProperty]
    public bool IsActive { get; set; }
}
