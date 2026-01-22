using System.Text;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.S3;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

public class SaveTemplateTest
{
    private Mock<IAmazonS3> ClientMock;
    private AWSConfig Options;

    [SetUp]
    public void SetUp()
    {
        ClientMock = new Mock<IAmazonS3>(MockBehavior.Loose);

        Options = new AWSConfig
        {
            EmailTemplatesBucketName = "unit-test-bucket"
        };
    }

    [Test]
    [Description("Given a valid template stream, when saving the template, then it should upload the temp file and return success")]
    public async Task ShouldUploadTempFileAndReturnSuccessWhenSaveTemplateIsCalled()
    {
        // Given
        string templateId = Guid.NewGuid().ToString("N");
        string expectedContent = """{"template":"Hello","subject":"World"}""";
        byte[] expectedBytes = Encoding.UTF8.GetBytes(expectedContent);
        MemoryStream templateStream = new MemoryStream(expectedBytes);

        CancellationToken cancellationToken = CancellationToken.None;

        ClientMock
            .Setup(c => c.PutObjectAsync(It.IsAny<PutObjectRequest>(), cancellationToken))
            .ReturnsAsync(new PutObjectResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });
        
        ClientMock.Setup(c => c.Config).Returns(new AmazonS3Config { RegionEndpoint = RegionEndpoint.USEast1 });

        EmailTemplateStorageRepository repository =
            new EmailTemplateStorageRepository(ClientMock.Object, Options);

        string expectedTempPath = Path.Combine(Path.GetTempPath(), templateId);

        // When
        Result<string> result = await repository.SaveTemplate(templateId, templateStream, cancellationToken);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        
        // And: File should have been deleted
        Assert.That(File.Exists(expectedTempPath), Is.False);

        
        ClientMock.Verify(
            c => c.PutObjectAsync(
                It.Is<PutObjectRequest>(r =>
                    r.BucketName == Options.EmailTemplatesBucketName &&
                    r.Key == $"{templateId}/template.json" &&
                    r.InputStream != null),
                cancellationToken),
            Times.Once);

        // Cleanup
        if (File.Exists(expectedTempPath))
        {
            File.Delete(expectedTempPath);
        }
    }

    [Test]
    [Description("Given a repository upload failure, when saving the template, then it should propagate the exception")]
    public void ShouldThrowWhenUploadAsyncThrows()
    {
        // Given
        string templateId = Guid.NewGuid().ToString("N");
        string content = """{"template":"Hello"}""";
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream templateStream = new MemoryStream(bytes);

        CancellationToken cancellationToken = CancellationToken.None;

        ClientMock
            .Setup(c => c.PutObjectAsync(It.IsAny<PutObjectRequest>(), cancellationToken))
            .ThrowsAsync(new IOException("Upload failed"));

        EmailTemplateStorageRepository repository =
            new EmailTemplateStorageRepository(ClientMock.Object, Options);

        string expectedTempPath = Path.Combine(Path.GetTempPath(), templateId);

        // When / Then
        Assert.That(
            async () => await repository.SaveTemplate(templateId, templateStream, cancellationToken),
            Throws.TypeOf<IOException>());

        // Cleanup (si llegó a crear el fichero)
        if (File.Exists(expectedTempPath))
        {
            File.Delete(expectedTempPath);
        }
    }
}
