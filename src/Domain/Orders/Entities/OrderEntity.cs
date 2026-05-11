using VibraHeka.Domain.Orders.Enums;

namespace VibraHeka.Domain.Orders.Entities;

public class OrderEntity : BaseAuditableEntity
{
    public String OrderID { get; set; } = string.Empty;
    public String ExternalOrderID { get; set; } = string.Empty;
    public String CustomerID { get; set; } = string.Empty;
    public String PaymentGatewayUrl { get; set; } = string.Empty;
    public String ProductID { get; set; } = string.Empty;
    public String UserID { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public OrderType OrderType { get; set; } = OrderType.Event;
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Created;
}
