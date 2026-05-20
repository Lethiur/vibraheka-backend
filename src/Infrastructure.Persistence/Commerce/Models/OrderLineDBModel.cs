using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Converters;
using NMoneys;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace Infrastructure.Persistence.Commerce.Models;

[DynamoDBTable("OrderLines")]
public class OrderLineDBModel : BaseAuditableDBModel
{
    [DynamoDBHashKey]
    public string OrderLineID { get; set; } = string.Empty;

    [DynamoDBGlobalSecondaryIndexHashKey("OrderID-Index")]
    [DynamoDBProperty]
    public string OrderID { get; set; } = string.Empty;

    [DynamoDBProperty(typeof(EnumStringConverter<SellableItemType>))]
    public SellableItemType Type { get; set; }

    [DynamoDBProperty]
    public string SellableItemID { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string SellablePriceID { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string NameSnapshot { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string PaymentGatewayPriceIDSnapshot { get; set; } = string.Empty!;

    [DynamoDBProperty]
    public string PaymentGatewayProductIDSnapshot { get; set; } = string.Empty;

    [DynamoDBProperty]
    public int Quantity { get; set; }

    [DynamoDBProperty(typeof(MoneyConverter))]
    public Money UnitPrice { get; set; }

    [DynamoDBProperty(typeof(MoneyConverter))]
    public Money Subtotal { get; set; }

    [DynamoDBProperty(typeof(MoneyConverter))]
    public Money DiscountAmount { get; set; }

    [DynamoDBProperty(typeof(MoneyConverter))]
    public Money TaxAmount { get; set; }

    [DynamoDBProperty(typeof(MoneyConverter))]
    public Money Total { get; set; }
}
