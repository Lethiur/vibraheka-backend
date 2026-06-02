using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Adapters.RecordingsAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class DeleteRecordingAsyncTest : GenericRecordingsAdapterTest
{
    [Test]
    [DisplayName("Should return Result.Success propagated from repository without alteration when delete succeeds")]
    public async Task ShouldReturnSuccessWhenRepositorySucceeds()
    {
        // Given: a valid recording entity and repository returns success on delete
        RecordingEntity recording = BuildDefaultRecordingEntity();

        RepositoryMock
            .Setup(x => x.DeleteRecordingAsync(It.IsAny<RecordingEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // When: DeleteRecordingAsync is called on the adapter
        Result result = await Adapter.DeleteRecordingAsync(recording, CancellationToken.None);

        // Then: result is Success — adapter propagates without alteration
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        RepositoryMock.Verify(
            x => x.DeleteRecordingAsync(
                It.Is<RecordingEntity>(e => e.RecordingID == recording.RecordingID),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected DeleteRecordingAsync called once with recordingId='{recording.RecordingID}'");

        RepositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return Result.Failure propagated from repository without alteration when delete fails")]
    public async Task ShouldReturnFailureWhenRepositoryFails()
    {
        // Given: a valid recording entity and repository returns a generic persistence failure
        RecordingEntity recording = BuildDefaultRecordingEntity();

        RepositoryMock
            .Setup(x => x.DeleteRecordingAsync(It.IsAny<RecordingEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(GenericPersistenceErrors.GeneralError));

        // When: DeleteRecordingAsync is called on the adapter
        Result result = await Adapter.DeleteRecordingAsync(recording, CancellationToken.None);

        // Then: result is Failure — adapter propagates the error without modification
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success");
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError),
            $"Expected error '{GenericPersistenceErrors.GeneralError}' (GPE-999) but got '{result.Error}'");

        RepositoryMock.Verify(
            x => x.DeleteRecordingAsync(
                It.Is<RecordingEntity>(e => e.RecordingID == recording.RecordingID),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected DeleteRecordingAsync called exactly once before returning failure");

        RepositoryMock.VerifyNoOtherCalls();
    }
}
