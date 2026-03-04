using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.EmailTemplateRepositoryTest;

[TestFixture]
public class GetTemplateByIDTest : GenericEmailTemplateRepositoryIntegrationTest
{
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
        Result<EmailEntity> result = await Repository.GetTemplateByID(templateId, CancellationToken.None);

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
        Result<EmailEntity> result = await Repository.GetTemplateByID(nonExistentId, CancellationToken.None);

        // Then: Should return failure
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }

    [Test]
    [DisplayName("Should return generic persistence error when operation is cancelled")]
    public async Task ShouldReturnGenericPersistenceErrorWhenOperationIsCancelled()
    {
        // Given: un token de cancelacion cancelado.
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // When: se intenta recuperar plantilla con la operacion cancelada.
        Result<EmailEntity> result = await Repository.GetTemplateByID(Guid.NewGuid().ToString("N"), cts.Token);

        // Then: el repositorio debe mapear a error general de persistencia.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }

    #endregion

    #region Helper Methods

    private async Task SeedTemplate(string id, string path)
    {
        EmailTemplateDBModel model = new()
        {
            TemplateID = id,
            Path = path,
            // Agregamos fechas para evitar el error de DateTimeOffset si el modelo las requiere
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        SaveConfig config = new()
        {
            OverrideTableName = _configuration.EmailTemplatesTable
        };

        await DynamoContext.SaveAsync(model, config);
    }

    #endregion
}
