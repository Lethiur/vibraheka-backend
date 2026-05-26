using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.Repository;
namespace Infrastructure.Persistence.Catalog.Repositories;

public class SellableItemRepository(
    IAmazonDynamoDB client,
    IDynamoDBContext context,
    SellableItemEntityMapper mapper,
    ILogger<SellableItemRepository> logger)
    : GenericDynamoRepository<SellableItemDBModel>(context, client, logger), ISellableItemRepository
{
    public async Task<Result<SellableItemEntity>> GetByReferenceIdAsync(string referenceId, CancellationToken ct)
    {
        return (await FindOneByIndex("ReferenceID-Index", referenceId, ct))
            .MapError(error => error == GenericPersistenceErrors.NoRecordsFound
                ? CatalogErrors.SellableItemNotFound
                : CatalogErrors.FailedToQuerySellableItem)
            .Map(mapper.ToDomain);
    }

    public Task<Result<SellableItemEntity>> FindByIDAsync(string sellableItemID,
        CancellationToken cancellationToken)
    {
        return FindByID(sellableItemID, cancellationToken).Map(mapper.ToDomain);
    }
}
