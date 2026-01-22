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
public class SaveTemplateTest : TestBase
{
    private IAmazonS3 _s3 = default!;
    private EmailTemplateStorageRepository _repository = default!;
    private EmailTemplateStorageService _service = default!;

    private CancellationToken _cancellationToken;
    private string _bucketName = default!;

    [OneTimeSetUp]
    public void OneTimeSetUpS3()
    {
        // Given (shared)
        RegionEndpoint? region = RegionEndpoint.GetBySystemName(_configuration.Location);

        _s3 = new AmazonS3Client(new AmazonS3Config
        {
            RegionEndpoint = region, Profile = new Profile(_configuration.Profile)
        });

        _bucketName = _configuration.EmailTemplatesBucketName;
        _cancellationToken = CancellationToken.None;

        _repository = new EmailTemplateStorageRepository(_s3, _configuration);
        _service = new EmailTemplateStorageService(_repository);
    }

    [OneTimeTearDown]
    public void OneTimeTearDownS3()
    {
        _s3?.Dispose();
    }

    [Test]
    public async Task ShouldReturnTemplateIdWhenSaveTemplateIsCalled()
    {
        // Given
        string templateId = Guid.NewGuid().ToString("N");
        byte[] expectedBytes = Encoding.UTF8.GetBytes("""{"template":"Hello","subject":"World"}""");
        await using MemoryStream templateStream = new MemoryStream(expectedBytes);

        // When
        Result<string> result = await _service.SaveTemplate(templateId, templateStream, _cancellationToken);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(  $"https://{_configuration.EmailTemplatesBucketName}.s3.{_configuration.Location}.amazonaws.com/{templateId}/template.json"));

        // Cleanup remoto
        await _s3.DeleteObjectAsync(_bucketName, templateId, _cancellationToken);
    }

    [Test]
    public async Task ShouldUploadTemplateToS3WhenSaveTemplateIsCalled()
    {
        // Given
        string templateId = Guid.NewGuid().ToString("N");
        byte[] expectedBytes = Encoding.UTF8.GetBytes("""{"template":"Integration","subject":"S3"}""");
        await using MemoryStream templateStream = new MemoryStream(expectedBytes);

        // When
        Result<string> result = await _service.SaveTemplate(templateId, templateStream, _cancellationToken);

        // Then
        Assert.That(result.IsSuccess, Is.True);

        using (GetObjectResponse response = await _s3.GetObjectAsync(_bucketName, $"{templateId}/template.json", _cancellationToken))
        await using (Stream responseStream = response.ResponseStream)
        {
            using MemoryStream ms = new MemoryStream();
            await responseStream.CopyToAsync(ms, _cancellationToken);
            byte[] actualBytes = ms.ToArray();

            Assert.That(actualBytes, Is.EqualTo(expectedBytes));
        }

        // Cleanup remoto
        await _s3.DeleteObjectAsync(_bucketName, $"{templateId}/template.json", _cancellationToken);
    }

    [Test]
    public async Task ShouldNotLeaveTempFileWhenSaveTemplateIsCalled()
    {
        // Este test solo aplica si el repositorio borra el fichero temporal en finally.
        // Given
        string templateId = Guid.NewGuid().ToString("N");
        byte[] expectedBytes = Encoding.UTF8.GetBytes("""{"template":"TempCleanup"}""");
        await using MemoryStream templateStream = new MemoryStream(expectedBytes);

        string expectedTempPath = Path.Combine(Path.GetTempPath(), templateId);

        // When
        Result<string> result = await _service.SaveTemplate(templateId, templateStream, _cancellationToken);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(File.Exists(expectedTempPath), Is.False);

        // Cleanup remoto
        await _s3.DeleteObjectAsync(_bucketName, templateId, _cancellationToken);
    }
}
