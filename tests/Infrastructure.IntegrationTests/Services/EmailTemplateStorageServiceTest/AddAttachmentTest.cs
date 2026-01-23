using System.Text;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using VibraHeka.Infrastructure.Persistence.S3;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.EmailTemplateStorageServiceTest;

[TestFixture]
[Category("Integration")]
public class AddAttachmentTest : TestBase
{
    private IAmazonS3 S3 = default!;
    private EmailTemplateStorageRepository Repository = default!;
    private EmailTemplateStorageService Service = default!;

    private CancellationToken TestCancellationToken;
    private string BucketName = default!;

    [OneTimeSetUp]
    public void OneTimeSetUpS3()
    {
        RegionEndpoint? region = RegionEndpoint.GetBySystemName(_configuration.Location);

        S3 = new AmazonS3Client(new AmazonS3Config
        {
            RegionEndpoint = region,
            Profile = new Profile(_configuration.Profile)
        });

        BucketName = _configuration.EmailTemplatesBucketName;
        TestCancellationToken = CancellationToken.None;

        Repository = new EmailTemplateStorageRepository(S3, _configuration);
        Service = new EmailTemplateStorageService(Repository);
    }

    [OneTimeTearDown]
    public void OneTimeTearDownS3()
    {
        S3?.Dispose();
    }

    [Test]
    public async Task ShouldReturnAttachmentUrlWhenAddAttachmentIsCalled()
    {
        // Given: a valid attachment stream to verify upload and URL generation.
        string templateId = Guid.NewGuid().ToString("N");
        string attachmentName = $"{Guid.NewGuid():N}.bin";
        byte[] expectedBytes = Encoding.UTF8.GetBytes("attachment-service");
        await using MemoryStream attachmentStream = new MemoryStream(expectedBytes);

        string objectKey = $"{templateId}/attachments/{attachmentName}";
        string expectedTempPath = Path.Combine(Path.GetTempPath(), attachmentName);

        // When: adding the attachment via the service.
        Result<string> result =
            await Service.AddAttachment(templateId, attachmentStream, attachmentName, TestCancellationToken);

        // Then
        Assert.That(result.IsSuccess);
        Assert.That(
            result.Value,
            Is.EqualTo($"https://{BucketName}.s3.{_configuration.Location}.amazonaws.com/{objectKey}"));
        Assert.That(File.Exists(expectedTempPath), Is.False);

        using (GetObjectResponse response = await S3.GetObjectAsync(BucketName, objectKey, TestCancellationToken))
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
