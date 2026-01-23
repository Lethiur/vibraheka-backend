using System.Text;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using VibraHeka.Infrastructure.Persistence.S3;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

[TestFixture]
[Category("Integration")]
public class SaveAttachmentTest : TestBase
{
    private IAmazonS3 S3 = null!;

    [OneTimeSetUp]
    public void SetupS3()
    {
        base.OneTimeSetUp();
        RegionEndpoint? region = RegionEndpoint.GetBySystemName(_configuration.Location);

        S3 = new AmazonS3Client(new AmazonS3Config
        {
            RegionEndpoint = region,
            Profile = new Profile(_configuration.Profile)
        });
    }

    [OneTimeTearDown]
    public void TearDownS3()
    {
        S3?.Dispose();
    }

    [Test]
    public async Task ShouldUploadAttachmentToS3AndDeleteTempFileWhenSaveAttachmentIsCalled()
    {
        // Given: a valid attachment stream to verify upload and temp cleanup.
        string templateId = Guid.NewGuid().ToString("N");
        string attachmentName = $"{Guid.NewGuid():N}.bin";
        byte[] expectedBytes = Encoding.UTF8.GetBytes("attachment-data");

        await using MemoryStream attachmentStream = new MemoryStream(expectedBytes);
        attachmentStream.Position = 2;

        EmailTemplateStorageRepository repository = new EmailTemplateStorageRepository(S3, _configuration);

        string expectedTempPath = Path.Combine(Path.GetTempPath(), attachmentName);

        // When: saving the attachment.
        Result<string> result = await repository.SaveAttachment(templateId, attachmentStream, attachmentName, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess);
        Assert.That(File.Exists(expectedTempPath), Is.False);

        string objectKey = $"{templateId}/attachments/{attachmentName}";
        using (GetObjectResponse response =
               await S3.GetObjectAsync(_configuration.EmailTemplatesBucketName, objectKey))
        await using (Stream responseStream = response.ResponseStream)
        {
            using MemoryStream ms = new MemoryStream();
            await responseStream.CopyToAsync(ms);
            byte[] actualBytes = ms.ToArray();

            Assert.That(actualBytes, Is.EqualTo(expectedBytes));
        }

        // Remote cleanup.
        await S3.DeleteObjectAsync(_configuration.EmailTemplatesBucketName, objectKey);
    }
}
