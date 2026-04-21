using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Ports.Out;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.Persistence.Repository;

public class RecordingRepository(IDynamoDBContext context, AWSConfig config) : IRecordingRegistryPort
{
    public async Task<Result<string>> SaveAsync(RecordingEntity recording, CancellationToken cancellationToken)
    {
        SaveConfig saveConfig = new()
        {
            OverrideTableName = config.RecordingsTable
        };

        await context.SaveAsync(RecordingDBModel.FromDomain(recording), saveConfig, cancellationToken);
        return recording.Id;
    }

    public async Task<Result<IEnumerable<RecordingEntity>>> GetAllAsync(CancellationToken cancellationToken)
    {
        ScanConfig scanConfig = new() { OverrideTableName = config.RecordingsTable };
        try
        {
            IAsyncSearch<RecordingDBModel> search =
                context.ScanAsync<RecordingDBModel>(Enumerable.Empty<ScanCondition>(), scanConfig);
            List<RecordingDBModel> models = await search.GetRemainingAsync(cancellationToken);
            return Result.Success<IEnumerable<RecordingEntity>>(models.Select(m => new RecordingEntity
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Type = m.Type,
                StorageKey = m.StorageKey,
                Created = m.Created,
                CreatedBy = m.CreatedBy,
                LastModified = m.LastModified,
                LastModifiedBy = m.LastModifiedBy
            }));
        }
        catch (Exception ex)
        {
            string error = ex switch
            {
                ProvisionedThroughputExceededException => GenericPersistenceErrors.ProvisionedThroughputExceeded,
                ResourceNotFoundException => GenericPersistenceErrors.ResourceNotFound,
                _ => GenericPersistenceErrors.GeneralError
            };
            return Result.Failure<IEnumerable<RecordingEntity>>(error);
        }
    }
}
