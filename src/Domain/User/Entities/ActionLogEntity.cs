using VibraHeka.Domain.Common.Enums;

namespace VibraHeka.Domain.Entities;

public class ActionLogEntity
{
    public string ID { get; set; } = string.Empty;

    public ActionType Action { get; set; } = ActionType.UserVerification;
    
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
