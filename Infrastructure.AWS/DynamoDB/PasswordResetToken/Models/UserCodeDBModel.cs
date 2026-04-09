using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;

namespace VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

/// <summary>
/// DynamoDB model used to persist user token/code markers.
/// </summary>
[DynamoDBTable("CODES-TABLE")]
public class UserCodeDBModel : BaseAuditableDBModel
{
    /// <summary>
    /// Gets or sets the unique token identifier.
    /// </summary>
    [DynamoDBHashKey]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user email associated to this token.
    /// </summary>
    [DynamoDBProperty]
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the action represented by this marker.
    /// </summary>
    [DynamoDBProperty(typeof(EnumStringConverter<ActionType>))]
    public ActionType ActionType { get; set; } = ActionType.UserVerification;

    /// <summary>
    /// Gets or sets UNIX epoch time in seconds used by DynamoDB TTL.
    /// </summary>
    [DynamoDBProperty]
    public long ExpiresAtUnix { get; set; }
}
