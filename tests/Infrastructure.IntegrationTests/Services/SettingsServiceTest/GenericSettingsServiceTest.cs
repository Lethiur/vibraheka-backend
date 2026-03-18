using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SimpleSystemsManagement;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SettingsServiceTest;

public abstract class GenericSettingsServiceTest : TestBase
{
    protected SettingsService _service;
    protected SettingsRepository _repository;
    protected IAmazonSimpleSystemsManagement _ssmClient;
    protected AppSettingsEntity _appSettings;

    [SetUp]
    public void SetUp()
    {
        string profileName = _configuration.Profile;
        RegionEndpoint? region = RegionEndpoint.GetBySystemName(_configuration.Location);

        CredentialProfileStoreChain chain = new();
        if (!chain.TryGetAWSCredentials(profileName, out AWSCredentials? credentials))
        {
            throw new InvalidOperationException($"AWS Profile '{profileName}' not found in local credentials file.");
        }

        _ssmClient = new AmazonSimpleSystemsManagementClient(credentials, new AmazonSimpleSystemsManagementConfig
        {
            RegionEndpoint = region
        });

        _appSettings = CreateAppSettings();
        _repository = new SettingsRepository(_ssmClient, _configuration, CreateTestLogger<SettingsRepository>());
        _service = new SettingsService(
            _repository,
            new StaticOptionsMonitor<AppSettingsEntity>(_appSettings),
            CreateTestLogger<SettingsService>());
    }

    [TearDown]
    public void TearDown()
    {
        _ssmClient?.Dispose();
    }

    private sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue { get; private set; } = value;

        public T Get(string? name) => CurrentValue;

        public IDisposable? OnChange(Action<T, string?> listener)
        {
            return null;
        }
    }
}
