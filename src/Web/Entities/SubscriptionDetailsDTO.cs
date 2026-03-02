using VibraHeka.Domain.Common.Enums;

namespace VibraHeka.Web.Entities;

public class SubscriptionDetailsDTO
{
    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset EndDate { get; set; } = DateTimeOffset.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Created;
    
    public string CheckoutSessionUrl { get; set; } = string.Empty;

    public DateTimeOffset CheckoutSessionExpiresAt { get; set; } = DateTimeOffset.UtcNow;
}
