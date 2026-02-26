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
    protected string PasswordChangedParameterName = string.Empty;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        AmazonSimpleSystemsManagementConfig amazonSimpleSystemsManagementConfig = new AmazonSimpleSystemsManagementConfig() { Profile = new Profile("Twingers") };

        // Usamos un Profile para asegurar que se conecta a la cuenta de sandbox/test
        SSMClient = new AmazonSimpleSystemsManagementClient(amazonSimpleSystemsManagementConfig);
        Repository = new SettingsRepository(SSMClient, _configuration, LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SettingsRepository>());
        VerificationParameterName = $"/{_configuration.SettingsNameSpace}/VerificationEmailTemplate";
        PasswordChangedParameterName = $"/{_configuration.SettingsNameSpace}/PasswordChangedTemplate";
    }

    [OneTimeTearDown]
    [TearDown]
    public void TearDown()
    {
        SSMClient?.Dispose();
    }
    
}
