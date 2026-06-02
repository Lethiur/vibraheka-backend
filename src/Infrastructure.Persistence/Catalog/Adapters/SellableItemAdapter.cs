using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Repositories;
using MediatR;
using VibraHeka.Application.Common.Extensions.Results;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Ports.Out;
using VibraHeka.Infrastructure.Exceptions;

namespace Infrastructure.Persistence.Catalog.Adapters;

public class SellableItemAdapter(ISellableItemRepository repository, ISellableItemPriceRepository sellableItemPriceRepository) : ISellableItemPort
{
    public Task<Result<SellableItemEntity>> GetSellableItemByIdAsync(string sellableItemId, CancellationToken cancellationToken)
    {
        return repository.FindByIDAsync(sellableItemId, cancellationToken);
    }

    public Task<Result<SellableItemEntity>> GetSellableItemByReferenceAsync(
        string referenceID, CancellationToken cancellationToken)
    {
        return repository.GetByReferenceIdAsync(referenceID, cancellationToken).BindTry(
            async sellableItem =>
            {
                var pricesResult = await sellableItemPriceRepository.GetBySellableItemIdAsync(sellableItem.SellableItemID, cancellationToken);
                return pricesResult.Map(prices =>
                {
                    sellableItem.Prices = prices;
                    return sellableItem;
                });
            });
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
