using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Infrastructure;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace Infrastructure.Persistence.Catalog.Repositories;

public class SubscriptionPlanRepository(
    IAmazonDynamoDB client,
    IDynamoDBContext context,
    SubscriptionPlanEntityMapper mapper,
    ILogger<SubscriptionPlanRepository> logger)
    : GenericDynamoRepository<SubscriptionPlanDBModel>(context, client, logger), ISubscriptionPlanRepository
{
    public Task<Result<Unit>> DeActivateSubscriptionPlanAsync(string subscriptionPlanId,
        CancellationToken cancellationToken)
    {
        Dictionary<string, AttributeValue> key = new()
        {
            { nameof(RecordingDBModel.Id), new AttributeValue { S = subscriptionPlanId } }
        };

        DynamoExpression update = new()
        {
            Expression = "set #status = :status",
            AttributeNames = { ["#status"] = "IsActive" },
            AttributeValues = { { ":status", new AttributeValue { N = "0" } } }
        };

        return UpdateAsync(key, update, null, cancellationToken);
    }

    public Task<Result<Unit>> ActivateSubscriptionPlanAsync(string subscriptionPlanId,
        CancellationToken cancellationToken)
    {
        Dictionary<string, AttributeValue> key = new()
        {
            { nameof(RecordingDBModel.Id), new AttributeValue { S = subscriptionPlanId } }
        };

        DynamoExpression update = new()
        {
            Expression = "set #status = :status",
            AttributeNames = { ["#status"] = "IsActive" },
            AttributeValues = { { ":status", new AttributeValue { N = "1" } } }
        };

        return UpdateAsync(key, update, null, cancellationToken);
    }

    public async Task<Result<IEnumerable<SubscriptionPlanEntity>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await GetAll(cancellationToken).Map(models => models.Select(mapper.ToDomain));
    }

    public Task<Result<SubscriptionPlanEntity>> SaveSubscriptionPlanAsync(SubscriptionPlanEntity entity, CancellationToken cancellationToken)
    {
        return Save(mapper.FromDomain(entity), cancellationToken).Map(_ => entity);
    }
}
