namespace VibraHeka.Domain.Commerce.Enums;

public enum OrderStatus
{
    Draft,
    PendingPayment,
    Paid,
    PartiallyRefunded,
    Refunded,
    Cancelled,
    Failed
}
