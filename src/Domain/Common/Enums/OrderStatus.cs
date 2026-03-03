namespace VibraHeka.Domain.Common.Enums;

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
