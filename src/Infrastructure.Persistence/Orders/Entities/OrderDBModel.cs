using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Domain.Orders.Enums;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace Infrastructure.Persistence.Orders.Entities;

[DynamoDBTable("Orders")]
public class OrderDBModel : BaseAuditableDBModel
{
    [DynamoDBHashKey]
    public String OrderID { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public String ExternalOrderID { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public String PaymentGatewayUrl { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public String ProductID { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public String UserID { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public int Quantity { get; set; } = 1;

    [DynamoDBProperty(typeof(EnumStringConverter<OrderStatus>))]
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Created;
}
