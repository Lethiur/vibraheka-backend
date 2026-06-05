using System.ComponentModel;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Recordings.Commnands.DeleteRecording;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Recordings.Errors;

namespace VibraHeka.Application.UnitTests.Catalog.Commands.DeleteRecording;

[TestFixture]
public sealed class DeleteRecordingCommandHandlerTest : GenericDeleteRecordingTest
{
    #region Happy Path Tests

    [Test]
    [DisplayName("Should return success when recording exists and both S3 and DynamoDB deletes succeed")]
    public async Task ShouldReturnSuccessWhenRecordingExistsAndBothDeletesSucceed()
    {
        // Given: a valid command with an existing recording entity
        DeleteRecordingCommand command = BuildValidCommand();
        RecordingEntity entity = BuildValidEntity(id: command.RecordingId);

        RegistryPortMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entity));

        StoragePortMock
            .Setup(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        RegistryPortMock
            .Setup(x => x.DeleteRecordingAsync(It.IsAny<RecordingEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // When: the handler processes the command
        Result<Unit> result = await Handler.Handle(command, CancellationToken.None);

        // Then: the result should be success with no error
        Assert.That(
            result.IsSuccess,
            Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        RegistryPortMock.Verify(
            x => x.GetByIdAsync(
                It.Is<string>(id => id == command.RecordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByIdAsync to be called once with the correct recording ID");

        StoragePortMock.Verify(
            x => x.DeleteFileAsync(
                It.Is<string>(key => key == entity.RecordingID),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected DeleteFileAsync to be called once with the entity storage key");

        RegistryPortMock.Verify(
            x => x.DeleteRecordingAsync(
                It.Is<RecordingEntity>(e => e.RecordingID == entity.RecordingID),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected DeleteRecordingAsync to be called once with the correct entity");

        StoragePortMock.VerifyNoOtherCalls();
        RegistryPortMock.VerifyNoOtherCalls();
    }

    #endregion

    #region Not Found Tests

    [Test]
    [DisplayName("Should return failure with REC-001 and call no storage or registry delete when recording does not exist")]
    public async Task ShouldReturnNotFoundFailureAndSkipDeletesWhenRecordingDoesNotExist()
    {
        // Given: a valid command where the recording does not exist in DynamoDB
        DeleteRecordingCommand command = BuildValidCommand();

        RegistryPortMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<RecordingEntity>(RecordingErrors.NotFound));

        // When: the handler processes the command
        Result<Unit> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result should be failure with REC-001 and no storage or delete calls should be made
        Assert.That(
            result.IsFailure,
            Is.True,
            "Expected failure when recording does not exist");

        Assert.That(
            result.Error,
            Is.EqualTo(RecordingErrors.NotFound),
            $"Expected error '{RecordingErrors.NotFound}' but got: '{result.Error}'");

        RegistryPortMock.Verify(
            x => x.GetByIdAsync(
                It.Is<string>(id => id == command.RecordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByIdAsync to be called once");

        StoragePortMock.Verify(
            x => x.DeleteFileAsync(
                It.Is<string>(key => key != null),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Never,
            "Expected DeleteFileAsync to never be called when recording is not found");

        RegistryPortMock.Verify(
            x => x.DeleteRecordingAsync(
                It.Is<RecordingEntity>(e => e != null),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Never,
            "Expected DeleteRecordingAsync to never be called when recording is not found");

        StoragePortMock.VerifyNoOtherCalls();
        RegistryPortMock.VerifyNoOtherCalls();
    }

    #endregion

    #region Railway Pattern Tests

    [Test]
    [DisplayName("Should return infra failure and not call DeleteRecordingAsync when S3 delete fails")]
    public async Task ShouldReturnFailureAndNotCallDynamoDbDeleteWhenS3DeleteFails()
    {
        // Given: the recording exists but S3 delete fails
        DeleteRecordingCommand command = BuildValidCommand();
        RecordingEntity entity = BuildValidEntity(id: command.RecordingId);
        string infraError = "S3_DELETE_FAILED";

        RegistryPortMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entity));

        StoragePortMock
            .Setup(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(infraError));

        // When: the handler processes the command
        Result<Unit> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result is failure with the infra error and DynamoDB delete was NOT called (railway short-circuit)
        Assert.That(
            result.IsFailure,
            Is.True,
            "Expected failure when S3 delete fails");

        Assert.That(
            result.Error,
            Is.EqualTo(infraError),
            $"Expected error '{infraError}' but got: '{result.Error}'");

        RegistryPortMock.Verify(
            x => x.GetByIdAsync(
                It.Is<string>(id => id == command.RecordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByIdAsync to be called once");

        StoragePortMock.Verify(
            x => x.DeleteFileAsync(
                It.Is<string>(key => key == entity.RecordingID),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected DeleteFileAsync to be called once even when it fails");

        RegistryPortMock.Verify(
            x => x.DeleteRecordingAsync(
                It.Is<RecordingEntity>(e => e != null),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Never,
            "Expected DeleteRecordingAsync to never be called when S3 delete fails (railway pattern)");

        StoragePortMock.VerifyNoOtherCalls();
        RegistryPortMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return infra failure when S3 delete succeeds but DynamoDB delete fails (stale record risk)")]
    public async Task ShouldReturnFailureWhenS3DeleteSucceedsButDynamoDbDeleteFails()
    {
        // Given: the recording exists, S3 delete succeeds, but DynamoDB delete fails
        // NOTE: this creates a stale DynamoDB record pointing to a deleted S3 object (known deuda técnica — no compensación implementada)
        DeleteRecordingCommand command = BuildValidCommand();
        RecordingEntity entity = BuildValidEntity(id: command.RecordingId);
        string infraError = "DYNAMODB_DELETE_FAILED";

        RegistryPortMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entity));

        StoragePortMock
            .Setup(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        RegistryPortMock
            .Setup(x => x.DeleteRecordingAsync(It.IsAny<RecordingEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(infraError));

        // When: the handler processes the command
        Result<Unit> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result is failure with infra error (S3 object already deleted — stale record may remain in DynamoDB)
        Assert.That(
            result.IsFailure,
            Is.True,
            "Expected failure when DynamoDB delete fails");

        Assert.That(
            result.Error,
            Is.EqualTo(infraError),
            $"Expected error '{infraError}' but got: '{result.Error}'");

        RegistryPortMock.Verify(
            x => x.GetByIdAsync(
                It.Is<string>(id => id == command.RecordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByIdAsync to be called once");

        StoragePortMock.Verify(
            x => x.DeleteFileAsync(
                It.Is<string>(key => key == entity.RecordingID),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected DeleteFileAsync to be called once");

        RegistryPortMock.Verify(
            x => x.DeleteRecordingAsync(
                It.Is<RecordingEntity>(e => e.RecordingID == entity.RecordingID),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected DeleteRecordingAsync to be called once (S3 already deleted — stale record risk documented)");

        StoragePortMock.VerifyNoOtherCalls();
        RegistryPortMock.VerifyNoOtherCalls();
    }

    #endregion
}


