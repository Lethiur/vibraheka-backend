namespace VibraHeka.Domain.Entities;

public class AppSettingsEntity
{
    private readonly object _lock = new();
    private string _verificationEmailTemplate = string.Empty;
    private string _recoverPasswordEmailTemplate = string.Empty;

    public string ID { get; set; } = string.Empty;

    public string VerificationEmailTemplate
    {
        get { lock (_lock) return _verificationEmailTemplate; }
        set { lock (_lock) _verificationEmailTemplate = value; }
    }

    public string RecoverPasswordEmailTemplate
    {
        get { lock (_lock) return _recoverPasswordEmailTemplate; }
        set { lock (_lock) _recoverPasswordEmailTemplate = value; }
    }
}
