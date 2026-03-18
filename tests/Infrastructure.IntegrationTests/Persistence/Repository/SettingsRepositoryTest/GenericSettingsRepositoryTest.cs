using Amazon;
using Amazon.SimpleSystemsManagement;
using Microsoft.Extensions.Logging;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SettingsRepositoryTest;

public abstract class GenericSettingsRepositoryTest : TestBase
{
    protected IAmazonSimpleSystemsManagement SSMClient;
    protected SettingsRepository Repository;
    protected string VerificationParameterName = string.Empty;
    protected string RecoverPasswordParameterName = string.Empty;
    protected string UserWelcomeParameterName = string.Empty;
    protected string SubscriptionThankYouParameterName = string.Empty;
    protected string TrialEndingSoonParameterName = string.Empty;
    protected string PasswordChangedParameterName = string.Empty;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        AmazonSimpleSystemsManagementConfig amazonSimpleSystemsManagementConfig = new() { Profile = new Profile("Twingers") };

        // Usamos un Profile para asegurar que se conecta a la cuenta de sandbox/test
        SSMClient = new AmazonSimpleSystemsManagementClient(amazonSimpleSystemsManagementConfig);
        Repository = new SettingsRepository(SSMClient, _configuration, CreateTestLogger<SettingsRepository>());
        VerificationParameterName = $"/{_configuration.SettingsNameSpace}/VerificationEmailTemplate";
        RecoverPasswordParameterName = $"/{_configuration.SettingsNameSpace}/RecoverPasswordEmailTemplate";
        UserWelcomeParameterName = $"/{_configuration.SettingsNameSpace}/UserWelcomeEmailTemplate";
        SubscriptionThankYouParameterName = $"/{_configuration.SettingsNameSpace}/SubscriptionThankYouEmailTemplate";
        TrialEndingSoonParameterName = $"/{_configuration.SettingsNameSpace}/TrialEndingSoonEmailTemplate";
        PasswordChangedParameterName = $"/{_configuration.SettingsNameSpace}/PasswordChangedEmailTemplate";
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        SSMClient?.Dispose();
    }
    
}

