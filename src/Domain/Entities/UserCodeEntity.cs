using VibraHeka.Domain.Common.Enums;

namespace VibraHeka.Domain.Entities;

/// <summary>
/// Represents a persisted code/token marker associated with a user action.
/// </summary>
public class UserCodeEntity : BaseAuditableEntity
{
    /// <summary>
    /// Gets or sets the email associated with the token.
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the action represented by this code.
    /// </summary>
    public ActionType ActionType { get; set; } = ActionType.UserVerification;
    
    /// <summary>
    /// Gets or sets the token identifier (unique code id).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets UNIX epoch time in seconds used by DynamoDB TTL.
    /// </summary>
    public long ExpiresAtUnix { get; set; }
}
