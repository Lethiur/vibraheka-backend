using System.Text;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

[TestFixture]
[Category("Integration")]
public class SaveTemplateTest : GenericEmailTemplateStorageRepositoryIntegrationTest
{
     [Test]
    public async Task ShouldUploadTemplateToS3AndDeleteTempFileWhenSaveTemplateIsCalled()
    {
        // Given: a valid template stream to verify upload and cleanup.
        string templateId = Guid.NewGuid().ToString("N");
        string expectedJson = """{"template":"Hello","subject":"World"}""";
        byte[] expectedBytes = Encoding.UTF8.GetBytes(expectedJson);

        await using MemoryStream templateStream = new MemoryStream(expectedBytes);

        string expectedTempPath = Path.Combine(Path.GetTempPath(), templateId);

        // When: saving the template.
        Result<string> result = await Repository.SaveTemplate(templateId, templateStream, TestCancellationToken);

        // Then
        Assert.That(result.IsSuccess, Is.True);

        using (GetObjectResponse response =
               await S3.GetObjectAsync(BucketName, $"{templateId}/template.json", TestCancellationToken))
        await using (Stream responseStream = response.ResponseStream)
        {
            using MemoryStream ms = new MemoryStream();
            await responseStream.CopyToAsync(ms, TestCancellationToken);
            byte[] actualBytes = ms.ToArray();

            Assert.That(actualBytes, Is.EqualTo(expectedBytes));
        }

        Assert.That(File.Exists(expectedTempPath), Is.False);

        // Remote cleanup.
        await S3.DeleteObjectAsync(BucketName, templateId, TestCancellationToken);
    }

    [Test]
    public async Task ShouldOverwriteRemoteObjectWhenSaveTemplateIsCalledTwiceWithSameTemplateId()
    {
        // Given: two template payloads to verify overwrite behavior.
        string templateId = Guid.NewGuid().ToString("N");
        byte[] bytesV1 = Encoding.UTF8.GetBytes("""{"template":"V1"}""");
        byte[] bytesV2 = Encoding.UTF8.GetBytes("""{"template":"V2"}""");

        // When: saving the template twice with the same id.
        await using (MemoryStream s1 = new MemoryStream(bytesV1))
        {
            Result<string> r1 = await Repository.SaveTemplate(templateId, s1, TestCancellationToken);
            Assert.That(r1.IsSuccess, Is.True);
        }

        await using (MemoryStream s2 = new MemoryStream(bytesV2))
        {
            Result<string> r2 = await Repository.SaveTemplate(templateId, s2, TestCancellationToken);
            Assert.That(r2.IsSuccess, Is.True);
        }

        // Then
        using (GetObjectResponse response =
               await S3.GetObjectAsync(BucketName, $"{templateId}/template.json", TestCancellationToken))
        await using (Stream responseStream = response.ResponseStream)
        {
            using MemoryStream ms = new MemoryStream();
            await responseStream.CopyToAsync(ms, TestCancellationToken);
            byte[] actualBytes = ms.ToArray();

            Assert.That(actualBytes, Is.EqualTo(bytesV2));
        }

        // Remote cleanup.
        await S3.DeleteObjectAsync(BucketName, $"{templateId}/template.json", TestCancellationToken);
    }
}
