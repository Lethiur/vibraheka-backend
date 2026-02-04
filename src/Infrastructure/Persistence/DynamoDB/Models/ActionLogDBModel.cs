using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;

namespace VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

[DynamoDBTable("ActionLogs")]
public class ActionLogDBModel
{
    [DynamoDBHashKey("ActionLogID")]
    public string ID { get; set; } = string.Empty;

    [DynamoDBRangeKey(typeof(EnumStringConverter<ActionType>))]
    public ActionType Action { get; set; } = ActionType.UserVerification;
    
    [DynamoDBProperty(typeof(DateTimeOffsetConverter))]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    public static ActionLogDBModel FromDomain(ActionLogEntity actionLog) => new()
    {
        ID = actionLog.ID,
        Action = actionLog.Action,
        Timestamp = actionLog.Timestamp
    };

    public ActionLogEntity ToDomain() => new()
    {
        ID = this.ID,
        Action = this.Action,
        Timestamp = this.Timestamp
    };
}
