using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Repositories;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Ports.Out;

namespace Infrastructure.Persistence.Catalog.Adapters;

public class SellableItemPriceAdapter(SellableItemPriceRepository repository) : ISellableItemPricePort
{
    public Task<Result<SellableItemPriceEntity>> GetSellableItemPriceAndKindAsync(
        string sellableItemPriceId, PriceKind kind, CancellationToken cancellationToken)
    {
        return repository.GetBySellableItemIdAndKindAsync(sellableItemPriceId, kind, cancellationToken);
    }
}
