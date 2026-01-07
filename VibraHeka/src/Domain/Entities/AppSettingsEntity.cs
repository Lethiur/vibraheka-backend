namespace VibraHeka.Domain.Entities;

public class AppSettingsEntity
{
    public string ID { get; set; } = string.Empty;
    
    public string VerificationEmailTemplate { get; set; } = string.Empty;
    
    public string EmailForResetPassword { get; set; } = string.Empty;
    
}
