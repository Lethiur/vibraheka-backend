using CSharpFunctionalExtensions;
using VibraHeka.Domain.Catalog.Entities;

namespace Infrastructure.Persistence.Catalog.Repositories;

public interface ISellableItemRepository
{
    Task<Result<SellableItemEntity>> GetByReferenceIdAsync(string referenceId, CancellationToken ct);

    Task<Result<SellableItemEntity>> FindByIDAsync(string sellableItemId, CancellationToken cancellationToken);
}

