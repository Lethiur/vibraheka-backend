using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.EmailTemplateRepositoryTest;

[TestFixture]
public class GetAllTemplatesTest : GenericEmailTemplateRepositoryIntegrationTest
{
    [Test]
    [DisplayName("Should return all templates including seeded ones")]
    public async Task ShouldReturnAllTemplatesIncludingSeededOnes()
    {
        // Given: dos plantillas sembradas en la tabla de pruebas.
        string templateId1 = $"it-{Guid.NewGuid():N}";
        string templateId2 = $"it-{Guid.NewGuid():N}";

        SaveConfig saveConfig = new() { OverrideTableName = _configuration.EmailTemplatesTable };
        await DynamoContext.SaveAsync(new EmailTemplateDBModel
        {
            TemplateID = templateId1,
            Name = "Name 1",
            Path = "Path 1",
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        }, saveConfig);
        await DynamoContext.SaveAsync(new EmailTemplateDBModel
        {
            TemplateID = templateId2,
            Name = "Name 2",
            Path = "Path 2",
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        }, saveConfig);

        // When: se solicitan todas las plantillas del repositorio.
        Result<IEnumerable<EmailEntity>> result = await Repository.GetAllTemplates(CancellationToken.None);

        // Then: deben incluirse ambas plantillas insertadas.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Has.Some.Matches<EmailEntity>(t => t.ID == templateId1));
        Assert.That(result.Value, Has.Some.Matches<EmailEntity>(t => t.ID == templateId2));
    }

    [Test]
    [DisplayName("Should return generic persistence error when get all is cancelled")]
    public async Task ShouldReturnGenericPersistenceErrorWhenGetAllIsCancelled()
    {
        // Given: un cancellation token cancelado.
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // When: se consulta el listado con la operacion cancelada.
        Result<IEnumerable<EmailEntity>> result = await Repository.GetAllTemplates(cts.Token);

        // Then: debe devolverse error general de persistencia.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }
}
