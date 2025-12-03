using Amazon.DynamoDBv2.DataModel;

namespace VibraHeka.Domain.Entities;


[DynamoDBTable("TABLE_USERS")]
public class User
{
    [DynamoDBHashKey]
    public string Id { get; set; } = string.Empty;
    public string CognitoId { get; set; } = string.Empty; // Sub de Cognito
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
