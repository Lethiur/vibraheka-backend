using System.ComponentModel;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Recordings.Queries.GetRecordingDownloadUrl;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.Recordings.Errors;

namespace VibraHeka.Application.UnitTests.Catalog.Queries.GetRecordingDownloadUrl;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetRecordingDownloadUrlQueryHandlerTest : GenericGetRecordingDownloadUrlTest
{
    #region CA1 — Free recording + storage OK

    [Test]
    [DisplayName("Should return success with download URL when free recording exists and storage succeeds")]
    public async Task ShouldReturnSuccessWithDownloadUrlWhenFreeRecordingExistsAndStorageSucceeds()
    {
        // Given: a free recording and a presigned download URL in storage
        string recordingId = Guid.NewGuid().ToString();
        string downloadUrl = "https://pre-signed-url.example.com/download?token=abc";

        GetRecordingDownloadUrlQuery query = BuildValidQuery(recordingId);

        RegistryPortMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(BuildFreeRecordingEntity(recordingId)));

        StoragePortMock
            .Setup(s => s.GetDownloadUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(downloadUrl));

        // When: the handler processes the query
        Result<RecordingDownloadUrlDto> result = await Handler.Handle(query, CancellationToken.None);

        // Then: success with the expected URL and subscription service is never consulted
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
                It.Is<string>(key => key == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetDownloadUrlAsync to be called exactly once with the recording ID");

        RegistryPortMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();

        // ISubscriptionService must NOT be called for a free recording
        SubscriptionServiceMock.VerifyNoOtherCalls();

        // ICurrentUserService.UserId must NOT be accessed for a free recording
        CurrentUserServiceMock.VerifyNoOtherCalls();
    }

    #endregion

    #region CA2 — Premium recording + active subscription + storage OK

    [Test]
    [DisplayName("Should return success with download URL when premium recording has active subscription and storage succeeds")]
    public async Task ShouldReturnSuccessWithDownloadUrlWhenPremiumRecordingHasActiveSubscriptionAndStorageSucceeds()
    {
        // Given: a premium recording, an active subscription for the current user, and a presigned URL
        string recordingId = Guid.NewGuid().ToString();
        string downloadUrl = "https://pre-signed-url.example.com/premium?token=xyz";

        GetRecordingDownloadUrlQuery query = BuildValidQuery(recordingId);

        RegistryPortMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(BuildPremiumRecordingEntity(recordingId)));

        SubscriptionServiceMock
            .Setup(s => s.GetSubscriptionForUser(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(BuildActiveSubscriptionEntity()));

        StoragePortMock
            .Setup(s => s.GetDownloadUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(downloadUrl));

        // When: the handler processes the query
        Result<RecordingDownloadUrlDto> result = await Handler.Handle(query, CancellationToken.None);

        // Then: success, subscription consulted with the current user ID, storage called once
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.DownloadUrl, Is.EqualTo(downloadUrl),
            $"Expected DownloadUrl '{downloadUrl}' but got '{result.Value.DownloadUrl}'");

