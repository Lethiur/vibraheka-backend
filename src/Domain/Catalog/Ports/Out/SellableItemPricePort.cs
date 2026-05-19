using CSharpFunctionalExtensions;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Domain.Catalog.Ports.Out;

public interface ISellableItemPricePort
{
    public Task<Result<SellableItemPriceEntity>> GetSellableItemPriceAndKindAsync(string sellableItemPriceId, PriceKind kind, CancellationToken cancellationToken);
    
    public Task<Result<SellableItemPriceEntity>> GetSellableItemPriceById(string sellableItemPriceId, CancellationToken cancellationToken);
}
