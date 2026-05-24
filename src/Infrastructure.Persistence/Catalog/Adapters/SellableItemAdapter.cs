using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Repositories;
using MediatR;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Ports.Out;
namespace Infrastructure.Persistence.Catalog.Adapters;

public class SellableItemAdapter(SellableItemRepository repository) : ISellableItemPort
{
    public Task<Result<SellableItemEntity>> GetSellableItemByIdAsync(string sellableItemId, CancellationToken cancellationToken)
    {
        return repository.FindByIDAsync(sellableItemId, cancellationToken);
    }

    public Task<Result<SellableItemEntity>> GetSellableItemByReferenceAsync(
        string referenceID, CancellationToken cancellationToken)
    {
        return repository.GetByReferenceIdAsync(referenceID, cancellationToken);
    }

    public Task<Result<Unit>> DeactivateSellableItemAsync(string referenceID, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Unit>> ActivateSellableItemAsync(string referenceID, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Unit>> DeleteSellableItemAsync(string referenceID, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