        RegistryPortMock.Verify(
            r => r.GetByIdAsync(
                It.Is<string>(id => id == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByIdAsync to be called exactly once");

        SubscriptionServiceMock.Verify(
            s => s.GetSubscriptionForUser(
                It.Is<string>(id => id == UserId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected GetSubscriptionForUser to be called exactly once with userId '{UserId}'");

        StoragePortMock.Verify(
            s => s.GetDownloadUrlAsync(
                It.Is<string>(key => key == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetDownloadUrlAsync to be called exactly once with the recording ID");

        RegistryPortMock.VerifyNoOtherCalls();
        SubscriptionServiceMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();

        CurrentUserServiceMock.VerifyGet(
            s => s.UserId,
            Times.Once(),
            $"Expected UserId to be accessed exactly once for the premium subscription check");
        CurrentUserServiceMock.VerifyNoOtherCalls();
    }

    #endregion

    #region CA3 — Premium recording + subscription service failure

    [Test]
    [DisplayName("Should return subscription failure and not call StoragePort when subscription service fails for premium recording")]
    public async Task ShouldReturnSubscriptionFailureAndNotCallStoragePortWhenSubscriptionServiceFailsForPremiumRecording()
    {
        // Given: a premium recording but the subscription service returns a failure
        string recordingId = Guid.NewGuid().ToString();
        string subscriptionError = SubscriptionErrors.NoSubscriptionFound;

        GetRecordingDownloadUrlQuery query = BuildValidQuery(recordingId);

        RegistryPortMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(BuildPremiumRecordingEntity(recordingId)));

        SubscriptionServiceMock
            .Setup(s => s.GetSubscriptionForUser(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Domain.Entities.SubscriptionEntity>(subscriptionError));

        // When: the handler processes the query
        Result<RecordingDownloadUrlDto> result = await Handler.Handle(query, CancellationToken.None);

        // Then: failure with the subscription error propagated and storage never called
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when subscription service fails, but got success");
        Assert.That(result.Error, Is.EqualTo(subscriptionError),
            $"Expected error '{subscriptionError}' but got '{result.Error}'");

        RegistryPortMock.Verify(
            r => r.GetByIdAsync(
                It.Is<string>(id => id == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByIdAsync to be called exactly once");

        SubscriptionServiceMock.Verify(
            s => s.GetSubscriptionForUser(
                It.Is<string>(id => id == UserId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected GetSubscriptionForUser to be called exactly once with userId '{UserId}'");

        StoragePortMock.Verify(
            s => s.GetDownloadUrlAsync(
                It.Is<string>(_ => true),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Never,
            "Expected GetDownloadUrlAsync to never be called when subscription service fails (Railway)");

        RegistryPortMock.VerifyNoOtherCalls();
        SubscriptionServiceMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();

        CurrentUserServiceMock.VerifyGet(
            s => s.UserId,
            Times.Once(),
            "Expected UserId to be accessed exactly once for the premium subscription check");
        CurrentUserServiceMock.VerifyNoOtherCalls();
    }

    #endregion

    #region CA4 — Premium recording + inactive subscription

    [Test]
    [DisplayName("Should return failure and not call StoragePort when subscription is inactive for premium recording")]
    public async Task ShouldReturnFailureAndNotCallStoragePortWhenSubscriptionIsInactiveForPremiumRecording()
    {
        // Given: a premium recording but the found subscription is inactive
        string recordingId = Guid.NewGuid().ToString();

        GetRecordingDownloadUrlQuery query = BuildValidQuery(recordingId);

        RegistryPortMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(BuildPremiumRecordingEntity(recordingId)));

        SubscriptionServiceMock
            .Setup(s => s.GetSubscriptionForUser(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(BuildInactiveSubscriptionEntity()));

        // When: the handler processes the query
        Result<RecordingDownloadUrlDto> result = await Handler.Handle(query, CancellationToken.None);

        // Then: failure because inactive subscription is treated as unauthorized access and storage never called
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when subscription is inactive, but got success");

        RegistryPortMock.Verify(
            r => r.GetByIdAsync(
                It.Is<string>(id => id == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByIdAsync to be called exactly once");

        SubscriptionServiceMock.Verify(
            s => s.GetSubscriptionForUser(
                It.Is<string>(id => id == UserId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected GetSubscriptionForUser to be called exactly once with userId '{UserId}'");

        StoragePortMock.Verify(
            s => s.GetDownloadUrlAsync(
                It.Is<string>(_ => true),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Never,
            "Expected GetDownloadUrlAsync to never be called when subscription is inactive");

        RegistryPortMock.VerifyNoOtherCalls();
        SubscriptionServiceMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();

        CurrentUserServiceMock.VerifyGet(
            s => s.UserId,
            Times.Once(),
            "Expected UserId to be accessed exactly once for the premium subscription check");
        CurrentUserServiceMock.VerifyNoOtherCalls();
    }

    #endregion

    #region CA5 — Recording not found (REC-001)

    [Test]
    [DisplayName("Should return REC-001 failure and not call StoragePort when recording is not found")]
    public async Task ShouldReturnREC001FailureAndNotCallStoragePortWhenRecordingNotFound()
    {
        // Given: registry returns REC-001 (not found)
        string recordingId = Guid.NewGuid().ToString();
        GetRecordingDownloadUrlQuery query = BuildValidQuery(recordingId);

        RegistryPortMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Failure<RecordingEntity>(RecordingErrors.NotFound));

        // When: the handler processes the query
        Result<RecordingDownloadUrlDto> result = await Handler.Handle(query, CancellationToken.None);

        // Then: failure with REC-001, subscription and storage never consulted
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
            "Expected GetDownloadUrlAsync to never be called when recording is not found");

        RegistryPortMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();
        SubscriptionServiceMock.VerifyNoOtherCalls();
        CurrentUserServiceMock.VerifyNoOtherCalls();
    }

    #endregion

    #region CA6 — Generic registry error (GPE-999)

    [Test]
    [DisplayName("Should not call StoragePort when RegistryPort fails with generic error (Railway)")]
    public async Task ShouldNotCallStoragePortWhenRegistryPortFailsWithGenericError()
    {
        // Given: registry returns a generic persistence error
        string recordingId = Guid.NewGuid().ToString();
        string genericError = "GPE-999";
        GetRecordingDownloadUrlQuery query = BuildValidQuery(recordingId);

        RegistryPortMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Failure<RecordingEntity>(genericError));

        // When: the handler processes the query
        Result<RecordingDownloadUrlDto> result = await Handler.Handle(query, CancellationToken.None);

        // Then: failure with the original error propagated; subscription and storage never called
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
            "Expected GetDownloadUrlAsync to never be called when RegistryPort fails");

        RegistryPortMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();
        SubscriptionServiceMock.VerifyNoOtherCalls();
        CurrentUserServiceMock.VerifyNoOtherCalls();
    }

    #endregion

    #region CA7 — Registry OK but storage fails

    [Test]
    [DisplayName("Should return failure when StoragePort fails after free recording registry succeeds")]
    public async Task ShouldReturnFailureWhenStoragePortFailsAfterFreeRecordingRegistrySucceeds()
    {
        // Given: registry succeeds with a free recording but storage fails
        string recordingId = Guid.NewGuid().ToString();
        string storageError = "S3_PRESIGN_FAILED";
        GetRecordingDownloadUrlQuery query = BuildValidQuery(recordingId);

        RegistryPortMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(BuildFreeRecordingEntity(recordingId)));

        StoragePortMock
            .Setup(s => s.GetDownloadUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>(storageError));

        // When: the handler processes the query
        Result<RecordingDownloadUrlDto> result = await Handler.Handle(query, CancellationToken.None);

        // Then: failure with the storage error propagated; subscription never consulted
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
                It.Is<string>(key => key == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetDownloadUrlAsync to be called once with the recording ID even when it fails");

        RegistryPortMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();
        SubscriptionServiceMock.VerifyNoOtherCalls();
        CurrentUserServiceMock.VerifyNoOtherCalls();
    }

    #endregion

    #region CA8 — Logging on error paths

    [Test]
    [DisplayName("Should log warning when registry port fails to retrieve recording")]
    public async Task ShouldLogWarningWhenRegistryPortFails()
    {
        // Given: registry returns not-found failure
        string recordingId = Guid.NewGuid().ToString();
        GetRecordingDownloadUrlQuery query = BuildValidQuery(recordingId);

        RegistryPortMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Failure<RecordingEntity>(RecordingErrors.NotFound));

        // When: the handler processes the query
        Result<RecordingDownloadUrlDto> result = await Handler.Handle(query, CancellationToken.None);

        // Then: failure with Warning logged containing the error code
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
        SubscriptionServiceMock.VerifyNoOtherCalls();
        CurrentUserServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should log warning when subscription service fails for premium recording")]
    public async Task ShouldLogWarningWhenSubscriptionServiceFailsForPremiumRecording()
    {
        // Given: premium recording found but subscription service returns failure
        string recordingId = Guid.NewGuid().ToString();
        string subscriptionError = SubscriptionErrors.NoSubscriptionFound;

        GetRecordingDownloadUrlQuery query = BuildValidQuery(recordingId);

        RegistryPortMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(BuildPremiumRecordingEntity(recordingId)));

        SubscriptionServiceMock
            .Setup(s => s.GetSubscriptionForUser(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Domain.Entities.SubscriptionEntity>(subscriptionError));

        // When: the handler processes the query
        Result<RecordingDownloadUrlDto> result = await Handler.Handle(query, CancellationToken.None);

        // Then: failure with Warning logged containing the REC-003 code
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when subscription service fails");

        RegistryPortMock.Verify(
            r => r.GetByIdAsync(
                It.Is<string>(id => id == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByIdAsync to be called once");

        SubscriptionServiceMock.Verify(
            s => s.GetSubscriptionForUser(
                It.Is<string>(id => id == UserId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected GetSubscriptionForUser to be called once with userId '{UserId}'");

        StoragePortMock.Verify(
            s => s.GetDownloadUrlAsync(
                It.Is<string>(_ => true),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Never,
            "Expected GetDownloadUrlAsync to never be called when subscription check fails");

        RegistryPortMock.VerifyNoOtherCalls();
        SubscriptionServiceMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();

        CurrentUserServiceMock.VerifyGet(
            s => s.UserId,
            Times.Once(),
            "Expected UserId to be accessed exactly once for the premium subscription check");
        CurrentUserServiceMock.VerifyNoOtherCalls();
    }

    #endregion
}

