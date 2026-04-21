using System.ComponentModel;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.S3.RecordingStorageRepositoryTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetDownloadUrlAsyncTest : GenericRecordingStorageRepositoryTest
{
    [Test]
    [DisplayName("Should extract S3 object key from full HTTPS URL and generate pre-signed URL")]
    public async Task ShouldExtractS3ObjectKeyFromFullUrlAndGeneratePreSignedUrl()
    {
        // Given: a storageKey that is a full S3 HTTPS URL — the repository must strip the host
        string recordingId = Guid.NewGuid().ToString();
        string fileName = "meditacion.mp4";
        string storageKey = $"https://{Config.RecordingsBucketName}.s3.us-east-1.amazonaws.com/{recordingId}/{fileName}";
        string expectedS3Key = $"{recordingId}/{fileName}";
        string preSignedUrl = "https://pre-signed-url.example.com/download?token=abc";

        ClientMock
            .Setup(c => c.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .ReturnsAsync(preSignedUrl);

        // When: GetDownloadUrlAsync is called with the full URL storage key
        Result<string> result = await Repository.GetDownloadUrlAsync(storageKey, CancellationToken.None);

        // Then: result should be success and GetPreSignedURLAsync called with extracted key (not the full URL)
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Is.EqualTo(preSignedUrl),
            $"Expected pre-signed URL '{preSignedUrl}' but got '{result.Value}'");

        ClientMock.Verify(
            c => c.GetPreSignedURLAsync(
                It.Is<GetPreSignedUrlRequest>(r =>
                    r.BucketName == Config.RecordingsBucketName &&
                    r.Key == expectedS3Key &&
                    r.Verb == Amazon.S3.HttpVerb.GET &&
                    r.Expires > DateTime.UtcNow)),
            Times.Once,
            $"Expected GetPreSignedURLAsync called once with BucketName='{Config.RecordingsBucketName}' and Key='{expectedS3Key}'");

        ClientMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should use storageKey as-is when it is not a valid absolute URI (fallback path)")]
    public async Task ShouldUseStorageKeyAsIsWhenNotAValidAbsoluteUri()
    {
        // Given: a storageKey that is a plain relative path (not an absolute URI)
        string plainKey = "recordings/some-id/yoga.mp4";
        string preSignedUrl = "https://pre-signed-url.example.com/download?fallback=1";

        ClientMock
            .Setup(c => c.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .ReturnsAsync(preSignedUrl);

        // When: GetDownloadUrlAsync is called with the plain key
        Result<string> result = await Repository.GetDownloadUrlAsync(plainKey, CancellationToken.None);

        // Then: result should be success and GetPreSignedURLAsync called with the original key unchanged
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Is.EqualTo(preSignedUrl),
            $"Expected pre-signed URL '{preSignedUrl}' but got '{result.Value}'");

        ClientMock.Verify(
            c => c.GetPreSignedURLAsync(
                It.Is<GetPreSignedUrlRequest>(r =>
                    r.BucketName == Config.RecordingsBucketName &&
                    r.Key == plainKey &&
                    r.Verb == Amazon.S3.HttpVerb.GET)),
            Times.Once,
            $"Expected GetPreSignedURLAsync called once with the original key '{plainKey}' as fallback");

        ClientMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should call GetPreSignedURLAsync with expiry of 3600 seconds")]
    public async Task ShouldCallGetPreSignedUrlWithExpiryOf3600Seconds()
    {
        // Given: any valid storage key
        string storageKey = $"recordings/{Guid.NewGuid()}/audio.mp3";
        string preSignedUrl = "https://pre-signed-url.example.com/download?expires=3600";
        DateTime beforeCall = DateTime.UtcNow;

        ClientMock
            .Setup(c => c.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .ReturnsAsync(preSignedUrl);

        // When: GetDownloadUrlAsync is called
        Result<string> result = await Repository.GetDownloadUrlAsync(storageKey, CancellationToken.None);

        // Then: GetPreSignedURLAsync should have been called with Expires ≈ now + 3600 seconds
        DateTime expectedMinExpiry = beforeCall.AddSeconds(3600);
        DateTime expectedMaxExpiry = DateTime.UtcNow.AddSeconds(3600);

        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        ClientMock.Verify(
            c => c.GetPreSignedURLAsync(
                It.Is<GetPreSignedUrlRequest>(r =>
                    r.Expires >= expectedMinExpiry &&
                    r.Expires <= expectedMaxExpiry)),
            Times.Once,
            $"Expected GetPreSignedURLAsync called with Expires in range [{expectedMinExpiry:O}, {expectedMaxExpiry:O}] (3600 seconds from call time)");

        ClientMock.VerifyNoOtherCalls();
    }
}
