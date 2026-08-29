using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Infrastructure;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace Infrastructure.Persistence.Catalog.Repositories;

public class RecordingRepository(
    IDynamoDBContext context,
    IAmazonDynamoDB client,
    RecordingEntityMapper mapper,
    ILogger<RecordingRepository> logger)
    : GenericDynamoRepository<RecordingDBModel>(context, client, logger), IRecordingRepository
{
    public Task<Result<string>> SaveRecording(RecordingEntity recording, CancellationToken cancellationToken)
    {
        return Save(mapper.FromDomain(recording), cancellationToken).Map(_ => recording.RecordingID);
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

    public async Task<Result> DeleteRecordingAsync(RecordingEntity recording, CancellationToken cancellationToken)
    {
        RecordingDBModel model = mapper.FromDomain(recording);
        Result<Unit> result = await Delete(model, cancellationToken);
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
    }

    public Task<Result<Unit>> DeactivateRecording(string recordingId, CancellationToken cancellationToken)
    {
        Dictionary<string, AttributeValue> key = new()
        {
            { nameof(RecordingDBModel.Id), new AttributeValue { S = recordingId } }
        };

        DynamoExpression update = new()
        {
            Expression = "set #status = :status",
            AttributeNames = { ["#status"] = "IsActive" },
            AttributeValues = { { ":status", new AttributeValue { N = "0" } } }
        };

        return UpdateAsync(key, update, null, cancellationToken);
    }

    public Task<Result<Unit>> ActivateRecording(string recordingId, CancellationToken cancellationToken)
    {
        Dictionary<string, AttributeValue> key = new()
        {
            { nameof(RecordingDBModel.Id), new AttributeValue { S = recordingId } }
        };

        DynamoExpression update = new()
        {
            Expression = "set #status = :status",
            AttributeNames = { ["#status"] = "IsActive" },
            AttributeValues = { { ":status", new AttributeValue { N = "1" } } }
        };

        return UpdateAsync(key, update, null, cancellationToken);
    }
}
