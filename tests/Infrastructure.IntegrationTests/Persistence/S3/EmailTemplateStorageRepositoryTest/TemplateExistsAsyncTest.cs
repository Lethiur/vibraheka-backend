using System.ComponentModel;
using System.Text;
using CSharpFunctionalExtensions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

[TestFixture]
[NUnit.Framework.Category("Integration")]
public class TemplateExistsAsyncTest : GenericEmailTemplateStorageRepositoryIntegrationTest
{
    [Test]
    [DisplayName("Should return true after template is uploaded")]
    public async Task ShouldReturnTrueAfterTemplateUploaded()
    {
        // Given
        string templateId = Guid.NewGuid().ToString("N");
        byte[] bytes = Encoding.UTF8.GetBytes("""{"template":"Exists"}""");
        await using MemoryStream templateStream = new(bytes);

        Result<string> saveResult = await Repository.SaveTemplate(templateId, templateStream, TestCancellationToken);
        Assert.That(saveResult.IsSuccess, Is.True);

        // When
        Result<bool> existsResult = await Repository.TemplateExistsAsync(templateId, TestCancellationToken);

        // Then
        Assert.That(existsResult.IsSuccess, Is.True);
        Assert.That(existsResult.Value, Is.True);

        // Cleanup
        await S3.DeleteObjectAsync(BucketName, $"{templateId}/template.json", TestCancellationToken);
    }

    [Test]
    [DisplayName("Should return false for missing template")]
    public async Task ShouldReturnFalseForMissingTemplate()
    {
        string templateId = $"missing-{Guid.NewGuid():N}";
        Result<bool> result = await Repository.TemplateExistsAsync(templateId, TestCancellationToken);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.False);
    }
}

