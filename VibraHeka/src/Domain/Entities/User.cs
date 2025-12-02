namespace VibraHeka.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string CognitoId { get; set; } = string.Empty; // Sub de Cognito
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
