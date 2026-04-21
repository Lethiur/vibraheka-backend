using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using System.ComponentModel;
using System.Net;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.S3;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.S3.RecordingStorageRepositoryTest;

[TestFixture]
public class RecordingStorageRepositoryTest
{
    private Mock<IAmazonS3> ClientMock = default!;
    private AWSConfig Config = default!;
    private RecordingStorageRepository Repository = default!;

    [SetUp]
    public void SetUp()
    {
        ClientMock = new Mock<IAmazonS3>(MockBehavior.Loose);
        ClientMock
            .Setup(c => c.Config)
            .Returns(new AmazonS3Config { RegionEndpoint = RegionEndpoint.USEast1 });

        Config = new AWSConfig { RecordingsBucketName = "unit-test-recordings-bucket" };
        Repository = new RecordingStorageRepository(ClientMock.Object, Config);
    }

    [Test]
    [DisplayName("Should delete temp file after a successful upload")]
    public async Task ShouldDeleteTempFileAfterSuccessfulUpload()
    {
        // Given: a valid stream, recordingId and fileName; S3 returns HTTP 200
        string recordingId = Guid.NewGuid().ToString();
        string fileName = "meditacion.mp4";
        Stream fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        string expectedTempPath = Path.Combine(Path.GetTempPath(), fileName);

        ClientMock
            .Setup(c => c.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

        // When: UploadAsync is called
        Result<string> result = await Repository.UploadAsync(recordingId, fileStream, fileName, CancellationToken.None);

        // Then: the temp file should have been deleted after the upload
        Assert.That(result.IsSuccess, Is.True,
            $"Expected upload to succeed but got failure: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(File.Exists(expectedTempPath), Is.False,
            $"Expected temp file '{expectedTempPath}' to be deleted after successful upload, but it still exists");

        ClientMock.Verify(
            c => c.PutObjectAsync(
                It.Is<PutObjectRequest>(r =>
                    r.BucketName == Config.RecordingsBucketName &&
                    r.Key == $"{recordingId}/{fileName}" &&
                    r.InputStream != null),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected PutObjectAsync called once with BucketName={Config.RecordingsBucketName} and Key={recordingId}/{fileName}");

        ClientMock.VerifyGet(c => c.Config, Times.Once,
            "Expected Client.Config to be accessed once when building the storage key URL on success");
        ClientMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should delete temp file even when upload throws an exception (finally block)")]
    public void ShouldDeleteTempFileEvenWhenUploadThrows()
    {
        // Given: a valid stream and fileName; S3 throws an exception during PutObjectAsync
        string recordingId = Guid.NewGuid().ToString();
        string fileName = "yoga-exception.mp4";
        Stream fileStream = new MemoryStream(new byte[] { 10, 20, 30 });
        string expectedTempPath = Path.Combine(Path.GetTempPath(), fileName);

        ClientMock
            .Setup(c => c.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("S3 upload failed"));

        // When / Then: the exception should propagate
        Assert.That(
            async () => await Repository.UploadAsync(recordingId, fileStream, fileName, CancellationToken.None),
            Throws.TypeOf<AmazonS3Exception>(),
            "Expected AmazonS3Exception to propagate from UploadAsync");

        // And: the temp file must have been cleaned up in the finally block
        Assert.That(File.Exists(expectedTempPath), Is.False,
            $"Expected temp file '{expectedTempPath}' to be deleted in finally block even when upload throws, but it still exists");
    }

    [Test]
    [DisplayName("Should return Result.Success with storage key when S3 responds HTTP 200")]
    public async Task ShouldReturnSuccessWithStorageKeyWhenS3RespondsOk()
    {
        // Given: a valid stream, recordingId and fileName; S3 returns HTTP 200
        string recordingId = "test-recording-id";
        string fileName = "taller.mp4";
        Stream fileStream = new MemoryStream(new byte[] { 7, 8, 9 });
        string expectedStorageKey =
            $"https://{Config.RecordingsBucketName}.s3.{RegionEndpoint.USEast1.SystemName}.amazonaws.com/{recordingId}/{fileName}";

        ClientMock
            .Setup(c => c.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

        // When: UploadAsync is called
        Result<string> result = await Repository.UploadAsync(recordingId, fileStream, fileName, CancellationToken.None);

        // Then: result should be success and value should be the expected storage URL
        Assert.That(result.IsSuccess, Is.True,
            $"Expected Result.Success but got failure: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Is.EqualTo(expectedStorageKey),
            $"Expected storage key '{expectedStorageKey}' but got: '{result.Value}'");

        ClientMock.Verify(
            c => c.PutObjectAsync(
                It.Is<PutObjectRequest>(r =>
                    r.BucketName == Config.RecordingsBucketName &&
                    r.Key == $"{recordingId}/{fileName}" &&
                    r.InputStream != null),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected PutObjectAsync called once with BucketName={Config.RecordingsBucketName} and Key={recordingId}/{fileName}");

        ClientMock.VerifyGet(c => c.Config, Times.Once,
            "Expected Client.Config to be accessed once when building the storage key URL on success");
        ClientMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return Result.Failure when S3 responds with non-200 HTTP status")]
    public async Task ShouldReturnFailureWhenS3RespondsWithNonOkStatus()
    {
        // Given: a valid stream, recordingId and fileName; S3 returns HTTP 500
        string recordingId = "test-recording-id-fail";
        string fileName = "error.mp4";
        Stream fileStream = new MemoryStream(new byte[] { 1, 2, 3 });

        ClientMock
            .Setup(c => c.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.InternalServerError });

        // When: UploadAsync is called
        Result<string> result = await Repository.UploadAsync(recordingId, fileStream, fileName, CancellationToken.None);

        // Then: result should be failure
        Assert.That(result.IsSuccess, Is.False,
            $"Expected Result.Failure when S3 responds with 500, but got success with value: '{(result.IsSuccess ? result.Value : "N/A")}'");
        Assert.That(result.IsFailure, Is.True,
            "Expected IsFailure=true when S3 responds with InternalServerError");

        ClientMock.Verify(
            c => c.PutObjectAsync(
                It.Is<PutObjectRequest>(r =>
                    r.BucketName == Config.RecordingsBucketName &&
                    r.Key == $"{recordingId}/{fileName}"),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected PutObjectAsync to be called once with BucketName={Config.RecordingsBucketName} and Key={recordingId}/{fileName}");

        ClientMock.VerifyNoOtherCalls();
    }
}
