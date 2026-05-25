using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Repositories;
using MediatR;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Ports.Out;
namespace Infrastructure.Persistence.Catalog.Adapters;

public class SellableItemAdapter(SellableItemRepository repository, SellableItemPriceRepository sellableItemPriceRepository) : ISellableItemPort
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
