using System.Text;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.EmailTemplateStorageServiceTest;

[TestFixture]
[Category("Integration")]
public class SaveTemplateTest : GenericEmailTemplateStorageServiceIntegrationTest
{
    [Test]
    public async Task ShouldReturnTemplateIdWhenSaveTemplateIsCalled()
    {
        // Given: a valid template request to verify URL generation.
        string templateId = Guid.NewGuid().ToString("N");
        byte[] expectedBytes = Encoding.UTF8.GetBytes("""{"template":"Hello","subject":"World"}""");
        await using MemoryStream templateStream = new MemoryStream(expectedBytes);

        // When: saving the template via the service.
        Result<string> result = await Service.SaveTemplate(templateId, templateStream, TestCancellationToken);

        // Then
        Assert.That(result.IsSuccess);
        Assert.That(result.Value, Is.EqualTo($"https://{_configuration.EmailTemplatesBucketName}.s3.{_configuration.Location}.amazonaws.com/{templateId}/template.json"));

        // Remote cleanup.
        await S3.DeleteObjectAsync(BucketName, templateId, TestCancellationToken);
    }

    [Test]
    public async Task ShouldUploadTemplateToS3WhenSaveTemplateIsCalled()
    {
        // Given: a template payload to verify it is uploaded.
        string templateId = Guid.NewGuid().ToString("N");
        byte[] expectedBytes = Encoding.UTF8.GetBytes("""{"template":"Integration","subject":"S3"}""");
        await using MemoryStream templateStream = new MemoryStream(expectedBytes);

        // When: saving the template via the service.
        Result<string> result = await Service.SaveTemplate(templateId, templateStream, TestCancellationToken);

        // Then
        Assert.That(result.IsSuccess);

        using (GetObjectResponse response = await S3.GetObjectAsync(BucketName, $"{templateId}/template.json", TestCancellationToken))
        await using (Stream responseStream = response.ResponseStream)
        {
            using MemoryStream ms = new MemoryStream();
            await responseStream.CopyToAsync(ms, TestCancellationToken);
            byte[] actualBytes = ms.ToArray();

            Assert.That(actualBytes, Is.EqualTo(expectedBytes));
        }

        // Remote cleanup.
        await S3.DeleteObjectAsync(BucketName, $"{templateId}/template.json", TestCancellationToken);
    }

    [Test]
    public async Task ShouldNotLeaveTempFileWhenSaveTemplateIsCalled()
    {
        // This test only applies if the repository deletes the temp file in finally.
        // Given: a template payload to verify temp cleanup.
        string templateId = Guid.NewGuid().ToString("N");
        byte[] expectedBytes = Encoding.UTF8.GetBytes("""{"template":"TempCleanup"}""");
        await using MemoryStream templateStream = new MemoryStream(expectedBytes);

        string expectedTempPath = Path.Combine(Path.GetTempPath(), templateId);

        // When: saving the template via the service.
        Result<string> result = await Service.SaveTemplate(templateId, templateStream, TestCancellationToken);

        // Then
        Assert.That(result.IsSuccess);
        Assert.That(File.Exists(expectedTempPath), Is.False);

        // Remote cleanup.
        await S3.DeleteObjectAsync(BucketName, templateId, TestCancellationToken);
    }
}
