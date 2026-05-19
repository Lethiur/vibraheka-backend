using VibraHeka.Domain.Access.Enums;

namespace VibraHeka.Domain.Access.Entities;

public class EntitlementEntity : BaseAuditableEntity
{
    public string EntitlementID { get; private set; } = string.Empty;

    public string UserID { get; private set; } = string.Empty;

    public string ProductID { get; private set; } = string.Empty;

    public EntitlementSourceType SourceType { get; private set; }
    public string SourceID { get; private set; } = string.Empty;

    public DateTimeOffset GrantedAt { get; private set; }
    public DateTimeOffset? ValidUntil { get; private set; }
}
