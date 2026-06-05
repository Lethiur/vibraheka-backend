using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Recordings.Errors;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Adapters.RecordingsAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetByIdAsyncTest : GenericRecordingsAdapterTest
{
    [Test]
    [DisplayName("Should return Result.Success propagated from repository without alteration when recording is found")]
    public async Task ShouldReturnSuccessWhenRepositoryReturnsEntity()
    {
        // Given: repository returns a valid recording entity for the requested recordingId
        string recordingId = "recording-id-adapter-success-001";
        RecordingEntity entity = BuildDefaultRecordingEntity(recordingId);

        RepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entity));

        // When: GetByIdAsync is called on the adapter
        Result<RecordingEntity> result = await Adapter.GetByIdAsync(recordingId, CancellationToken.None);

        // Then: result is Success with entity data matching — adapter propagates without alteration
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.RecordingID, Is.EqualTo(recordingId),
            $"Expected RecordingID '{recordingId}' but got '{result.Value.RecordingID}'");
        Assert.That(result.Value.Name, Is.EqualTo(entity.Name),
            $"Expected Name '{entity.Name}' but got '{result.Value.Name}'");

        RepositoryMock.Verify(
            x => x.GetByIdAsync(
                It.Is<string>(id => id == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected GetByIdAsync called once with recordingId='{recordingId}'");

        RepositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return Result.Failure propagated from repository without alteration when recording is not found")]
    public async Task ShouldReturnFailureWhenRepositoryReturnsNotFound()
    {
        // Given: repository returns REC-001 failure — no entity exists for the requested recordingId
        string recordingId = "recording-id-adapter-notfound-002";

        RepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<RecordingEntity>(RecordingErrors.NotFound));

        // When: GetByIdAsync is called on the adapter
        Result<RecordingEntity> result = await Adapter.GetByIdAsync(recordingId, CancellationToken.None);

        // Then: result is Failure with the REC-001 error — adapter does not modify it
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with RecordingID: '{(result.IsSuccess ? result.Value.RecordingID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(RecordingErrors.NotFound),
            $"Expected error '{RecordingErrors.NotFound}' (REC-001) but got '{result.Error}'");

        RepositoryMock.Verify(
            x => x.GetByIdAsync(
                It.Is<string>(id => id == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByIdAsync called exactly once before returning not-found failure");

        RepositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return Result.Failure propagated from repository without alteration when a generic persistence error occurs")]
    public async Task ShouldReturnFailureWhenRepositoryReturnsGenericError()
    {
        // Given: repository returns GPE-999 (general persistence error)
        string recordingId = "recording-id-adapter-gpe-003";

        RepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<RecordingEntity>(GenericPersistenceErrors.GeneralError));

        // When: GetByIdAsync is called on the adapter
        Result<RecordingEntity> result = await Adapter.GetByIdAsync(recordingId, CancellationToken.None);

        // Then: result is Failure with GPE-999 propagated — adapter does not modify it
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with RecordingID: '{(result.IsSuccess ? result.Value.RecordingID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError),
            $"Expected error '{GenericPersistenceErrors.GeneralError}' (GPE-999) but got '{result.Error}'");

        RepositoryMock.Verify(
            x => x.GetByIdAsync(
                It.Is<string>(id => id == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByIdAsync called exactly once before returning general error failure");

        RepositoryMock.VerifyNoOtherCalls();
    }
}
