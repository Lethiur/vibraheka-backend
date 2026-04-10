namespace VibraHeka.Web.Entities;

public class SubscriptionCreationDTO
{
    public string Url { get; set; } = string.Empty;
    
    public DateTimeOffset SessionExpiresAt { get; set; } = DateTimeOffset.UtcNow;
}
