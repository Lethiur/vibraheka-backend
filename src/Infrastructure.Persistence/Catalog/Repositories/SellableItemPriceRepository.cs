using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.Repository;
namespace Infrastructure.Persistence.Catalog.Repositories;

public class SellableItemPriceRepository(
    IAmazonDynamoDB client,
    IDynamoDBContext context,
    AWSConfig config,
    SellableItemPriceEntityMapper mapper,
    ILogger<SellableItemPriceRepository> logger)
    : GenericDynamoRepository<SellableItemPriceDBModel>(context, client, config.SellableItemPricesTable, logger)
{
    public async Task<Result<SellableItemPriceEntity>> GetBySellableItemIdAndKindAsync(
        string sellableItemId, PriceKind kind, CancellationToken ct)
    {
        Result<List<SellableItemPriceDBModel>> queryResult =
            await FindAllByIndexAsync("SellableItemID-Index", sellableItemId, ct);
        return queryResult
            .MapError(error => error == GenericPersistenceErrors.NoRecordsFound
                ? CatalogErrors.SellableItemPriceNotFound
                : CatalogErrors.FailedToQuerySellableItemPrice)
            .Bind(models =>
            {
                SellableItemPriceDBModel? match = models.Find(m => m.Kind == kind);
                return match is not null
                    ? Result.Success(mapper.ToDomain(match))
                    : Result.Failure<SellableItemPriceEntity>(CatalogErrors.SellableItemPriceNotFound);
            });
    }

    public Task<Result<SellableItemPriceEntity>> GetBySellableItemPriceIdAsync(string sellableItemPriceId,
        CancellationToken cancellationToken)
    {
        return FindByID(sellableItemPriceId, cancellationToken).Map(mapper.ToDomain);
    }
}
