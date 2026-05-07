using VibraHeka.Domain.Orders.Enums;

namespace VibraHeka.Domain.Orders.Entities;

public record OrderEntity : BaseAuditableEntity
{
    public required String OrderID { get; set; } = string.Empty;
    public required String ExternalOrderID { get; set; } = string.Empty;
    public required String CustomerID { get; set; } = string.Empty;
    public required String PaymentGatewayUrl { get; set; } = string.Empty;
    public required String ProductID { get; set; } = string.Empty;
    public required OrderType OrderType { get; set; } = OrderType.Event;
    public required OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
}
