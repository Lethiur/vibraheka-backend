using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Application.Catalog.Queries.GetAllRecordings;
using VibraHeka.Application.Recordings.Queries.GetAllRecordings;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Application.UnitTests.Catalog.Queries.GetAllRecordings;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public class HandleTest : GenericGetAllRecordingsQueryHandlerTest
{
    [Test]
    [DisplayName("Should return all recording DTOs when registry succeeds")]
    public async Task ShouldReturnAllRecordingsDtosWhenRegistrySucceeds()
    {
        // Given: the registry returns a list of recordings
        DateTimeOffset now = DateTimeOffset.UtcNow;
        IEnumerable<RecordingEntity> entities =
        [
            new RecordingEntity { RecordingID = "id-1", Name = "Meditacion", Description = "Desc1", RecordingType = RecordingType.Meditacion, Created = now, IsActive = true },
            new RecordingEntity { RecordingID = "id-2", Name = "Masterclass", Description = "Desc2", RecordingType = RecordingType.Masterclass, Created = now, IsActive = true },
            new RecordingEntity { RecordingID = "id-3", Name = "Taller", Description = "Desc3", RecordingType = RecordingType.Taller, Created = now, IsActive = true }
        ];

        RegistryPortMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entities));

        // When: the handler processes the query
        Result<IEnumerable<RecordingDto>> result = await Handler.Handle(new GetAllRecordingsQuery(), CancellationToken.None);

        // Then: all three recordings are returned as DTOs
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        List<RecordingDto> dtos = result.Value.ToList();
        Assert.That(dtos, Has.Count.EqualTo(3),
            $"Expected 3 DTOs but got {dtos.Count}");
        Assert.That(dtos.Select(d => d.Id), Is.EquivalentTo(new[] { "id-1", "id-2", "id-3" }),
            "Expected DTOs to contain the same IDs as the source entities");

        RegistryPortMock.Verify(
            x => x.GetAllAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetAllAsync to be called exactly once");

        RegistryPortMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return empty list when registry returns no recordings")]
    public async Task ShouldReturnEmptyListWhenRegistryReturnsNoRecordings()
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
    [DisplayName("Should return failure when registry port fails")]
    public async Task ShouldReturnFailureWhenRegistryPortFails()
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
    [DisplayName("Should correctly map entity fields to DTO")]
    public async Task ShouldCorrectlyMapEntityFieldsToDto()
    {
        // Given: a single recording entity with all fields populated
        DateTimeOffset now = new(2025, 6, 10, 8, 0, 0, TimeSpan.Zero);
        RecordingEntity entity = new()
        {
            ID = "map-id-1",
            Name = "Meditacion matutina",
            Description = "Sesion guiada",
            RecordingType = RecordingType.Meditacion,
            Created = now,
            IsActive = true
        };

        RegistryPortMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IEnumerable<RecordingEntity>>([entity]));

        // When: the handler processes the query
        Result<IEnumerable<RecordingDto>> result = await Handler.Handle(new GetAllRecordingsQuery(), CancellationToken.None);

        // Then: the DTO fields match the source entity
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        RecordingDto dto = result.Value.Single();
        Assert.That(dto.Id, Is.EqualTo(entity.RecordingID),
            $"Expected Id '{entity.RecordingID}' but got '{dto.Id}'");
        Assert.That(dto.Name, Is.EqualTo(entity.Name),
            $"Expected Name '{entity.Name}' but got '{dto.Name}'");
        Assert.That(dto.Description, Is.EqualTo(entity.Description),
            $"Expected Description '{entity.Description}' but got '{dto.Description}'");
        Assert.That(dto.Type, Is.EqualTo(entity.RecordingType),
            $"Expected Type '{entity.RecordingType}' but got '{dto.Type}'");

        RegistryPortMock.Verify(
            x => x.GetAllAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetAllAsync to be called exactly once");

        RegistryPortMock.VerifyNoOtherCalls();
    }
}



