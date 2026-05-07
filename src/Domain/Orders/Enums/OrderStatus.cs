namespace VibraHeka.Domain.Orders.Enums;

public enum OrderStatus
{
    Pending,
    OrderPayed,
    InvoicePayed,
    PaymentPending,
    OrderDelayed,
    PaymentFailed,
    Cancelled,
}
