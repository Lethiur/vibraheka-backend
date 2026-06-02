using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Adapters.RecordingsAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class SaveRecordingTest : GenericRecordingsAdapterTest
{
    [Test]
    [DisplayName("Should return Result.Success propagated from repository without alteration when save succeeds")]
    public async Task ShouldReturnSuccessWhenRepositorySucceeds()
    {
        // Given: a valid recording entity and repository returns success with the recording ID
        RecordingEntity recording = BuildDefaultRecordingEntity();

        RepositoryMock
            .Setup(x => x.SaveRecording(It.IsAny<RecordingEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(recording.RecordingID));

        // When: SaveRecording is called on the adapter
        Result<string> result = await Adapter.SaveRecording(recording, CancellationToken.None);

        // Then: result is Success with the recording ID — adapter propagates without alteration
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Is.EqualTo(recording.RecordingID),
            $"Expected RecordingID '{recording.RecordingID}' but got '{result.Value}'");

        RepositoryMock.Verify(
            x => x.SaveRecording(
                It.Is<RecordingEntity>(e => e.RecordingID == recording.RecordingID),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected SaveRecording called once with recordingId='{recording.RecordingID}'");

        RepositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return Result.Failure propagated from repository without alteration when save fails")]
    public async Task ShouldReturnFailureWhenRepositoryFails()
    {
        // Given: a valid recording entity and repository returns a generic persistence failure
        RecordingEntity recording = BuildDefaultRecordingEntity();

        RepositoryMock
            .Setup(x => x.SaveRecording(It.IsAny<RecordingEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>(GenericPersistenceErrors.GeneralError));

        // When: SaveRecording is called on the adapter
        Result<string> result = await Adapter.SaveRecording(recording, CancellationToken.None);

        // Then: result is Failure — adapter propagates the error without modification
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with value: '{(result.IsSuccess ? result.Value : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError),
            $"Expected error '{GenericPersistenceErrors.GeneralError}' (GPE-999) but got '{result.Error}'");

        RepositoryMock.Verify(
            x => x.SaveRecording(
                It.Is<RecordingEntity>(e => e.RecordingID == recording.RecordingID),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected SaveRecording called exactly once before returning failure");

        RepositoryMock.VerifyNoOtherCalls();
    }
}
