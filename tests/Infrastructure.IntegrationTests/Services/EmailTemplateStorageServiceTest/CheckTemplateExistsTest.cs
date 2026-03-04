using CSharpFunctionalExtensions;
using MediatR;
using System.Text;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.EmailTemplateStorageServiceTest;

[TestFixture]
[Category("Integration")]
public class CheckTemplateExistsTest : GenericEmailTemplateStorageServiceIntegrationTest
{
    [Test]
    public async Task ShouldReturnSuccessWhenTemplateExists()
    {
        // Given: una plantilla existente en S3.
        string templateId = Guid.NewGuid().ToString("N");
        await using MemoryStream templateStream = new(Encoding.UTF8.GetBytes("""{"template":"exists"}"""));
        Result<string> saveResult = await Service.SaveTemplate(templateId, templateStream, TestCancellationToken);
        Assert.That(saveResult.IsSuccess, Is.True);

        // When: se verifica existencia de la plantilla.
        Result<Unit> result = await Service.CheckTemplateExists(templateId, TestCancellationToken);

        // Then: la verificacion debe ser exitosa.
        Assert.That(result.IsSuccess, Is.True);

        await S3.DeleteObjectAsync(BucketName, $"{templateId}/template.json", TestCancellationToken);
    }

    [Test]
    public async Task ShouldReturnTemplateNotFoundWhenTemplateDoesNotExist()
    {
        // Given: un template id inexistente.
        string templateId = $"missing-{Guid.NewGuid():N}";

        // When: se verifica existencia para una plantilla no creada.
        Result<Unit> result = await Service.CheckTemplateExists(templateId, TestCancellationToken);

        // Then: debe devolverse TemplateNotFound.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task ShouldReturnInvalidTemplateIdWhenTemplateIdIsInvalid(string? templateId)
    {
        // Given: un template id invalido.

        // When: se verifica existencia con id invalido.
        Result<Unit> result = await Service.CheckTemplateExists(templateId!, TestCancellationToken);

        // Then: debe devolverse InvalidTempalteID.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.InvalidTempalteID));
    }
}
