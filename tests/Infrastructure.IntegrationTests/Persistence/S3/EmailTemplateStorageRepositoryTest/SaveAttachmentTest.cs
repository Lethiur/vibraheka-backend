using System.Text;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

[TestFixture]
[Category("Integration")]
public class SaveAttachmentTest : GenericEmailTemplateStorageRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldUploadAttachmentToS3AndDeleteTempFileWhenSaveAttachmentIsCalled()
    {
        // Given: a valid attachment stream to verify upload and temp cleanup.
        string templateId = Guid.NewGuid().ToString("N");
        string attachmentName = $"{Guid.NewGuid():N}.bin";
        byte[] expectedBytes = Encoding.UTF8.GetBytes("attachment-data");

        await using MemoryStream attachmentStream = new MemoryStream(expectedBytes);
        attachmentStream.Position = 2;

        string expectedTempPath = Path.Combine(Path.GetTempPath(), attachmentName);

        // When: saving the attachment.
        Result<string> result = await Repository.SaveAttachment(templateId, attachmentStream, attachmentName, TestCancellationToken);

        // Then
        Assert.That(result.IsSuccess);
        Assert.That(File.Exists(expectedTempPath), Is.False);

        string objectKey = $"{templateId}/attachments/{attachmentName}";
        using (GetObjectResponse response =
               await S3.GetObjectAsync(BucketName, objectKey, TestCancellationToken))
        await using (Stream responseStream = response.ResponseStream)
        {
            using MemoryStream ms = new MemoryStream();
            await responseStream.CopyToAsync(ms, TestCancellationToken);
            byte[] actualBytes = ms.ToArray();

            Assert.That(actualBytes, Is.EqualTo(expectedBytes));
        }

        // Remote cleanup.
        await S3.DeleteObjectAsync(BucketName, objectKey, TestCancellationToken);
    }
}
