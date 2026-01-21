using System.ComponentModel;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SimpleSystemsManagement;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;
using static System.Threading.CancellationToken;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SettingsServiceTest;

[TestFixture]
public class SettingsServiceIntegrationTests : TestBase
{
    private SettingsService _service;
    private SettingsRepository _repository;
    private IAmazonSimpleSystemsManagement _ssmClient;

    [SetUp]
    public void SetUp()
    {
        string profileName = _configuration.Profile;
        RegionEndpoint? region = Amazon.RegionEndpoint.GetBySystemName(_configuration.Region);

      
        CredentialProfileStoreChain chain = new Amazon.Runtime.CredentialManagement.CredentialProfileStoreChain();
        if (!chain.TryGetAWSCredentials(profileName, out AWSCredentials? credentials))
        {
            throw new InvalidOperationException($"AWS Profile '{profileName}' not found in local credentials file.");
        }

        _ssmClient = new AmazonSimpleSystemsManagementClient(credentials, new AmazonSimpleSystemsManagementConfig
        {
            RegionEndpoint = region
        });

        _repository = new SettingsRepository(_ssmClient);
        _service = new SettingsService(_repository, CreateAppSettings());
    }

    [TearDown]
    public void TearDown()
    {
        _ssmClient?.Dispose();
    }

    [Test]
    [DisplayName("Should update verification email template in SSM Parameter Store successfully")]
    public async Task ShouldUpdateVerificationEmailTemplateInSSMSuccessfully()
    {
        // Given: A new random email template content
        string newTemplate = $"<html><body><h1>Verify your account</h1><p>{_faker.Lorem.Sentence()}</p></body></html>";
        CancellationToken cancellationToken = None;

        // When: Changing the email verification template through the service
        Result<Unit> result = await _service.ChangeEmailForVerificationAsync(newTemplate, cancellationToken);

        // Then: The operation should be successful in AWS SSM
        Assert.That(result.IsSuccess, Is.True, $"The update operation should succeed");
        Assert.That(result.Value, Is.EqualTo(Unit.Value));
    }

    [Test]
    [DisplayName("Should return failure when providing an invalid template string")]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null!)]
    public async Task ShouldReturnFailureWhenInvalidTemplateProvided(string invalidTemplate)
    {
        // Given: An invalid template string (validation happens in Service)

        // When: Attempting to change the template
        Result<Unit> result = await _service.ChangeEmailForVerificationAsync(invalidTemplate, None);

        // Then: It should fail with the specific validation error without calling AWS
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.InvalidVerificationEmailTemplate));
    }

    [Test]
    [DisplayName("Should handle AWS service cancellation correctly")]
    public async Task ShouldHandleCancellationDuringUpdate()
    {
        // Given: A cancelled token
        using CancellationTokenSource cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // When: Attempting to update via Service/Repository
        // Then: Should propagate the task cancellation and the result should be unexpected error
        Result<Unit> changeEmailForVerificationAsync = await _service.ChangeEmailForVerificationAsync("some template", cts.Token);
        Assert.That(changeEmailForVerificationAsync.IsFailure, Is.True);
        Assert.That(changeEmailForVerificationAsync.Error, Is.EqualTo(InfrastructureConfigErrors.UnexpectedError));
    }
}
