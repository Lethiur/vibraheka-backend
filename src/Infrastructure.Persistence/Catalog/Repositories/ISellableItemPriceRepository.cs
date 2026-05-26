using CSharpFunctionalExtensions;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;

namespace Infrastructure.Persistence.Catalog.Repositories;

public interface ISellableItemPriceRepository
{
    Task<Result<SellableItemPriceEntity>> GetBySellableItemIdAndKindAsync(
        string sellableItemId, PriceKind kind, CancellationToken ct);

    Task<Result<List<SellableItemPriceEntity>>> GetBySellableItemIdAsync(
        string sellableItemId, CancellationToken ct);

    Task<Result<SellableItemPriceEntity>> GetBySellableItemPriceIdAsync(
        string sellableItemPriceId, CancellationToken cancellationToken);
}

