using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Converters;

namespace VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

[DynamoDBTable("TABLE_USERS")]
public class UserProfileDBModel : BaseAuditableDBModel
{
    
    [DynamoDBHashKey]
    public string Id { get; set; } = string.Empty;
    [DynamoDBProperty]
    public string CustomerID { get; set; } = string.Empty; // Sub de Cognito
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
    
    [DynamoDBProperty]
    public string TimezoneID { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string ProfilePictureUrl { get; set; } = string.Empty;
    

    
    [DynamoDBProperty(typeof(EnumStringConverter<UserRole>))]
    [DynamoDBGlobalSecondaryIndexHashKey("Role-Index")]
    public UserRole Role { get; set; } = UserRole.User;
    
    public static UserProfileDBModel FromDomain(UserProfileEntity userProfileEntity) => new()
    {
        Id = userProfileEntity.Id,
        Email = userProfileEntity.Email,
        Role = userProfileEntity.Role,
        CustomerID = userProfileEntity.CustomerID,
        FirstName = userProfileEntity.FirstName,
        MiddleName = userProfileEntity.MiddleName,
        ProfilePictureUrl = userProfileEntity.ProfilePictureUrl,
        LastName = userProfileEntity.LastName,
        PhoneNumber = userProfileEntity.PhoneNumber,
        Bio = userProfileEntity.Bio,
        TimezoneID = userProfileEntity.TimezoneID,
        Created = userProfileEntity.Created,
        CreatedBy = userProfileEntity.CreatedBy,
        LastModified = userProfileEntity.LastModified,
        LastModifiedBy = userProfileEntity.LastModifiedBy
    };

    public UserProfileEntity ToDomain() => new()
    {
        Id = this.Id,
        Email = this.Email,
        Role = this.Role,
        ProfilePictureUrl = this.ProfilePictureUrl,
        CustomerID = this.CustomerID,
        FirstName = this.FirstName,
        MiddleName = this.MiddleName,
        LastName = this.LastName,
        PhoneNumber = this.PhoneNumber,
        Bio = this.Bio,
        TimezoneID = this.TimezoneID,
        Created = this.Created,
        CreatedBy = this.CreatedBy,
        LastModified = this.LastModified,
        LastModifiedBy = this.LastModifiedBy
    };
}
