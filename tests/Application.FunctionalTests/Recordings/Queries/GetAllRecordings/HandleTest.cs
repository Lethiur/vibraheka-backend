using System.ComponentModel;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Moq;
using VibraHeka.Application.Recordings.Queries.GetAllRecordings;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Application.FunctionalTests.Recordings.Queries.GetAllRecordings;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class HandleTest : GenericGetAllRecordingsQueryHandlerTest
{
    [Test]
    [DisplayName("Full pipeline: handler returns success with correctly mapped DTOs")]
    public async Task ShouldReturnMappedDtosWhenRegistrySucceeds()
    {
        // Given: the registry returns a list of recordings
        DateTimeOffset now = DateTimeOffset.UtcNow;
        IEnumerable<RecordingEntity> entities =
        [
            new RecordingEntity { Id = "f-id-1", Name = "Meditacion", Description = "Desc1", Type = RecordingType.Meditacion, StorageKey = "key1", Created = now },
            new RecordingEntity { Id = "f-id-2", Name = "Masterclass", Description = "Desc2", Type = RecordingType.Masterclass, StorageKey = "key2", Created = now },
            new RecordingEntity { Id = "f-id-3", Name = "Taller", Description = "Desc3", Type = RecordingType.Taller, StorageKey = "key3", Created = now }
        ];

        RegistryPortMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entities));

        // When: the handler processes the query
        Result<IEnumerable<RecordingDto>> result = await Handler.Handle(new GetAllRecordingsQuery(), CancellationToken.None);

        // Then: all three recordings are returned correctly mapped
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        List<RecordingDto> dtos = result.Value.ToList();
        Assert.That(dtos, Has.Count.EqualTo(3),
            $"Expected 3 DTOs but got {dtos.Count}");
        Assert.That(dtos.Select(d => d.Id), Is.EquivalentTo(new[] { "f-id-1", "f-id-2", "f-id-3" }),
            "Expected DTOs to contain the same IDs as the source entities");
        Assert.That(dtos.Select(d => d.Type).Distinct().Count(), Is.EqualTo(3),
            "Expected each DTO to have a distinct RecordingType matching its entity");

        RegistryPortMock.Verify(
            x => x.GetAllAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetAllAsync to be called exactly once");

        RegistryPortMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Full pipeline: handler returns empty list when no recordings exist")]
    public async Task ShouldReturnEmptyListWhenNoRecordingsExist()
    {
        // Given: the registry has no recordings
        RegistryPortMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Enumerable.Empty<RecordingEntity>()));

        // When: the handler processes the query
        Result<IEnumerable<RecordingDto>> result = await Handler.Handle(new GetAllRecordingsQuery(), CancellationToken.None);

        // Then: result is success with an empty collection
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.Count(), Is.EqualTo(0),
            $"Expected 0 DTOs but got {result.Value.Count()}");

        RegistryPortMock.Verify(
            x => x.GetAllAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetAllAsync to be called exactly once");

        RegistryPortMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Full pipeline: handler propagates failure when registry returns error")]
    public async Task ShouldPropagateFailureWhenRegistryFails()
    {
        // Given: the registry fails with a general persistence error
        string error = GenericPersistenceErrors.GeneralError;
        RegistryPortMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<IEnumerable<RecordingEntity>>(error));

        // When: the handler processes the query
        Result<IEnumerable<RecordingDto>> result = await Handler.Handle(new GetAllRecordingsQuery(), CancellationToken.None);

        // Then: result is failure with the original error message propagated
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with count: '{(result.IsSuccess ? result.Value.Count() : 0)}'");
        Assert.That(result.Error, Is.EqualTo(error),
            $"Expected error '{error}' but got '{result.Error}'");

        RegistryPortMock.Verify(
            x => x.GetAllAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetAllAsync to be called exactly once even on failure");

        RegistryPortMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Full pipeline: handler propagates resource-not-found error from registry")]
    public async Task ShouldPropagateResourceNotFoundErrorWhenRegistryFails()
    {
        // Given: the registry fails with a resource-not-found error
        string error = GenericPersistenceErrors.ResourceNotFound;
        RegistryPortMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<IEnumerable<RecordingEntity>>(error));

        // When: the handler processes the query
        Result<IEnumerable<RecordingDto>> result = await Handler.Handle(new GetAllRecordingsQuery(), CancellationToken.None);

        // Then: result is failure with the resource-not-found error code
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with count: '{(result.IsSuccess ? result.Value.Count() : 0)}'");
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.ResourceNotFound),
            $"Expected error '{GenericPersistenceErrors.ResourceNotFound}' but got '{result.Error}'");

        RegistryPortMock.Verify(
            x => x.GetAllAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetAllAsync to be called exactly once");

        RegistryPortMock.VerifyNoOtherCalls();
    }
}


