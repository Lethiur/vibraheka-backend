using System.ComponentModel;
using System.Text;
using Amazon.S3;
using CSharpFunctionalExtensions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.EmailTemplateStorageServiceTest;

[TestFixture]
[NUnit.Framework.Category("Integration")]
public class GetTemplateContentTest : GenericEmailTemplateStorageServiceIntegrationTest
{
    [Test]
    [DisplayName("Should return template content when template exists")]
    public async Task ShouldReturnTemplateContentWhenTemplateExists()
    {
        // Given
        string templateId = Guid.NewGuid().ToString("N");
        const string expectedContent = """{"template":"Hello","subject":"World"}""";
        byte[] bytes = Encoding.UTF8.GetBytes(expectedContent);
        await using MemoryStream templateStream = new(bytes);

        Result<string> saveResult = await Service.SaveTemplate(templateId, templateStream, TestCancellationToken);
        Assert.That(saveResult.IsSuccess, Is.True);

        // When
        Result<string> contentResult = await Service.GetTemplateContent(templateId, TestCancellationToken);

        // Then
        Assert.That(contentResult.IsSuccess, Is.True);
        Assert.That(contentResult.Value, Is.EqualTo(expectedContent));

        // Remote cleanup.
        await S3.DeleteObjectAsync(BucketName, $"{templateId}/template.json", TestCancellationToken);
    }

    [Test]
    [DisplayName("Should throw when template does not exist")]
    public void ShouldThrowWhenTemplateDoesNotExist()
    {
        // Given
        string templateId = $"missing-{Guid.NewGuid():N}";

        // When / Then
        Assert.That(
            async () => await Service.GetTemplateContent(templateId, TestCancellationToken),
            Throws.TypeOf<AmazonS3Exception>());
    }
}
