using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;

namespace VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

[DynamoDBTable("CODES-TABLE")]
public class UserCodeDBModel : BaseAuditableDBModel
{
    [DynamoDBHashKey]
    public string UserEmail { get; set; } = string.Empty;

    [DynamoDBProperty(typeof(EnumStringConverter<ActionType>))]
    public ActionType ActionType { get; set; } = ActionType.UserVerification;
    
    [DynamoDBProperty]
    public string Code { get; set; } = string.Empty;
}
