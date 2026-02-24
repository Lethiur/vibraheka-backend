using Amazon.DynamoDBv2.DataModel;

namespace VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

[DynamoDBTable("TABLE_VERIFICATION_CODES")]
public class VerificationCodeDBModel
{
    [DynamoDBHashKey("username")] public string UserName { get; set; } = string.Empty;

    [DynamoDBProperty("verification_code")]
    public string Code { get; set; } = string.Empty;

    [DynamoDBProperty("timestamp")] public long Timestamp { get; set; } = 0;
}
