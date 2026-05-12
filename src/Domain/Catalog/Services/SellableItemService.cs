using CSharpFunctionalExtensions;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Models;
using VibraHeka.Domain.Catalog.Ports.Out;

namespace VibraHeka.Domain.Catalog.Services;

/// <summary>
/// Represents a service for managing sellable items and their associated prices within the domain.
/// Provides methods to interact with sellable items, fetch price details for specific items and price kinds.
/// </summary>
public class SellableItemService(ISellableItemPort sellableItemPort, ISellableItemPricePort sellableItemPricePort)
{
    /// <summary>
    /// Fetches the price of a sellable item by its product reference and specified price kind.
    /// </summary>
    /// <param name="referenceID">
    /// The unique reference ID of the product used to identify the sellable item.
    /// </param>
    /// <param name="priceKind">
    /// The type of price to be fetched (e.g., OneTime, Recurring).
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used by other objects or threads to receive notice of cancellation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/>
    /// wrapping a <see cref="SellableItemPriceEntity"/> if the operation succeeds, or an error message if it fails.
    /// </returns>
    public Task<Result<SellableInformationModel>> GetSellableItemPriceByProductReferenceAndPriceKindAsync(
        string referenceID, PriceKind priceKind, CancellationToken cancellationToken)
    {
        return sellableItemPort.GetSellableItemByReferenceAsync(referenceID, cancellationToken)
            .Map(sellableItem => new SellableInformationModel { Item = sellableItem })
            .BindTry(sellableInformation => sellableItemPricePort.GetSellableItemPriceAndKindAsync(
                    sellableInformation.Item.SellableItemID, priceKind, cancellationToken)
                .Map(price =>
                {
                    sellableInformation.Price = price;
                    return sellableInformation;
                }));
    }
}
