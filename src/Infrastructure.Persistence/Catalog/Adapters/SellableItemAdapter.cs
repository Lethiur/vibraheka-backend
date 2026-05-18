using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Repositories;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Ports.Out;
namespace Infrastructure.Persistence.Catalog.Adapters;

public class SellableItemAdapter(SellableItemRepository repository) : ISellableItemPort
{
    public Task<Result<SellableItemEntity>> GetSellableItemByReferenceAsync(
        string referenceID, CancellationToken cancellationToken)
    {
        return repository.GetByReferenceIdAsync(referenceID, cancellationToken);
    }
}
