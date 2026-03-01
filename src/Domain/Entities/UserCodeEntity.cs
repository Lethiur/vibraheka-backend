using VibraHeka.Domain.Common.Enums;

namespace VibraHeka.Domain.Entities;

public class UserCodeEntity : BaseAuditableEntity
{
    public string UserEmail { get; set; } = string.Empty;

    public ActionType ActionType { get; set; } = ActionType.UserVerification;
    
    public string Code { get; set; } = string.Empty;
    
}
