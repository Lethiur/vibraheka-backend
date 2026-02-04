using System.Text;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

[TestFixture]
public class GetTemplateContentTest : GenericEmailTemplateStorageRepositoryTest
{
    [Test]
    public async Task ShouldReturnFileContentsAsString()
    {
        // Given
        const string templateId = "template-1";
        const string expectedJson = """{"template":"Hello"}""";
        byte[] bytes = Encoding.UTF8.GetBytes(expectedJson);

        GetObjectResponse response = new GetObjectResponse
        {
            ResponseStream = new MemoryStream(bytes)
        };

        ClientMock
            .Setup(c => c.GetObjectAsync(Options.EmailTemplatesBucketName, $"{templateId}/template.json", TestCancellationToken))
            .ReturnsAsync(response);

        // When
        Result<string> result = await Repository.GetTemplateContent(templateId, TestCancellationToken);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedJson));
    }

    [Test]
    public void ShouldThrowWhenS3Throws()
    {
        // Given
        const string templateId = "template-1";
        ClientMock
            .Setup(c => c.GetObjectAsync(Options.EmailTemplatesBucketName, $"{templateId}/template.json", TestCancellationToken))
            .ThrowsAsync(new Amazon.S3.AmazonS3Exception("Boom"));

        // When / Then
        Assert.That(
            async () => await Repository.GetTemplateContent(templateId, TestCancellationToken),
            Throws.TypeOf<Amazon.S3.AmazonS3Exception>());
    }
}

