using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.RecordingRepositoryTest;

public abstract class GenericRecordingRepositoryTest : TestBase
{
    protected RecordingRepository RecordingRepository = default!;
    protected IDynamoDBContext DynamoContext = default!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        base.OneTimeSetUp();
        DynamoContext = CreateDynamoDBContext();
        RecordingRepository = new RecordingRepository(DynamoContext, _configuration);
    }

    [OneTimeTearDown]
    public void OneTimeTearDownContext()
    {
        DynamoContext.Dispose();
    }

    protected async Task CleanupRecording(string recordingId)
    {
        try
        {
            SaveConfig deleteConfig = new() { OverrideTableName = _configuration.RecordingsTable };
            await DynamoContext.DeleteAsync<RecordingDBModel>(recordingId, deleteConfig);
            Console.WriteLine($"Cleanup: Deleted recording {recordingId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not cleanup recording {recordingId}: {ex.Message}");
        }
    }

    protected RecordingEntity CreateValidRecordingEntity()
    {
        return new RecordingEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = _faker.Lorem.Sentence(3),
            Description = _faker.Lorem.Paragraph(),
            Type = RecordingType.Meditacion,
            StorageKey = $"recordings/{Guid.NewGuid()}/audio.mp4",
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "admin-user-id",
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = "admin-user-id"
        };
    }
}

[TestFixture]
public class RecordingRepositoryIntegrationTest : GenericRecordingRepositoryTest
{
    private string? LastCreatedRecordingId;

    [TearDown]
    public async Task TearDown()
    {
        if (LastCreatedRecordingId is not null)
        {
            await CleanupRecording(LastCreatedRecordingId);
            LastCreatedRecordingId = null;
        }
    }

    #region SaveAsync — Success Cases

    [Test]
    [Description("Should return Result.Success with the recording ID when the entity is persisted")]
    public async Task ShouldReturnSuccessWithRecordingIdWhenEntityIsPersisted()
    {
        // Given: a valid RecordingEntity
        RecordingEntity entity = CreateValidRecordingEntity();
        LastCreatedRecordingId = entity.Id;

        // When: saving the entity
        Result<string> result = await RecordingRepository.SaveAsync(entity, CancellationToken.None);

        // Then: result should be success and contain the entity's ID
        Assert.That(result.IsSuccess, Is.True,
            $"Expected Result.Success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Is.EqualTo(entity.Id),
            $"Expected result.Value == '{entity.Id}' but got: '{result.Value}'");
    }

    [Test]
    [Description("Should persist all fields of the RecordingEntity correctly in DynamoDB")]
    public async Task ShouldPersistAllRecordingFieldsCorrectlyWhenEntityIsSaved()
    {
        // Given: a RecordingEntity with all fields populated
        RecordingEntity entity = new()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Meditacion matutina",
            Description = "Sesion guiada de meditacion para el inicio del dia",
            Type = RecordingType.Meditacion,
            StorageKey = $"recordings/{Guid.NewGuid()}/meditacion.mp4",
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "admin-user-id",
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = "admin-user-id"
        };
        LastCreatedRecordingId = entity.Id;

        // When: saving the entity
        Result<string> saveResult = await RecordingRepository.SaveAsync(entity, CancellationToken.None);
        Assert.That(saveResult.IsSuccess, Is.True,
            $"Pre-condition failed: SaveAsync returned failure with error: '{(saveResult.IsSuccess ? "N/A" : saveResult.Error)}'");

        // Then: loading the record from DynamoDB should return matching data
        LoadConfig loadConfig = new() { OverrideTableName = _configuration.RecordingsTable };
        RecordingDBModel? persisted = await DynamoContext.LoadAsync<RecordingDBModel>(entity.Id, loadConfig);

