using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Extensions.Results;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Infrastructure;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace Infrastructure.Persistence.Catalog.Repositories;

public class SellableItemPriceRepository(
    IAmazonDynamoDB client,
    IDynamoDBContext context,
    SellableItemPriceEntityMapper mapper,
    ILogger<SellableItemPriceRepository> logger)
    : GenericDynamoRepository<SellableItemPriceDBModel>(context, client, logger), ISellableItemPriceRepository
{
    public async Task<Result<SellableItemPriceEntity>> GetBySellableItemIdAndKindAsync(
        string sellableItemId, PriceKind kind, CancellationToken ct)
    {
        DynamoExpression expression = new()
        {
            IndexName = "SellableItemID-Kind-Index",
            Expression = "#sid = :sid AND #kind = :kind",
            AttributeNames = new Dictionary<string, string> { ["#sid"] = "SellableItemID", ["#kind"] = "Kind", },
            AttributeValues = new Dictionary<string, AttributeValue>
            {
                [":sid"] = new AttributeValue { S = sellableItemId },
                [":kind"] = new AttributeValue { S = kind.ToString() },
            },
        };

        return await QueryIndexAsync(expression, ct)
            .Bind(items => items.Count > 0
                ? Result.Success(mapper.ToDomain(items[0]))
                : Result.Failure<SellableItemPriceEntity>(CatalogErrors.SellableItemPriceNotFound));
    }

    public async Task<Result<List<SellableItemPriceEntity>>> GetBySellableItemIdAsync(string sellableItemId,
        CancellationToken ct)
    {
        return await FindAllByIndexAsync("SellableItemID-Index", sellableItemId, ct)
            .MapError(error => error == GenericPersistenceErrors.NoRecordsFound
                ? CatalogErrors.SellableItemPriceNotFound
                : CatalogErrors.FailedToQuerySellableItemPrice)
            .Map(models => models.ConvertAll(mapper.ToDomain))
            .OnFailureCompensateWhen(error => error == CatalogErrors.SellableItemPriceNotFound, _ => Task.FromResult(Result.Success<List<SellableItemPriceEntity>>([])));
    }

    public Task<Result<SellableItemPriceEntity>> GetBySellableItemPriceIdAsync(string sellableItemPriceId,
        CancellationToken cancellationToken)
    {
        return FindByID(sellableItemPriceId, cancellationToken).Map(mapper.ToDomain);
    }

    public Task<Result<IEnumerable<SellableItemPriceEntity>>> GetAllActivePricesBySellableItemId(string sellableItemId,
        CancellationToken cancellationToken)
    {
        DynamoExpression expression = new()
        {
            IndexName = "SellableItemID-Index",
            Expression = "#sid = :sid",
            FilterExpression = "#active = :active",
            AttributeNames = new()
            {
                ["#sid"] = nameof(SellableItemPriceDBModel.SellableItemID),
                ["#active"] = nameof(SellableItemPriceDBModel.IsActive),
            },
            AttributeValues = new()
            {
                [":sid"] = new AttributeValue { S = sellableItemId },
                [":active"] = new AttributeValue { N = "1" },
            },
        };


        return QueryIndexAsync(expression, cancellationToken)
            .MapError(error => error == GenericPersistenceErrors.NoRecordsFound
                ? CatalogErrors.SellableItemPriceNotFound
                : CatalogErrors.FailedToQuerySellableItemPrice)
            .Map(models => models.Select(mapper.ToDomain));
    }
}
