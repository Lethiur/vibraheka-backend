namespace VibraHeka.Domain.Entities;

public class PasswordRecoveryEntity
{
    public string Code {get; set;} = string.Empty;
    
    public string Password {get;set;} = string.Empty;
    
    public string PasswordConfirmation {get; set;}= string.Empty;
}
