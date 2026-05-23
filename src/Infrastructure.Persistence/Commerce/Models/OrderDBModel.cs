using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Converters;
using NMoneys;
using VibraHeka.Domain.Commerce.Enums;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace Infrastructure.Persistence.Commerce.Models;


[DynamoDBTable("Commerce-Orders")]
public class OrderDBModel : BaseAuditableDBModel
{
    [DynamoDBHashKey] public string OrderID { get; set; } = string.Empty;

    [DynamoDBGlobalSecondaryIndexHashKey("User-Index")]
    public string UserID { get; set; } = string.Empty;

    [DynamoDBProperty(typeof(EnumStringConverter<OrderStatus>))]
    public OrderStatus Status { get; set; }

    [DynamoDBProperty(typeof(MoneyConverter))]
    public Money Subtotal { get; set; }

    [DynamoDBProperty(typeof(MoneyConverter))]
    public Money DiscountTotal { get; set; }

    [DynamoDBProperty(typeof(MoneyConverter))]
    public Money TaxTotal { get; set; }

    [DynamoDBProperty(typeof(MoneyConverter))]
    public Money Total { get; set; }

    [DynamoDBProperty(typeof(DateTimeOffsetConverter))]
    public DateTimeOffset PaidAt { get; set; }
}
