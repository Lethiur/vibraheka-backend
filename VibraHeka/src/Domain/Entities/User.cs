namespace VibraHeka.Domain.Entities;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string CognitoId { get; set; } = string.Empty; 
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } = UserRole.User;

    public User()
    {
        
    }
    
    public User(string id, string email, string personFullName, string cognitoId = "")
    {
        Id = id;
        Email = email;
        FullName = personFullName;
        CognitoId = cognitoId;
    }
}
