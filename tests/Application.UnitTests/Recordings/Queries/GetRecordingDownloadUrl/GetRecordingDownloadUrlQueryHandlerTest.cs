using System.ComponentModel;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Recordings.Queries.GetRecordingDownloadUrl;
using VibraHeka.Domain.Recordings.Errors;

namespace VibraHeka.Application.UnitTests.Recordings.Queries.GetRecordingDownloadUrl;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetRecordingDownloadUrlQueryHandlerTest : GenericGetRecordingDownloadUrlTest
{
    #region Happy Path Tests

    [Test]
    [DisplayName("Should return success with download URL when recording exists and storage succeeds")]
    public async Task ShouldReturnSuccessWithDownloadUrlWhenRecordingExistsAndStorageSucceeds()
    {
        // Given: a valid query, registry returns recording, storage returns pre-signed URL
        string recordingId = Guid.NewGuid().ToString();
        string storageKey = $"https://bucket.s3.us-east-1.amazonaws.com/{recordingId}/meditacion.mp4";
        string downloadUrl = "https://pre-signed-url.example.com/download?token=abc";

        GetRecordingDownloadUrlQuery query = BuildValidQuery(recordingId);

        RegistryPortMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(BuildRecordingEntity(recordingId, storageKey)));

        StoragePortMock
            .Setup(s => s.GetDownloadUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(downloadUrl));

        // When: the handler processes the query
        Result<RecordingDownloadUrlDto> result = await Handler.Handle(query, CancellationToken.None);

        // Then: result should be success with the expected download URL
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.DownloadUrl, Is.EqualTo(downloadUrl),
            $"Expected DownloadUrl '{downloadUrl}' but got '{result.Value.DownloadUrl}'");


        RegistryPortMock.Verify(
            r => r.GetByIdAsync(
                It.Is<string>(id => id == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByIdAsync to be called exactly once with the correct recording ID");

        StoragePortMock.Verify(
            s => s.GetDownloadUrlAsync(
                It.Is<string>(key => key == storageKey),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetDownloadUrlAsync to be called exactly once with the recording's storage key");

        RegistryPortMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();
    }

    #endregion



    #region Railway Pattern Tests

    [Test]
    [DisplayName("Should return REC-001 failure and not call StoragePort when recording is not found")]
    public async Task ShouldReturnREC001FailureAndNotCallStoragePortWhenRecordingNotFound()
    {
        // Given: validation passes but registry returns REC-001 (not found)
        string recordingId = Guid.NewGuid().ToString();
        GetRecordingDownloadUrlQuery query = BuildValidQuery(recordingId);

        RegistryPortMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Failure<Domain.Recordings.Entities.RecordingEntity>(RecordingErrors.NotFound));

        // When: the handler processes the query
        Result<RecordingDownloadUrlDto> result = await Handler.Handle(query, CancellationToken.None);

        // Then: result should be failure with REC-001 and StoragePort should never be called (Railway)
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when recording not found, but got success");
        Assert.That(result.Error, Is.EqualTo(RecordingErrors.NotFound),
            $"Expected error '{RecordingErrors.NotFound}' but got '{result.Error}'");


        RegistryPortMock.Verify(
            r => r.GetByIdAsync(
                It.Is<string>(id => id == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByIdAsync to be called once");

        StoragePortMock.Verify(
            s => s.GetDownloadUrlAsync(
                It.Is<string>(_ => true),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Never,
            "Expected GetDownloadUrlAsync to never be called when registry returns not found (Railway)");

        RegistryPortMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should not call StoragePort when RegistryPort fails with generic error (Railway)")]
    public async Task ShouldNotCallStoragePortWhenRegistryPortFailsWithGenericError()
    {
        // Given: validation passes but registry returns a generic persistence error
        string recordingId = Guid.NewGuid().ToString();
        string genericError = "GPE-999";
        GetRecordingDownloadUrlQuery query = BuildValidQuery(recordingId);


        RegistryPortMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Failure<Domain.Recordings.Entities.RecordingEntity>(genericError));

        // When: the handler processes the query
        Result<RecordingDownloadUrlDto> result = await Handler.Handle(query, CancellationToken.None);

        // Then: result should be failure with the original error (not REC-001) and StoragePort never called
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when RegistryPort fails");
        Assert.That(result.Error, Is.EqualTo(genericError),
            $"Expected error '{genericError}' but got '{result.Error}'");


        RegistryPortMock.Verify(
            r => r.GetByIdAsync(
                It.Is<string>(id => id == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByIdAsync to be called once even when it returns failure");

        StoragePortMock.Verify(
            s => s.GetDownloadUrlAsync(
                It.Is<string>(_ => true),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Never,
            "Expected GetDownloadUrlAsync to never be called when RegistryPort fails (Railway pattern)");

        RegistryPortMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return failure when StoragePort fails after registry succeeds")]
    public async Task ShouldReturnFailureWhenStoragePortFailsAfterRegistrySucceeds()
    {
        // Given: validation and registry succeed, but storage fails
        string recordingId = Guid.NewGuid().ToString();
        string storageKey = $"recordings/{recordingId}/taller.mp4";
        string storageError = "S3_PRESIGN_FAILED";
        GetRecordingDownloadUrlQuery query = BuildValidQuery(recordingId);



        RegistryPortMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(BuildRecordingEntity(recordingId, storageKey)));

        StoragePortMock
            .Setup(s => s.GetDownloadUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>(storageError));

        // When: the handler processes the query
        Result<RecordingDownloadUrlDto> result = await Handler.Handle(query, CancellationToken.None);

        // Then: result should be failure with the storage error propagated
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when storage fails, but got success");
        Assert.That(result.Error, Is.EqualTo(storageError),
            $"Expected error '{storageError}' but got '{result.Error}'");

        RegistryPortMock.Verify(
            r => r.GetByIdAsync(
                It.Is<string>(id => id == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByIdAsync to be called exactly once");

        StoragePortMock.Verify(
            s => s.GetDownloadUrlAsync(
                It.Is<string>(key => key == storageKey),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetDownloadUrlAsync to be called once with the recording's storage key even on failure");

        RegistryPortMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();
    }

    #endregion

    #region Logging Tests

    [Test]
    [DisplayName("Should log warning when registry port fails to retrieve recording")]
    public async Task ShouldLogWarningWhenRegistryPortFails()
    {
        // Given: validation passes but registry returns not-found failure
        string recordingId = Guid.NewGuid().ToString();
        GetRecordingDownloadUrlQuery query = BuildValidQuery(recordingId);



        RegistryPortMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Failure<Domain.Recordings.Entities.RecordingEntity>(RecordingErrors.NotFound));

        // When: the handler processes the query
        Result<RecordingDownloadUrlDto> result = await Handler.Handle(query, CancellationToken.None);

        // Then: result is failure and a Warning is logged containing the error code
        Assert.That(result.IsFailure, Is.True,
            "Expected failure result when RegistryPort fails");

        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(RecordingErrors.NotFound)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            $"Expected a Warning log containing '{RecordingErrors.NotFound}' when RegistryPort fails");

        RegistryPortMock.Verify(
            r => r.GetByIdAsync(
                It.Is<string>(id => id == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByIdAsync to be called once");

        StoragePortMock.Verify(
            s => s.GetDownloadUrlAsync(
                It.Is<string>(_ => true),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Never,
            "Expected GetDownloadUrlAsync to never be called when RegistryPort fails");

        RegistryPortMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();
    }

    #endregion
}

