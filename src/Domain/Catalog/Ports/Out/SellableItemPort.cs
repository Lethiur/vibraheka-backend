using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Domain.Catalog.Ports.Out;

public interface ISellableItemPort
{
    public Task<Result<SellableItemEntity>> GetSellableItemByIdAsync(string sellableItemId, CancellationToken cancellationToken);

    public Task<Result<SellableItemEntity>> GetSellableItemByReferenceAsync(string referenceID, CancellationToken cancellationToken);

    public Task<Result<Unit>> DeactivateSellableItemAsync(string referenceID, CancellationToken cancellationToken);

    public Task<Result<Unit>> ActivateSellableItemAsync(string referenceID, CancellationToken cancellationToken);

    public Task<Result<Unit>> DeleteSellableItemAsync(string referenceID, CancellationToken cancellationToken);
}
