namespace VibraHeka.Domain.Entities;

public class UserEntity : BaseAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string CognitoId { get; set; } = string.Empty; 
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    
    public string ProfilePictureUrl { get; set; } = string.Empty;
    
    public string Bio { get; set; } = string.Empty;
    
    public string MiddleName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string PhoneNumber { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } = UserRole.User;

    public UserEntity()
    {
        
    }
    
    public UserEntity(string id, string email, string personFirstName, string cognitoId = "")
    {
        Id = id;
        Email = email;
        FirstName = personFirstName;
        CognitoId = cognitoId;
    }
}
