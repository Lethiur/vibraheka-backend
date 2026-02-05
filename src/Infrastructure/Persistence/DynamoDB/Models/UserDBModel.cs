using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;

namespace VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

[DynamoDBTable("TABLE_USERS")]
public class UserDBModel : BaseAuditableDBModel
{
    
    [DynamoDBHashKey]
    public string Id { get; set; } = string.Empty;
    [DynamoDBProperty]
    public string CognitoId { get; set; } = string.Empty; // Sub de Cognito
    [DynamoDBGlobalSecondaryIndexHashKey("EmailIndex")]
    public string Email { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string FirstName { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string MiddleName { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string LastName { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string Bio { get; set; } = string.Empty;
    
    public string ProfilePictureUrl { get; set; } = string.Empty;
    

    
    [DynamoDBProperty(typeof(EnumStringConverter<UserRole>))]
    [DynamoDBGlobalSecondaryIndexHashKey("Role-Index")]
    public UserRole Role { get; set; } = UserRole.User;
    
    public static UserDBModel FromDomain(UserEntity userEntity) => new()
    {
        Id = userEntity.Id,
        Email = userEntity.Email,
        Role = userEntity.Role,
        CognitoId = userEntity.CognitoId,
        FirstName = userEntity.FirstName,
        MiddleName = userEntity.MiddleName,
        ProfilePictureUrl = userEntity.ProfilePictureUrl,
        LastName = userEntity.LastName,
        PhoneNumber = userEntity.PhoneNumber,
        Bio = userEntity.Bio,
        Created = userEntity.Created,
        CreatedBy = userEntity.CreatedBy,
        LastModified = userEntity.LastModified,
        LastModifiedBy = userEntity.LastModifiedBy
    };

    public UserEntity ToDomain() => new()
    {
        Id = this.Id,
        Email = this.Email,
        Role = this.Role,
        ProfilePictureUrl = this.ProfilePictureUrl,
        CognitoId = this.CognitoId,
        FirstName = this.FirstName,
        MiddleName = this.MiddleName,
        LastName = this.LastName,
        PhoneNumber = this.PhoneNumber,
        Bio = this.Bio,
        Created = this.Created,
        CreatedBy = this.CreatedBy,
        LastModified = this.LastModified,
        LastModifiedBy = this.LastModifiedBy
    };
}
