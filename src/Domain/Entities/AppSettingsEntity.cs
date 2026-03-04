namespace VibraHeka.Domain.Entities;

public class AppSettingsEntity
{
    private readonly object _lock = new();
    private string _verificationEmailTemplate = string.Empty;
    private string _recoverPasswordEmailTemplate = string.Empty;
    private string _userWelcomeEmailTemplate = string.Empty;
    private string _subscriptionThankYouEmailTemplate = string.Empty;
    private string _trialEndingSoonEmailTemplate = string.Empty;
    private string _passwordChangedEmailTemplate = string.Empty;

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

    public string UserWelcomeEmailTemplate
    {
        get { lock (_lock) return _userWelcomeEmailTemplate; }
        set { lock (_lock) _userWelcomeEmailTemplate = value; }
    }

    public string SubscriptionThankYouEmailTemplate
    {
        get { lock (_lock) return _subscriptionThankYouEmailTemplate; }
        set { lock (_lock) _subscriptionThankYouEmailTemplate = value; }
    }

    public string TrialEndingSoonEmailTemplate
    {
        get { lock (_lock) return _trialEndingSoonEmailTemplate; }
        set { lock (_lock) _trialEndingSoonEmailTemplate = value; }
    }

    public string PasswordChangedEmailTemplate
    {
        get { lock (_lock) return _passwordChangedEmailTemplate; }
        set { lock (_lock) _passwordChangedEmailTemplate = value; }
    }
}
