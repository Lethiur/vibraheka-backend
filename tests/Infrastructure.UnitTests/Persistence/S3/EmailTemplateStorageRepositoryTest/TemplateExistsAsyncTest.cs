using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.S3.EmailTemplateStorageRepositoryTest;

[TestFixture]
public class TemplateExistsAsyncTest : GenericEmailTemplateStorageRepositoryTest
{
    [Test]
    public async Task ShouldReturnTrueWhenObjectMetadataExists()
    {
        // Given
        const string templateId = "template-1";
        string key = $"{templateId}/template.json";

        ClientMock
            .Setup(c => c.GetObjectMetadataAsync(
                It.Is<GetObjectMetadataRequest>(r => r.BucketName == Options.EmailTemplatesBucketName && r.Key == key),
                TestCancellationToken))
            .ReturnsAsync(new GetObjectMetadataResponse());

        // When
        Result<bool> result = await Repository.TemplateExistsAsync(templateId, TestCancellationToken);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);
    }

    [Test]
    public async Task ShouldReturnFalseWhenObjectMetadataReturnsNotFound()
    {
        // Given
        const string templateId = "missing-template";
        Amazon.S3.AmazonS3Exception notFound = new Amazon.S3.AmazonS3Exception("Not Found")
        {
            StatusCode = System.Net.HttpStatusCode.NotFound
        };

        ClientMock
            .Setup(c => c.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), TestCancellationToken))
            .ThrowsAsync(notFound);

        // When
        Result<bool> result = await Repository.TemplateExistsAsync(templateId, TestCancellationToken);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.False);
    }
}

