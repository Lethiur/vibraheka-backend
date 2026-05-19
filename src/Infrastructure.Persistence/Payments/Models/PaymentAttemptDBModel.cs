using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Converters;
using NMoneys;
using VibraHeka.Domain.Payments.Enums;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace Infrastructure.Persistence.Payments.Models;

[DynamoDBTable("PaymentAttempt")]
public class PaymentAttemptDBModel : BaseAuditableDBModel
{
    [DynamoDBHashKey]
    public string PaymentAttemptID { get; set; } = string.Empty;

    [DynamoDBGlobalSecondaryIndexHashKey("OrderID-Index")]
    [DynamoDBProperty]
    public string OrderId { get; set; } = string.Empty;
    
    [DynamoDBGlobalSecondaryIndexHashKey("UserID-Index")]
    [DynamoDBProperty]
    public string UserId { get; set; } = string.Empty;

    [DynamoDBProperty(typeof(EnumStringConverter<PaymentsProviders>))]
    public PaymentsProviders Provider { get; set; }
    
    [DynamoDBProperty(typeof(EnumStringConverter<PaymentsStatus>))]
    public PaymentsStatus Status { get; set; }

    [DynamoDBProperty(typeof(MoneyConverter))]
    public Money Amount { get; set; }

    [DynamoDBProperty]
    public string PaymentGatewayCheckoutSessionID { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string PaymentGatewayCheckoutURL { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string PaymentGatewayIntentID { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string PaymentGatewayInvoiceID { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string PaymentGatewaySubscriptionID { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public bool IsExpired { get; private set; }
    
    [DynamoDBProperty(typeof(DateTimeOffsetConverter))]
    public DateTimeOffset SucceededAt { get; set; }
    
    [DynamoDBProperty(typeof(DateTimeOffsetConverter))]
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddHours(23);
}
