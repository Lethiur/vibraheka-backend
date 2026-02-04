using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SettingsRepositoryTest;

[TestFixture]
public class GetVerificationEmailTemplateAsyncTest : TestBase
{
    private IAmazonSimpleSystemsManagement _ssmClient;
    private SettingsRepository _repository;
    private const string ParameterName = "/VibraHeka/VerificationEmailTemplate";

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        AmazonSimpleSystemsManagementConfig amazonSimpleSystemsManagementConfig = new AmazonSimpleSystemsManagementConfig() { Profile = new Profile("Twingers") };

        // Usamos un Profile para asegurar que se conecta a la cuenta de sandbox/test
        _ssmClient = new AmazonSimpleSystemsManagementClient(amazonSimpleSystemsManagementConfig);
        _repository = new SettingsRepository(_ssmClient);
    }

    [OneTimeTearDown]
    [TearDown]
    public void TearDown()
    {
        _ssmClient?.Dispose();
    }
    
    [Test]
    public async Task ShouldOverwriteExistingParameterWhenUpdating()
    {
        // Given
        string firstTemplate = "First Template";
        string secondTemplate = "Second Template (Updated)";

        await _ssmClient.PutParameterAsync(new PutParameterRequest
        {
            Name = ParameterName,
            Value = firstTemplate,
            Type = ParameterType.String,
            Overwrite = true
        });

        // When
        Result<Unit> updateResult = await _repository.UpdateVerificationEmailTemplateAsync(secondTemplate, CancellationToken.None);
        Result<string> getResult = await _repository.GetVerificationEmailTemplateAsync();

        // Then
        Assert.That(updateResult.IsSuccess, Is.True);
        Assert.That(getResult.Value, Is.EqualTo(secondTemplate));
    }
}
