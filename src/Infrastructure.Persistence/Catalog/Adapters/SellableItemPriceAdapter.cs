using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Repositories;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Ports.Out;

namespace Infrastructure.Persistence.Catalog.Adapters;

public class SellableItemPriceAdapter(ISellableItemPriceRepository repository) : ISellableItemPricePort
{
    public Task<Result<SellableItemPriceEntity>> GetSellableItemPriceAndKindAsync(
        string sellableItemPriceId, PriceKind kind, CancellationToken cancellationToken)
    {
        return repository.GetBySellableItemIdAndKindAsync(sellableItemPriceId, kind, cancellationToken);
    }

    public Task<Result<SellableItemPriceEntity>> GetSellableItemPriceById(string sellableItemPriceId, CancellationToken cancellationToken)
    {
        return repository.GetBySellableItemPriceIdAsync(sellableItemPriceId, cancellationToken);
    }
}
