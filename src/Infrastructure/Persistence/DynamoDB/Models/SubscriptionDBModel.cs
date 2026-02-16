using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;

namespace VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

[DynamoDBTable("TABLE_SUBSCRIPTIONS")]
public class SubscriptionDBModel : BaseAuditableDBModel
{
    [DynamoDBHashKey]
    public string SubscriptionID { get; set; } = string.Empty;
    
    [DynamoDBGlobalSecondaryIndexHashKey("User-Index")]
    public string UserID { get; set; } = string.Empty;
    
    [DynamoDBProperty(typeof(DateTimeOffsetConverter))]
    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.UtcNow;
    
    [DynamoDBProperty(typeof(DateTimeOffsetConverter))]
    public DateTimeOffset EndDate { get; set; } = DateTimeOffset.UtcNow;
    
    [DynamoDBProperty]
    public string ExternalSubscriptionID { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string ExternalSubscriptionItemID { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string ExternalCustomerID { get; set; } = string.Empty;
    
    [DynamoDBProperty(typeof(EnumStringConverter<OrderType>))]
    public OrderType OrderType { get; set; } = OrderType.Subscription;
    
    [DynamoDBProperty(typeof(EnumStringConverter<OrderStatus>))]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    [DynamoDBProperty(typeof(EnumStringConverter<SubscriptionStatus>))]
    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Created;
}

