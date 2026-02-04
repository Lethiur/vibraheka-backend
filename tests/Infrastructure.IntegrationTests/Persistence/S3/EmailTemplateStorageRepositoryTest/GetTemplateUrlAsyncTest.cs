using System.ComponentModel;
using System.Text;
using CSharpFunctionalExtensions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

[TestFixture]
[NUnit.Framework.Category("Integration")]
public class GetTemplateUrlAsyncTest : GenericEmailTemplateStorageRepositoryIntegrationTest
{
    [Test]
    [DisplayName("Should return a pre-signed URL when template exists")]
    public async Task ShouldReturnPreSignedUrlWhenTemplateExists()
    {
        // Given
        string templateId = Guid.NewGuid().ToString("N");
        byte[] bytes = Encoding.UTF8.GetBytes("""{"template":"Url"}""");
        await using MemoryStream templateStream = new(bytes);

        Result<string> saveResult = await Repository.SaveTemplate(templateId, templateStream, TestCancellationToken);
        Assert.That(saveResult.IsSuccess, Is.True);

        // When
        Result<string> urlResult = await Repository.GetTemplateUrlAsync(templateId);

        // Then
        Assert.That(urlResult.IsSuccess, Is.True);
        Uri uri = new Uri(urlResult.Value, UriKind.Absolute);
        Assert.That(uri.Scheme, Is.EqualTo(Uri.UriSchemeHttps));
        Assert.That(uri.AbsolutePath, Is.EqualTo($"/{templateId}/template.json"));
        Assert.That(uri.Query, Does.Contain("X-Amz-"));

        // Cleanup
        await S3.DeleteObjectAsync(BucketName, $"{templateId}/template.json", TestCancellationToken);
    }
}

