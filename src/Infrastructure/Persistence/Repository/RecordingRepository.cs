using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Errors;
using VibraHeka.Domain.Recordings.Ports.Out;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.Persistence.Repository;

public class RecordingRepository(
    IDynamoDBContext context,
    AWSConfig config,
    RecordingEntityMapper mapper,
    ILogger<RecordingRepository> logger)
    : GenericDynamoRepository<RecordingDBModel>(context, config.RecordingsTable, logger),
        IRecordingRegistryPort
{
    public Task<Result<string>> SaveRecording(RecordingEntity recording, CancellationToken cancellationToken)
    {
        return Save(mapper.FromDomain(recording), cancellationToken).Map(_ => recording.Id);
    }

    public async Task<Result<IEnumerable<RecordingEntity>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await GetAll(cancellationToken).Map(models => models.Select(mapper.FromDbModel));
    }

    public async Task<Result<RecordingEntity>> GetByIdAsync(string recordingId, CancellationToken cancellationToken)
    {
        return await FindByID(recordingId, cancellationToken)
            .MapError(error => error == GenericPersistenceErrors.NoRecordsFound
                ? RecordingErrors.NotFound
                : error)
            .Map(mapper.FromDbModel);
    }
}
