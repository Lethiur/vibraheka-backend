using System.ComponentModel;
using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Bogus;
using CSharpFunctionalExtensions;
using DotEnv.Core;
using MediatR;
using VibraHeka.Domain.Common.Interfaces.Settings;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SettingsRepositoryTest;

[TestFixture]
public class UpdateVerificationEmailTemplateTest
{
    private ISettingsRepository _repository;
    private IAmazonSimpleSystemsManagement _ssmClient;
    private Faker _faker;
    private const string ParameterName = "/VibraHeka/VerificationEmailTemplate";

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        new EnvLoader().Load();
        
        AmazonSimpleSystemsManagementConfig amazonSimpleSystemsManagementConfig = new AmazonSimpleSystemsManagementConfig() { Profile = new Profile("Twingers") };

        // Usamos un Profile para asegurar que se conecta a la cuenta de sandbox/test
       
        
        _ssmClient = new AmazonSimpleSystemsManagementClient(amazonSimpleSystemsManagementConfig);
        _repository = new SettingsRepository(_ssmClient);
        _faker = new Faker();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _ssmClient?.Dispose();
    }

    #region UpdateVerificationEmailTemplate - Success Cases

    [Test]
    [DisplayName("Should update verification email template successfully in SSM")]
    public async Task ShouldUpdateVerificationEmailTemplateSuccessfully()
    {
        // Given: A random email template content
        string emailTemplate = $"<html><body><h1>Verify your email</h1><p>{_faker.Lorem.Sentence()}</p></body></html>";

        // When: Updating the template in SSM
        Result<Unit> result = await _repository.UpdateVerificationEmailTemplateAsync(emailTemplate, CancellationToken.None);

        // Then: Should return success
        Assert.That(result.IsSuccess, Is.True);

        // And: Verify the value was actually stored in AWS SSM
        GetParameterResponse? response = await _ssmClient.GetParameterAsync(new GetParameterRequest { Name = ParameterName });
        Assert.That(response.Parameter.Value, Is.EqualTo(emailTemplate));
    }

    [Test]
    [DisplayName("Should overwrite existing template when updated again")]
    public async Task ShouldOverwriteExistingTemplate()
    {
        // Given: An initial template already in SSM
        await _repository.UpdateVerificationEmailTemplateAsync("initial template", CancellationToken.None);
        string newTemplate = "updated template " + _faker.Random.Guid();

        // When: Updating the same parameter
        Result<Unit> result = await _repository.UpdateVerificationEmailTemplateAsync(newTemplate, CancellationToken.None);

        // Then: Should succeed and reflect the new value
        Assert.That(result.IsSuccess, Is.True);
        
        GetParameterResponse? response = await _ssmClient.GetParameterAsync(new GetParameterRequest { Name = ParameterName });
        Assert.That(response.Parameter.Value, Is.EqualTo(newTemplate));
    }

    #endregion

    #region UpdateVerificationEmailTemplate - Edge Cases

    [Test]
    [DisplayName("Should handle very large template content")]
    public async Task ShouldHandleLargeTemplateContent()
    {
        // Given: A large string (SSM Standard parameters support up to 4KB)
        string largeTemplate = new string('A', 3000); 

        // When: Updating the template
        Result<Unit> result = await _repository.UpdateVerificationEmailTemplateAsync(largeTemplate, CancellationToken.None);

        // Then: Should return success if within limits
        Assert.That(result.IsSuccess, Is.True);
    }

    #endregion
}
