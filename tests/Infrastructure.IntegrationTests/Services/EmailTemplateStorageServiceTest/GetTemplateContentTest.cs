using System.ComponentModel;
using System.Text;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.EmailTemplateStorageServiceTest;

[TestFixture]
[NUnit.Framework.Category("Integration")]
public class GetTemplateContentTest : GenericEmailTemplateStorageServiceIntegrationTest
{
    [Test]
    [DisplayName("Should return template content when template exists")]
    public async Task ShouldReturnTemplateContentWhenTemplateExists()
    {
        // Given: una plantilla previamente almacenada en S3.
        string templateId = Guid.NewGuid().ToString("N");
        const string expectedContent = """{"template":"Hello","subject":"World"}""";
        byte[] bytes = Encoding.UTF8.GetBytes(expectedContent);
        await using MemoryStream templateStream = new(bytes);

        Result<string> saveResult = await Service.SaveTemplate(templateId, templateStream, TestCancellationToken);
        Assert.That(saveResult.IsSuccess, Is.True);

        // When: se obtiene el contenido de la plantilla por id.
        Result<string> contentResult = await Service.GetTemplateContent(templateId, TestCancellationToken);

        // Then: debe devolverse el contenido exacto guardado.
        Assert.That(contentResult.IsSuccess, Is.True);
        Assert.That(contentResult.Value, Is.EqualTo(expectedContent));

        await S3.DeleteObjectAsync(BucketName, $"{templateId}/template.json", TestCancellationToken);
    }

    [Test]
    [DisplayName("Should return failure when template does not exist")]
    public async Task ShouldReturnFailureWhenTemplateDoesNotExist()
    {
        // Given: un template id inexistente.
        string templateId = $"missing-{Guid.NewGuid():N}";

        // When: se intenta obtener contenido de una plantilla inexistente.
        Result<string> result = await Service.GetTemplateContent(templateId, TestCancellationToken);

        // Then: debe fallar con TemplateNotFound.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }
}
