namespace VibraHeka.Domain.Entities;

public class VerificationCodeEntity
{

    public string UserName { get; set; } = string.Empty;
    
    public string Code { get; set; } = string.Empty;
    
    public long Timestamp { get; set; } = 0;
}
