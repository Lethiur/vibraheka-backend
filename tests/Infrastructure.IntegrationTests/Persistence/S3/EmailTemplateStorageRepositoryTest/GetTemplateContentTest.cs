using System.ComponentModel;
using System.Text;
using Amazon.S3;
using CSharpFunctionalExtensions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

[TestFixture]
[NUnit.Framework.Category("Integration")]
public class GetTemplateContentTest : GenericEmailTemplateStorageRepositoryIntegrationTest
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

        Result<string> saveResult = await Repository.SaveTemplate(templateId, templateStream, TestCancellationToken);
        Assert.That(saveResult.IsSuccess, Is.True);

        // When
        Result<string> contentResult = await Repository.GetTemplateContent(templateId, TestCancellationToken);

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
        string templateId = $"missing-{Guid.NewGuid():N}";

        Assert.That(
            async () => await Repository.GetTemplateContent(templateId, TestCancellationToken),
            Throws.TypeOf<AmazonS3Exception>());
    }
}

