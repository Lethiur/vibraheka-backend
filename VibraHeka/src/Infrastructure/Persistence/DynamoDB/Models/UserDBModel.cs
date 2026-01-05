using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;

namespace VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

[DynamoDBTable("TABLE_USERS")]
public class UserDBModel
{
    
    [DynamoDBHashKey]
    public string Id { get; set; } = string.Empty;
    [DynamoDBProperty]
    public string CognitoId { get; set; } = string.Empty; // Sub de Cognito
    [DynamoDBProperty]
    public string Email { get; set; } = string.Empty;
    [DynamoDBProperty]
    public string FullName { get; set; } = string.Empty;
    
    [DynamoDBProperty(typeof(EnumStringConverter<UserRole>))]
    [DynamoDBGlobalSecondaryIndexHashKey("Role-Index")]
    public UserRole Role { get; set; } = UserRole.User;
    
    public static UserDBModel FromDomain(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Role = user.Role,
        CognitoId = user.CognitoId,
        FullName = user.FullName
        
    };

    public User ToDomain() => new()
    {
        Id = this.Id,
        Email = this.Email,
        Role = this.Role,
        CognitoId = this.CognitoId,
        FullName = this.FullName
    };
}
