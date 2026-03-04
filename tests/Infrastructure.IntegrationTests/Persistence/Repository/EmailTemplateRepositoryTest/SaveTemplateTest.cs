using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.EmailTemplateRepositoryTest;

[TestFixture]
public class SaveTemplateTest : GenericEmailTemplateRepositoryIntegrationTest
{
    #region SaveTemplate - Success Cases

    [Test]
    [DisplayName("Should save email template successfully with valid ID and S3 path")]
    public async Task ShouldSaveTemplateSuccessfullyWhenValidDataProvided()
    {
        // Given: una entidad de template valida.
        EmailEntity template = CreateValidTemplate();

        // When: se guarda el template en repositorio.
        Result<Unit> result = await Repository.SaveTemplate(template, CancellationToken.None);

        // Then: debe persistirse correctamente.
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    [DisplayName("Should save template successfully with deep S3 path structure")]
    public async Task ShouldSaveTemplateSuccessfullyWhenDeepPathProvided()
    {
        // Given: una plantilla con ruta profunda de S3.
        EmailEntity template = new()
        {
            ID = Guid.NewGuid().ToString(),
            Path = "s3://my-bucket/templates/marketing/v1/user-welcome.html"
        };

        // When: se guarda la plantilla.
        Result<Unit> result = await Repository.SaveTemplate(template, CancellationToken.None);

        // Then: la operacion debe ser exitosa.
        Assert.That(result.IsSuccess, Is.True);
    }

    #endregion

    #region SaveTemplate - Data Persistence Verification

    [Test]
    [DisplayName("Should persist ID and Path correctly in DynamoDB")]
    public async Task ShouldPersistTemplateDataCorrectlyWhenSaved()
    {
        // Given: una plantilla con valores especificos.
        EmailEntity originalTemplate = CreateValidTemplate();

        // When: se guarda la plantilla y luego se carga desde DynamoDB.
        await Repository.SaveTemplate(originalTemplate, CancellationToken.None);

        LoadConfig loadConfig = new() { OverrideTableName = _configuration.EmailTemplatesTable };
        EmailTemplateDBModel? retrieved = await DynamoContext.LoadAsync<EmailTemplateDBModel>(originalTemplate.ID, loadConfig);

        // Then: los valores persistidos deben coincidir.
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.TemplateID, Is.EqualTo(originalTemplate.ID));
        Assert.That(retrieved.Path, Is.EqualTo(originalTemplate.Path));
        Assert.That(retrieved.Created, Is.EqualTo(originalTemplate.Created));
        Assert.That(retrieved.CreatedBy, Is.EqualTo(originalTemplate.CreatedBy));
        Assert.That(retrieved.LastModified, Is.EqualTo(originalTemplate.LastModified));
        Assert.That(retrieved.LastModifiedBy, Is.EqualTo(originalTemplate.LastModifiedBy));
    }

    #endregion

    #region SaveTemplate - Overwrite Cases

    [Test]
    [DisplayName("Should update Path when same template ID is saved with different location")]
    public async Task ShouldOverwritePathWhenSameIdSavedTwice()
    {
        // Given: una plantilla inicial guardada.
        string templateId = Guid.NewGuid().ToString();
        EmailEntity template = new() { ID = templateId, Path = "path/old.html" };
        await Repository.SaveTemplate(template, CancellationToken.None);

        // And: el mismo ID con nueva ruta.
        template.Path = "path/new.html";

        // When: se vuelve a guardar.
        Result<Unit> result = await Repository.SaveTemplate(template, CancellationToken.None);

        // Then: la ruta debe actualizarse.
        Assert.That(result.IsSuccess, Is.True);

        LoadConfig loadConfig = new() { OverrideTableName = _configuration.EmailTemplatesTable };
        EmailTemplateDBModel? retrieved = await DynamoContext.LoadAsync<EmailTemplateDBModel>(templateId, loadConfig);

        Assert.That(retrieved!.Path, Is.EqualTo("path/new.html"));
    }

    [Test]
    [DisplayName("Should return generic persistence error when save is cancelled")]
    public async Task ShouldReturnGenericPersistenceErrorWhenSaveIsCancelled()
    {
        // Given: una plantilla valida y un token de cancelacion cancelado.
        EmailEntity template = CreateValidTemplate();
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // When: se intenta guardar con la operacion cancelada.
        Result<Unit> result = await Repository.SaveTemplate(template, cts.Token);

        // Then: debe devolverse error general de persistencia.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }

    #endregion

    #region Helper Methods

    private EmailEntity CreateValidTemplate()
    {
        return new EmailEntity
        {
            ID = Guid.NewGuid().ToString(),
            Path = $"templates/{_faker.System.FileName("html")}",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            CreatedBy = "Test User",
            LastModifiedBy = "Test User"
        };
    }

    #endregion
}
