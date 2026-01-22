using System.Text;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Infrastructure.Persistence.S3;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

[TestFixture]
[Category("Integration")]
public class SaveTemplateTest : TestBase
{
    private IAmazonS3 _s3 = null!;
    
    [OneTimeSetUp]
    public void SetupS3()
    {
        base.OneTimeSetUp();
        RegionEndpoint? region = RegionEndpoint.GetBySystemName(_configuration.Location);

        _s3 = new AmazonS3Client(new AmazonS3Config
        {
            RegionEndpoint = region,
            Profile = new Profile(_configuration.Profile)
        });
    }
    
    [OneTimeTearDown]
    public void TearDownS3()
    {
        _s3?.Dispose();
    }   
    
     [Test]
    public async Task ShouldUploadTemplateToS3AndDeleteTempFileWhenSaveTemplateIsCalled()
    {
        // Given
        string templateId = Guid.NewGuid().ToString("N");
        string expectedJson = """{"template":"Hello","subject":"World"}""";
        byte[] expectedBytes = Encoding.UTF8.GetBytes(expectedJson);

        await using MemoryStream templateStream = new MemoryStream(expectedBytes);

        EmailTemplateStorageRepository repository = new EmailTemplateStorageRepository(_s3, _configuration);

        string expectedTempPath = Path.Combine(Path.GetTempPath(), templateId);

        // When
        Result<string> result = await repository.SaveTemplate(templateId, templateStream, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);

        using (GetObjectResponse response =
               await _s3.GetObjectAsync(_configuration.EmailTemplatesBucketName, templateId))
        await using (Stream responseStream = response.ResponseStream)
        {
            using MemoryStream ms = new MemoryStream();
            await responseStream.CopyToAsync(ms);
            byte[] actualBytes = ms.ToArray();

            Assert.That(actualBytes, Is.EqualTo(expectedBytes));
        }

        Assert.That(File.Exists(expectedTempPath), Is.False);

        // Cleanup remoto
        await _s3.DeleteObjectAsync(_configuration.EmailTemplatesBucketName, templateId);
    }

    [Test]
    public async Task ShouldOverwriteRemoteObjectWhenSaveTemplateIsCalledTwiceWithSameTemplateId()
    {
        // Given
        string templateId = Guid.NewGuid().ToString("N");
        byte[] bytesV1 = Encoding.UTF8.GetBytes("""{"template":"V1"}""");
        byte[] bytesV2 = Encoding.UTF8.GetBytes("""{"template":"V2"}""");

        EmailTemplateStorageRepository repository = new EmailTemplateStorageRepository(_s3, _configuration);

        // When
        await using (MemoryStream s1 = new MemoryStream(bytesV1))
        {
            Result<string> r1 = await repository.SaveTemplate(templateId, s1, CancellationToken.None);
            Assert.That(r1.IsSuccess, Is.True);
        }

        await using (MemoryStream s2 = new MemoryStream(bytesV2))
        {
            Result<string> r2 = await repository.SaveTemplate(templateId, s2, CancellationToken.None);
            Assert.That(r2.IsSuccess, Is.True);
        }

        // Then
        using (GetObjectResponse response =
               await _s3.GetObjectAsync(_configuration.EmailTemplatesBucketName, $"{templateId}/template.json"))
        await using (Stream responseStream = response.ResponseStream)
        {
            using MemoryStream ms = new MemoryStream();
            await responseStream.CopyToAsync(ms);
            byte[] actualBytes = ms.ToArray();

            Assert.That(actualBytes, Is.EqualTo(bytesV2));
        }

        // Cleanup remoto
        await _s3.DeleteObjectAsync(_configuration.EmailTemplatesBucketName, $"{templateId}/template.json");
    }
}
