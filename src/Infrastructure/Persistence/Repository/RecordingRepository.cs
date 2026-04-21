using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Ports.Out;
using VibraHeka.Infrastructure.Entities;
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
}
