using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Adapters.RecordingsAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetAllAsyncTest : GenericRecordingsAdapterTest
{
    [Test]
    [DisplayName("Should return Result.Success with recording list propagated from repository without alteration")]
    public async Task ShouldReturnSuccessWhenRepositoryReturnsRecordings()
    {
        // Given: repository returns a list of recordings
        RecordingEntity recording = BuildDefaultRecordingEntity();
        IEnumerable<RecordingEntity> recordings = new List<RecordingEntity> { recording };

        RepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(recordings));

        // When: GetAllAsync is called on the adapter
        Result<IEnumerable<RecordingEntity>> result = await Adapter.GetAllAsync(CancellationToken.None);

        // Then: result is Success with the same recording list — adapter propagates without alteration
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Has.Exactly(1).Items,
            $"Expected 1 recording in list but got {result.Value.Count()}");

        RepositoryMock.Verify(
            x => x.GetAllAsync(
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetAllAsync called once");

        RepositoryMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return Result.Failure propagated from repository without alteration when persistence fails")]
    public async Task ShouldReturnFailureWhenRepositoryFails()
    {
        // Given: repository returns a persistence failure
        RepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<IEnumerable<RecordingEntity>>(GenericPersistenceErrors.GeneralError));

        // When: GetAllAsync is called on the adapter
        Result<IEnumerable<RecordingEntity>> result = await Adapter.GetAllAsync(CancellationToken.None);

        // Then: result is Failure — adapter propagates the error without modification
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with items: '{(result.IsSuccess ? result.Value.Count() : 0)}'");
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError),
            $"Expected error '{GenericPersistenceErrors.GeneralError}' (GPE-999) but got '{result.Error}'");

        RepositoryMock.Verify(
            x => x.GetAllAsync(
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetAllAsync called exactly once before returning failure");

        RepositoryMock.VerifyNoOtherCalls();
    }
}
