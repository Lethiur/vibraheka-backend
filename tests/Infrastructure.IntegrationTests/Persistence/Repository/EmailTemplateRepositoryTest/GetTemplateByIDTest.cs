using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using Bogus;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.EmailTemplateRepositoryTest;

[TestFixture]
public class GetTemplateByIDTest : TestBase
{
    private Infrastructure.Persistence.Repository.EmailTemplateRepository _repository;
    private IDynamoDBContext _dynamoContext;
    
    
    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _dynamoContext = CreateDynamoDBContext();
        _repository = new Infrastructure.Persistence.Repository.EmailTemplateRepository(_dynamoContext, _configuration);
        _faker = new Faker();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _dynamoContext?.Dispose();
    }

    #region GetTemplateByID - Success Cases

    [Test]
    [DisplayName("Should return template entity when ID exists")]
    public async Task ShouldReturnTemplateWhenIdExists()
    {
        // Given: A template already exists in DynamoDB
        string templateId = Guid.NewGuid().ToString();
        string expectedPath = $"templates/{_faker.System.FileName("html")}";
        
        await SeedTemplate(templateId, expectedPath);

        // When: Retrieving the template by ID
        Result<EmailEntity> result = await _repository.GetTemplateByID(templateId);

        // Then: Should return success and match the seeded data
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ID, Is.EqualTo(templateId));
        Assert.That(result.Value.Path, Is.EqualTo(expectedPath));
    }

    #endregion

    #region GetTemplateByID - Failure Cases

    [Test]
    [DisplayName("Should return failure when template ID does not exist")]
    public async Task ShouldReturnFailureWhenIdDoesNotExist()
    {
        // Given: A non-existent ID
        string nonExistentId = "non-existent-id-" + Guid.NewGuid();

        // When: Trying to retrieve it
        Result<EmailEntity> result = await _repository.GetTemplateByID(nonExistentId);

        // Then: Should return failure
        Assert.That(result.IsFailure, Is.True);
        // Nota: EmailTemplateErrors.TemplateNotFound es el error esperado según el repositorio
    }

    #endregion

    #region Helper Methods

    private async Task SeedTemplate(string id, string path)
    {
        EmailTemplateDBModel model = new EmailTemplateDBModel
        {
            TemplateID = id,
            Path = path,
            // Agregamos fechas para evitar el error de DateTimeOffset si el modelo las requiere
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        SaveConfig config = new SaveConfig()
        {
            OverrideTableName = _configuration.EmailTemplatesTable
        };

        await _dynamoContext.SaveAsync(model, config);
    }

    #endregion
}
