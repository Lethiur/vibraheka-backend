namespace VibraHeka.Domain.Payments.Enums;

public enum PaymentsStatus
{
    Created,
    Pending,
    Succeeded,
    Failed,
    Cancelled,
    Refunded,
    PartiallyRefunded
}
