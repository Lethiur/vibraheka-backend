using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.RecordingRepositoryTest;

public abstract class GenericRecordingRepositoryTest : TestBase
{
    protected RecordingRepository RecordingRepository = default!;
    protected IDynamoDBContext DynamoContext = default!;
    protected ILogger<RecordingRepository> Logger = default!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        base.OneTimeSetUp();
        Logger = CreateTestLogger<RecordingRepository>();
        DynamoContext = CreateDynamoDBContext();
        RecordingRepository = new RecordingRepository(DynamoContext, _client, _configuration, new RecordingEntityMapper(), Logger);
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
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "admin-user-id",
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = "admin-user-id"
        };
    }
}
