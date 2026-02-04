using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

[TestFixture]
public class GetTemplateUrlAsyncTest : GenericEmailTemplateStorageRepositoryTest
{
    [Test]
    public async Task ShouldGeneratePreSignedDownloadUrlForTemplateJson()
    {
        // Given
        const string templateId = "template-123";
        string expectedKey = $"{templateId}/template.json";
        const string expectedUrl = "https://example.com/presigned";

        ClientMock
            .Setup(c => c.GetPreSignedURLAsync(It.Is<GetPreSignedUrlRequest>(r =>
                r.BucketName == Options.EmailTemplatesBucketName &&
                r.Key == expectedKey &&
                r.Verb == HttpVerb.GET)))
            .ReturnsAsync(expectedUrl);

        // When
        Result<string> result = await Repository.GetTemplateUrlAsync(templateId);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedUrl));
    }
}

