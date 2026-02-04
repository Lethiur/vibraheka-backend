using System.ComponentModel;
using System.Text;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.EmailTemplateStorageServiceTest;

[TestFixture]
[NUnit.Framework.Category("Integration")]
public class GetTemplateUrlAsyncTest : GenericEmailTemplateStorageServiceIntegrationTest
{
    [Test]
    [DisplayName("Should return pre-signed URL when template exists")]
    public async Task ShouldReturnPreSignedUrlWhenTemplateExists()
    {
        // Given
        string templateId = Guid.NewGuid().ToString("N");
        byte[] expectedBytes = Encoding.UTF8.GetBytes("""{"template":"UrlTest","subject":"S3"}""");
        await using MemoryStream templateStream = new(expectedBytes);

        Result<string> saveResult = await Service.SaveTemplate(templateId, templateStream, TestCancellationToken);
        Assert.That(saveResult.IsSuccess, Is.True);

        // When
        Result<string> urlResult = await Service.GetTemplateUrlAsync(templateId, TestCancellationToken);

        // Then
        Assert.That(urlResult.IsSuccess, Is.True);
        Assert.That(urlResult.Value, Is.Not.Null.Or.Empty);

        Uri uri = new(urlResult.Value, UriKind.Absolute);
        Assert.That(uri.Scheme, Is.EqualTo(Uri.UriSchemeHttps));
        Assert.That(uri.AbsolutePath, Is.EqualTo($"/{templateId}/template.json"));
        Assert.That(uri.Query, Does.Contain("X-Amz-"));

        // Remote cleanup.
        await S3.DeleteObjectAsync(BucketName, $"{templateId}/template.json", TestCancellationToken);
    }

    [Test]
    [DisplayName("Should return failure when template does not exist")]
    public async Task ShouldReturnFailureWhenTemplateDoesNotExist()
    {
        // Given
        string templateId = $"missing-{Guid.NewGuid():N}";

        // When
        Result<string> result = await Service.GetTemplateUrlAsync(templateId, TestCancellationToken);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.TemplateNotFound));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [DisplayName("Should return failure when template id is invalid")]
    public async Task ShouldReturnFailureWhenTemplateIdIsInvalid(string? templateId)
    {
        Result<string> result = await Service.GetTemplateUrlAsync(templateId!, TestCancellationToken);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(EmailTemplateErrors.InvalidTempalteID));
    }
}
