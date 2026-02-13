namespace VibraHeka.Domain.Entities;

public class SubscriptionEntity
{
    public string SubscriptionID { get; set; } = string.Empty;
    
    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.UtcNow;
    
    public DateTimeOffset EndDate { get; set; } = DateTimeOffset.UtcNow;
}
