namespace VibraHeka.Web.Entities;

public class SubscriptionCreationDTO
{
    public string Url { get; set; } = string.Empty;
    
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow;
}
