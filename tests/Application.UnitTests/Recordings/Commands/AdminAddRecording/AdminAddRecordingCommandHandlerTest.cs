using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Recordings.Commnads.AdminAddRecording;
using VibraHeka.Application.Recordings.Entities;
using VibraHeka.Domain.Recordings.Errors;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.UnitTests.Recordings.Commands.AdminAddRecording;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class AdminAddRecordingCommandHandlerTest : GenericAdminAddRecordingTest
{
    #region Happy Path Tests

    [Test]
    [DisplayName("Should return success with RecordingId and UploadUrl when registry save and storage URL generation succeed")]
    public async Task ShouldReturnSuccessWithRecordingIdAndUploadUrlWhenBothPortsSucceed()
    {
        // Given: a valid command, registry save succeeds and storage returns a pre-signed URL
        AdminAddRecordingCommand command = BuildValidCommand();
        string expectedUploadUrl = "https://bucket.s3.amazonaws.com/recordings/id/file.mp4?X-Amz-Signature=abc";

        RegistryPortMock
            .Setup(x => x.SaveRecording(It.IsAny<Domain.Recordings.Entities.RecordingEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("saved"));

        StoragePortMock
            .Setup(x => x.GetUploadUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedUploadUrl));

        // When: the handler processes the command
        Result<AddRecordingResult> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result is success with a non-empty RecordingId and the expected UploadUrl
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.RecordingId, Is.Not.Null.And.Not.Empty,
            $"Expected a non-empty RecordingId but got: '{result.Value.RecordingId}'");
        Assert.That(result.Value.UploadUrl, Is.EqualTo(expectedUploadUrl),
            $"Expected UploadUrl '{expectedUploadUrl}' but got '{result.Value.UploadUrl}'");

        RegistryPortMock.Verify(
            x => x.SaveRecording(
                It.Is<Domain.Recordings.Entities.RecordingEntity>(e =>
                    e.Name == command.Name &&
                    e.Description == command.Description &&
                    e.Type == command.Type &&
                    !string.IsNullOrEmpty(e.Id)),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected SaveRecording to be called once with an entity matching the command");

        StoragePortMock.Verify(
            x => x.GetUploadUrlAsync(
                It.Is<string>(key => key.Contains(command.FileName)),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetUploadUrlAsync to be called once with the storage key containing the FileName");

        RegistryPortMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();
    }

    #endregion

    #region Railway Pattern — Registry Failure

    [Test]
    [DisplayName("Should return failure and not call StoragePort when RegistryPort save fails")]
    public async Task ShouldReturnFailureAndNotCallStoragePortWhenRegistrySaveFails()
    {
        // Given: a valid command but the registry fails to persist
        AdminAddRecordingCommand command = BuildValidCommand();
        string registryError = RecordingErrors.UploadFailed;

        RegistryPortMock
            .Setup(x => x.SaveRecording(It.IsAny<Domain.Recordings.Entities.RecordingEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>(registryError));

        // When: the handler processes the command
        Result<AddRecordingResult> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result is failure with the registry error and StoragePort is never called (railway short-circuit)
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when RegistryPort save fails but got success");
        Assert.That(result.Error, Is.EqualTo(registryError),
            $"Expected error '{registryError}' but got '{result.Error}'");

        RegistryPortMock.Verify(
            x => x.SaveRecording(
                It.Is<Domain.Recordings.Entities.RecordingEntity>(e => e.Name == command.Name),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected SaveRecording to be called once even on failure");

        StoragePortMock.Verify(
            x => x.GetUploadUrlAsync(
                It.Is<string>(_ => true),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Never,
            "Expected GetUploadUrlAsync to never be called when RegistryPort save fails (railway pattern)");

        RegistryPortMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();
    }

    #endregion

    #region Railway Pattern — Storage Failure

    [Test]
    [DisplayName("Should return failure when RegistryPort save succeeds but StoragePort URL generation fails")]
    public async Task ShouldReturnFailureWhenRegistrySaveSuceedsButStorageUrlGenerationFails()
    {
        // Given: registry save succeeds but storage URL generation fails
        AdminAddRecordingCommand command = BuildValidCommand();
        string storageError = RecordingErrors.UrlGenerationFailed;

        RegistryPortMock
            .Setup(x => x.SaveRecording(It.IsAny<Domain.Recordings.Entities.RecordingEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("saved"));

        StoragePortMock
            .Setup(x => x.GetUploadUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>(storageError));

        // When: the handler processes the command
        Result<AddRecordingResult> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result is failure with the storage error
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when StoragePort URL generation fails but got success");
        Assert.That(result.Error, Is.EqualTo(storageError),
            $"Expected error '{storageError}' but got '{result.Error}'");

        RegistryPortMock.Verify(
            x => x.SaveRecording(
                It.Is<Domain.Recordings.Entities.RecordingEntity>(e => e.Name == command.Name),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected SaveRecording to be called once");

        StoragePortMock.Verify(
            x => x.GetUploadUrlAsync(
                It.Is<string>(key => key.Contains(command.FileName)),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetUploadUrlAsync to be called once even when it fails");

        RegistryPortMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();
    }

    #endregion

    #region Entity Construction Tests

    [Test]
    [DisplayName("Should set StorageKey using recordings prefix, generated ID, and FileName")]
    public async Task ShouldSetStorageKeyWithCorrectPrefixIdAndFileName()
    {
        // Given: a valid command
        AdminAddRecordingCommand command = BuildValidCommand();
        Domain.Recordings.Entities.RecordingEntity? capturedEntity = null;

        RegistryPortMock
            .Setup(x => x.SaveRecording(It.IsAny<Domain.Recordings.Entities.RecordingEntity>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Recordings.Entities.RecordingEntity, CancellationToken>((e, _) => capturedEntity = e)
            .ReturnsAsync(Result.Success("saved"));

        StoragePortMock
            .Setup(x => x.GetUploadUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("https://presigned-url.example.com/upload"));

        // When: the handler processes the command
        Result<AddRecordingResult> result = await Handler.Handle(command, CancellationToken.None);

        // Then: the entity StorageKey follows the pattern "recordings/{id}/{fileName}"
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(capturedEntity, Is.Not.Null,
            "Expected the entity to have been captured by the SaveRecording callback");

        RegistryPortMock.Verify(
            x => x.SaveRecording(
                It.Is<Domain.Recordings.Entities.RecordingEntity>(e => e.Name == command.Name),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected SaveRecording to be called once with the correct entity");

        StoragePortMock.Verify(
            x => x.GetUploadUrlAsync(
                It.Is<string>(key => key == capturedEntity.Id),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetUploadUrlAsync to be called once with the captured storage key");

        RegistryPortMock.VerifyNoOtherCalls();
        StoragePortMock.VerifyNoOtherCalls();
    }

    #endregion
}



