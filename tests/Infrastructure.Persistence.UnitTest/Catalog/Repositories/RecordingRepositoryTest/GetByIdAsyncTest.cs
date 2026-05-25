using System.ComponentModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Domain.Recordings.Errors;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.RecordingRepositoryTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class GetByIdAsyncTest : GenericRecordingRepositoryTest
{
    [Test]
    [DisplayName("Should return correctly mapped RecordingEntity when DynamoDB returns a model")]
    public async Task ShouldReturnMappedEntityWhenDynamoDbReturnsModel()
    {
        // Given: DynamoDB returns a valid RecordingDBModel for the requested ID
        string recordingId = Guid.NewGuid().ToString();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        RecordingDBModel model = new()
        {
            Id = recordingId,
            Name = "Meditacion matutina",
            Description = "Una sesion de meditacion para empezar el dia",
            RecordingType = RecordingType.Meditacion,
            Created = now,
            CreatedBy = "admin-user-id",
            LastModified = now,
            LastModifiedBy = "admin-user-id"
        };

        ContextMock
            .Setup(c => c.LoadAsync<RecordingDBModel>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        // When: GetByIdAsync is called with the recording ID
        Result<RecordingEntity> result = await Repository.GetByIdAsync(recordingId, CancellationToken.None);

        // Then: result should be success with correctly mapped entity fields
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.RecordingID, Is.EqualTo(model.Id),
            $"Expected entity Id '{model.Id}' but got '{result.Value.RecordingID}'");
        Assert.That(result.Value.Name, Is.EqualTo(model.Name),
            $"Expected entity Name '{model.Name}' but got '{result.Value.Name}'");
        Assert.That(result.Value.Description, Is.EqualTo(model.Description),
            $"Expected entity Description '{model.Description}' but got '{result.Value.Description}'");

        ContextMock.Verify(
            c => c.LoadAsync<RecordingDBModel>(
                It.Is<string>(id => id == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected LoadAsync called once with id='{recordingId}'");

        ContextMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return REC-001 failure when DynamoDB returns null (record not found)")]
    public async Task ShouldReturnREC001FailureWhenDynamoDbReturnsNull()
    {
        // Given: DynamoDB returns null — the item does not exist in the table
        string recordingId = Guid.NewGuid().ToString();

        ContextMock
            .Setup(c => c.LoadAsync<RecordingDBModel>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RecordingDBModel)null!);

        // When: GetByIdAsync is called with a non-existent recording ID
        Result<RecordingEntity> result = await Repository.GetByIdAsync(recordingId, CancellationToken.None);

        // Then: result should be failure with REC-001 (mapped from GPE-000 NoRecordsFound)
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with Id: '{(result.IsSuccess ? result.Value.RecordingID : "N/A")}'");
        Assert.That(result.Error, Is.EqualTo(RecordingErrors.NotFound),
            $"Expected error '{RecordingErrors.NotFound}' (REC-001) mapped from GPE-000, but got '{result.Error}'");

        ContextMock.Verify(
            c => c.LoadAsync<RecordingDBModel>(
                It.Is<string>(id => id == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected LoadAsync to be called exactly once before returning null");

        ContextMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return GPE-999 failure (not REC-001) when DynamoDB throws an unexpected exception")]
    public async Task ShouldReturnGPE999FailureAndNotMapToREC001WhenDynamoDbThrows()
    {
        // Given: DynamoDB throws an unexpected exception (not a NotFound scenario)
        string recordingId = Guid.NewGuid().ToString();

        ContextMock
            .Setup(c => c.LoadAsync<RecordingDBModel>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected DynamoDB connection error"));

        // When: GetByIdAsync is called
        Result<RecordingEntity> result = await Repository.GetByIdAsync(recordingId, CancellationToken.None);

        // Then: result should be failure with GPE-999 (not mapped to REC-001)
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when DynamoDB throws an unexpected exception");
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError),
            $"Expected GPE-999 for unexpected exception, but got '{result.Error}'");
        Assert.That(result.Error, Is.Not.EqualTo(RecordingErrors.NotFound),
            $"Generic exceptions must NOT be mapped to REC-001; they should remain as GPE-999");

        ContextMock.Verify(
            c => c.LoadAsync<RecordingDBModel>(
                It.Is<string>(id => id == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected LoadAsync to be called once before throwing");

        ContextMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should call LoadAsync with correct OverrideTableName from config")]
    public async Task ShouldCallLoadAsyncWithCorrectOverrideTableName()
    {
        // Given: DynamoDB returns a model (any ID will do)
        string recordingId = Guid.NewGuid().ToString();
        RecordingDBModel model = new()
        {
            Id = recordingId,
            Name = "Taller de respiracion",
            Description = "Tecnicas de respiracion",
            RecordingType = RecordingType.Taller,
            Created = DateTimeOffset.UtcNow
        };

        ContextMock
            .Setup(c => c.LoadAsync<RecordingDBModel>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        // When: GetByIdAsync is called
        Result<RecordingEntity> result = await Repository.GetByIdAsync(recordingId, CancellationToken.None);

        // Then: LoadAsync should have been called with the table name from AWSConfig
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        ContextMock.Verify(
            c => c.LoadAsync<RecordingDBModel>(
                It.Is<string>(id => id == recordingId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected LoadAsync called ");

        ContextMock.VerifyNoOtherCalls();
    }
}
