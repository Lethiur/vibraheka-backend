using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Catalog.Models;
using Infrastructure.Persistence.Catalog.Repositories;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using VibraHeka.Infrastructure.Persistence.Repository;
using RecordingEntityMapper = Infrastructure.Persistence.Catalog.Mappers.RecordingEntityMapper;

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
        RecordingRepository = new RecordingRepository(DynamoContext, _client, new RecordingEntityMapper(), Logger);
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
            await DynamoContext.DeleteAsync<RecordingDBModel>(recordingId);
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
            ID = Guid.NewGuid().ToString(),
            Name = _faker.Lorem.Sentence(3),
            Description = _faker.Lorem.Paragraph(),
            RecordingType = RecordingType.Meditacion,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "admin-user-id",
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = "admin-user-id"
        };
    }
}
