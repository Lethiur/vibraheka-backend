using Amazon.SimpleSystemsManagement;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SettingsServiceTest;

[TestFixture]
public class GetTemplatesForActionsTest : TestBase
{
    private IAmazonSimpleSystemsManagement _ssmClient;
    private SettingsRepository _repository;
    private SettingsService _service;
    private AppSettingsEntity _appSettings;

    [SetUp]
    public void SetUp()
    {
        // Given
        AmazonSimpleSystemsManagementConfig config = new AmazonSimpleSystemsManagementConfig
        {
            AuthenticationRegion = "eu-west-1"
        };

        _ssmClient = new AmazonSimpleSystemsManagementClient(config);
        _repository = new SettingsRepository(_ssmClient);
        _appSettings = CreateAppSettings();
        // Cargamos los settings iniciales desde la configuración de TestBase
        _service = new SettingsService(_repository, _appSettings);
    }

    [TearDown]
    [OneTimeTearDown]
    public void TearDown()
    {
        _ssmClient?.Dispose();
    }

    [Test]
    public void ShouldReturnCurrentStateOfTemplatesFromMemory()
    {
        // Given
        const string manualValue = "manual-memory-value";
        _appSettings.VerificationEmailTemplate = manualValue;

        // When
        Result<IEnumerable<TemplateForActionEntity>> result = _service.GetAllTemplatesForActions();

        // Then
        Assert.That(result.IsSuccess, Is.True);
        List<TemplateForActionEntity> templates = result.Value.ToList();
        Assert.That(templates[0].TemplateID, Is.EqualTo(manualValue));
    }
}
