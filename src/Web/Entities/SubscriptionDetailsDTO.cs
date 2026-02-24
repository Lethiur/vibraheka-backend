using VibraHeka.Domain.Common.Enums;

namespace VibraHeka.Web.Entities;

public class SubscriptionDetailsDTO
{
    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset EndDate { get; set; } = DateTimeOffset.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Created;
}
