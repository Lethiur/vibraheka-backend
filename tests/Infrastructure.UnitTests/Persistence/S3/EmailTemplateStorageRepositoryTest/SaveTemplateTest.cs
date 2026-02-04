using System.Text;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

public class SaveTemplateTest : GenericEmailTemplateStorageRepositoryTest
{
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
        
        string expectedTempPath = Path.Combine(Path.GetTempPath(), templateId);

        // When
        Result<string> result = await Repository.SaveTemplate(templateId, templateStream, cancellationToken);

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

        string expectedTempPath = Path.Combine(Path.GetTempPath(), templateId);

        // When / Then
        Assert.That(
            async () => await Repository.SaveTemplate(templateId, templateStream, cancellationToken),
            Throws.TypeOf<IOException>());

        // Cleanup (si llegó a crear el fichero)
        if (File.Exists(expectedTempPath))
        {
            File.Delete(expectedTempPath);
        }
    }
}
