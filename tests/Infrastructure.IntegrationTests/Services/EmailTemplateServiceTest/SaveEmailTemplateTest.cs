using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.EmailTemplateServiceTest;

[TestFixture]
public class SaveEmailTemplateTest : GenericEmailTemplateServiceTest
{
    [Test]
    public async Task ShouldSaveTemplateAndReturnTemplateIdWhenTemplateIsValid()
    {
        // Given: una plantilla valida con ID unico.
        EmailEntity emailTemplate = new()
        {
            ID = $"integration-template-{Guid.NewGuid():N}",
            Name = "Integration template",
            Path = $"templates/{Guid.NewGuid():N}.html",
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = "integration-test",
            LastModifiedBy = "integration-test"
        };

        // When: se guarda la plantilla por medio del servicio.
        Result<string> result = await _service.SaveEmailTemplate(emailTemplate, CancellationToken.None);

        // Then: debe completarse correctamente y devolver el ID.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(emailTemplate.ID));
    }

    [Test]
    public async Task ShouldReturnInvalidTemplateEntityWhenTemplateIsNull()
    {
        // Given: una entidad de plantilla nula.

        // When: se intenta guardar la plantilla nula.
        Result<string> result = await _service.SaveEmailTemplate(null!, CancellationToken.None);

        // Then: debe devolver error de entidad invalida.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.InvalidTemplateEntity));
    }
}
