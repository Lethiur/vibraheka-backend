using System.Globalization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Events.Mappers;
using Infrastructure.Persistence.Events.Models;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Events.Entities;
using VibraHeka.Infrastructure;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace Infrastructure.Persistence.Events.Repositories;

public class EventRepository(
    IDynamoDBContext context,
    IAmazonDynamoDB client,
    EventEntityMapper mapper,
    ILogger<EventRepository> logger)
    : GenericDynamoRepository<EventDBModel>(context, client, logger)
{
    public Task<Result<EventEntity>> SaveEventAsync(EventEntity entity, CancellationToken cancellationToken)
    {
        return Save(mapper.FromDomain(entity), cancellationToken).Map(_ => entity);
    }

    public Task<Result<List<EventEntity>>> GetEventsFromDateAsync(DateTimeOffset startDate, DateTimeOffset endDate,
        CancellationToken cancellationToken)
    {
        // "EventsByDate-Index" has EventDateUtc as its GSI partition key.
        // DynamoDB only allows equality (=) on partition keys inside KeyConditionExpression;
        // using BETWEEN there triggers "Query key condition not supported".
        // A date-range filter on a partition key requires a full-table scan with FilterExpression.
        DynamoExpression filter = new()
        {
            Expression = "#EventDate BETWEEN :startDate AND :endDate",
            AttributeNames = new Dictionary<string, string> { { "#EventDate", "EventDateUtc" } },
            AttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":startDate", new AttributeValue { S = startDate.ToString("O", CultureInfo.InvariantCulture) } },
                { ":endDate", new AttributeValue { S = endDate.ToString("O", CultureInfo.InvariantCulture) } },
            },
        };

        return ScanWithFilterAsync(filter, cancellationToken).Map(list => list.Select(mapper.ToDomain).ToList());
    }
}