        Assert.That(persisted, Is.Not.Null,
            $"Expected a persisted RecordingDBModel with Id='{entity.Id}' but got null");
        Assert.That(persisted!.Id, Is.EqualTo(entity.Id),
            $"Expected Id='{entity.Id}' but got: '{persisted.Id}'");
        Assert.That(persisted.Name, Is.EqualTo(entity.Name),
            $"Expected Name='{entity.Name}' but got: '{persisted.Name}'");
        Assert.That(persisted.Description, Is.EqualTo(entity.Description),
            $"Expected Description='{entity.Description}' but got: '{persisted.Description}'");
        Assert.That(persisted.Type, Is.EqualTo(entity.Type),
            $"Expected Type='{entity.Type}' but got: '{persisted.Type}'");
        Assert.That(persisted.StorageKey, Is.EqualTo(entity.StorageKey),
            $"Expected StorageKey='{entity.StorageKey}' but got: '{persisted.StorageKey}'");
        Assert.That(persisted.CreatedBy, Is.EqualTo(entity.CreatedBy),
            $"Expected CreatedBy='{entity.CreatedBy}' but got: '{persisted.CreatedBy}'");
    }

    [TestCase(RecordingType.Meditacion, TestName = "Meditacion")]
    [TestCase(RecordingType.Masterclass, TestName = "Masterclass")]
    [TestCase(RecordingType.Taller, TestName = "Taller")]
    [Description("Should persist the correct RecordingType enum value for each valid type")]
    public async Task ShouldPersistCorrectRecordingTypeWhenEntityIsSaved(RecordingType type)
    {
        // Given: a RecordingEntity with a specific type
        RecordingEntity entity = CreateValidRecordingEntity();
        entity.Type = type;
        LastCreatedRecordingId = entity.Id;

        // When: saving the entity
        Result<string> result = await RecordingRepository.SaveAsync(entity, CancellationToken.None);

        // Then: the persisted record should have the same type
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success for type='{type}' but got failure: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        LoadConfig loadConfig = new() { OverrideTableName = _configuration.RecordingsTable };
        RecordingDBModel? persisted = await DynamoContext.LoadAsync<RecordingDBModel>(entity.Id, loadConfig);

        Assert.That(persisted, Is.Not.Null,
            $"Expected persisted record with Id='{entity.Id}' but got null");
        Assert.That(persisted!.Type, Is.EqualTo(type),
            $"Expected persisted Type='{type}' but got: '{persisted.Type}'");
    }

    #endregion

    #region SaveAsync — Overwrite Behaviour

    [Test]
    [Description("Should overwrite existing recording when SaveAsync is called with the same ID")]
    public async Task ShouldOverwriteExistingRecordingWhenSameIdIsSavedTwice()
    {
        // Given: a recording persisted once
        string recordingId = Guid.NewGuid().ToString();
        RecordingEntity firstEntity = new()
        {
            Id = recordingId,
            Name = "Nombre original",
            Description = "Descripcion original",
            Type = RecordingType.Meditacion,
            StorageKey = "recordings/original-key.mp4",
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "admin-user-id",
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = "admin-user-id"
        };
        LastCreatedRecordingId = recordingId;

        Result<string> firstResult = await RecordingRepository.SaveAsync(firstEntity, CancellationToken.None);
        Assert.That(firstResult.IsSuccess, Is.True,
            $"Pre-condition failed: first SaveAsync returned failure: '{(firstResult.IsSuccess ? "N/A" : firstResult.Error)}'");

        // And: a modified entity with the same ID
        RecordingEntity updatedEntity = new()
        {
            Id = recordingId,
            Name = "Nombre actualizado",
            Description = "Descripcion actualizada",
            Type = RecordingType.Taller,
            StorageKey = "recordings/updated-key.mp4",
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "admin-user-id",
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = "admin-user-id"
        };

        // When: saving the updated entity
        Result<string> secondResult = await RecordingRepository.SaveAsync(updatedEntity, CancellationToken.None);

        // Then: the second save should succeed and the record should reflect updated data
        Assert.That(secondResult.IsSuccess, Is.True,
            $"Expected second SaveAsync to succeed but got failure: '{(secondResult.IsSuccess ? "N/A" : secondResult.Error)}'");

        LoadConfig loadConfig = new() { OverrideTableName = _configuration.RecordingsTable };
        RecordingDBModel? persisted = await DynamoContext.LoadAsync<RecordingDBModel>(recordingId, loadConfig);

        Assert.That(persisted, Is.Not.Null,
            $"Expected persisted record with Id='{recordingId}' but got null");
        Assert.That(persisted!.Name, Is.EqualTo("Nombre actualizado"),
            $"Expected overwritten Name='Nombre actualizado' but got: '{persisted.Name}'");
        Assert.That(persisted.Description, Is.EqualTo("Descripcion actualizada"),
            $"Expected overwritten Description='Descripcion actualizada' but got: '{persisted.Description}'");
        Assert.That(persisted.Name, Is.Not.EqualTo("Nombre original"),
            "Expected original Name to have been overwritten but it was still present");
    }

    #endregion

    #region SaveAsync — Uses Correct Table Name

    [Test]
    [Description("Should use the RecordingsTable name from config (OverrideTableName) when saving")]
    public async Task ShouldUseRecordingsTableNameFromConfigWhenSaving()
    {
        // Given: a valid recording entity
        RecordingEntity entity = CreateValidRecordingEntity();
        LastCreatedRecordingId = entity.Id;

        // When: saving the entity via the repository
        Result<string> result = await RecordingRepository.SaveAsync(entity, CancellationToken.None);

        // Then: the record should be retrievable from the configured RecordingsTable
        Assert.That(result.IsSuccess, Is.True,
            $"SaveAsync failed, indicating the table name might be wrong. Error: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        LoadConfig loadConfig = new() { OverrideTableName = _configuration.RecordingsTable };
        RecordingDBModel? persisted = await DynamoContext.LoadAsync<RecordingDBModel>(entity.Id, loadConfig);

        Assert.That(persisted, Is.Not.Null,
            $"Expected to load record from table '{_configuration.RecordingsTable}' but got null. " +
            $"This suggests OverrideTableName is not being applied correctly.");
    }

    #endregion
}
