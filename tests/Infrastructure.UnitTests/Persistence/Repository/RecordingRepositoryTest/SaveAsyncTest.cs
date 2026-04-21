using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.RecordingRepositoryTest;

[TestFixture]
public class SaveAsyncTest : GenericRecordingRepositoryTest
{
    [Test]
    [Description("Should map RecordingEntity to RecordingDBModel and call SaveAsync with correct OverrideTableName")]
    public async Task ShouldMapEntityToModelAndCallSaveAsyncWithCorrectTableName()
    {
        // Given: a valid RecordingEntity with all fields populated
        RecordingEntity entity = new()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Meditacion matutina",
            Description = "Sesion guiada de meditacion para el inicio del dia",
            Type = RecordingType.Meditacion,
            StorageKey = "recordings/abc/meditacion.mp4",
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "admin-user-id",
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = "admin-user-id"
        };

        ContextMock
            .Setup(c => c.SaveAsync(It.IsAny<RecordingDBModel>(), It.IsAny<SaveConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // When: SaveAsync is called with the entity
        Result<string> result = await Repository.SaveRecording(entity, CancellationToken.None);

        // Then: context.SaveAsync should be called once with the correct OverrideTableName and mapped model
        ContextMock.Verify(
            c => c.SaveAsync(
                It.Is<RecordingDBModel>(m =>
                    m.Id == entity.Id &&
                    m.Name == entity.Name &&
                    m.Description == entity.Description &&
                    m.Type == entity.Type &&
                    m.StorageKey == entity.StorageKey &&
                    m.CreatedBy == entity.CreatedBy),
                It.Is<SaveConfig>(s => s.OverrideTableName == Config.RecordingsTable),
                It.IsAny<CancellationToken>()),
            Times.Once,
            $"Expected SaveAsync called once with model mapped from entity.Id={entity.Id} and OverrideTableName={Config.RecordingsTable}");

        ContextMock.VerifyNoOtherCalls();
    }

    [Test]
    [Description("Should return Result.Success with recording Id when DynamoDB does not throw")]
    public async Task ShouldReturnSuccessWithRecordingIdWhenDynamoDbSavesWithoutException()
    {
        // Given: a valid RecordingEntity and a DynamoDB context that completes successfully
        RecordingEntity entity = new()
        {
            Id = "expected-recording-id",
            Name = "Taller de respiracion",
            Description = "Tecnicas avanzadas de respiracion consciente",
            Type = RecordingType.Taller,
            StorageKey = "recordings/expected-recording-id/taller.mp4",
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "admin-user-id",
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = "admin-user-id"
        };

        ContextMock
            .Setup(c => c.SaveAsync(It.IsAny<RecordingDBModel>(), It.IsAny<SaveConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // When: SaveAsync is called
        Result<string> result = await Repository.SaveRecording(entity, CancellationToken.None);

        // Then: result should be success with the entity's Id as value
        Assert.That(result.IsSuccess, Is.True,
            $"Expected Result.Success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Is.EqualTo(entity.Id),
            $"Expected result.Value == '{entity.Id}' but got: '{result.Value}'");

        ContextMock.Verify(
            c => c.SaveAsync(
                It.Is<RecordingDBModel>(m => m.Id == entity.Id && m.Name == entity.Name),
                It.Is<SaveConfig>(s => s.OverrideTableName == Config.RecordingsTable),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected SaveAsync to be called exactly once with the correct model and table name");

        ContextMock.VerifyNoOtherCalls();
    }

    [Test]
    [Description("Should propagate exception when context.SaveAsync throws")]
    public async Task ShouldPropagateExceptionWhenDynamoDbSaveAsyncThrows()
    {
        // Given: a valid RecordingEntity and a DynamoDB context that throws
        RecordingEntity entity = new()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Masterclass de yoga",
            Description = "Clase completa de yoga para principiantes",
            Type = RecordingType.Masterclass,
            StorageKey = "recordings/some-id/yoga.mp4",
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "admin-user-id",
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = "admin-user-id"
        };

        InvalidOperationException expectedException = new("DynamoDB connection failed");

        ContextMock
            .Setup(c => c.SaveAsync(It.IsAny<RecordingDBModel>(), It.IsAny<SaveConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // When / Then: invoking SaveAsync should propagate the exception
        Result<string> saveRecording = await Repository.SaveRecording(entity, CancellationToken.None);

        Assert.That(saveRecording.IsFailure, Is.True, "Expected Result.Failure when SaveAsync throws an exception");

        Assert.That(saveRecording.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError), "Expected specific error message from SaveAsync exception");

        ContextMock.Verify(
            c => c.SaveAsync(
                It.Is<RecordingDBModel>(m => m.Id == entity.Id),
                It.Is<SaveConfig>(s => s.OverrideTableName == Config.RecordingsTable),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected SaveAsync to be called once before throwing");
    }
}
