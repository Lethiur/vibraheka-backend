using CSharpFunctionalExtensions;
using MediatR;
using NMoneys;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Domain.Catalog.Ports.In;

/// <summary>
/// Defines a port for registering a sellable item in the system.
/// </summary>
public interface IRegisterSellableItemPort
{
    /// <summary>
    /// Registers a sellable item in the system with the specified details.
    /// </summary>
    /// <param name="entity">
    ///     The product entity representing the item to be registered. Must include
    ///     information such as ProductID, Name, Description, and Type.
    /// </param>
    /// <param name="price">
    ///     The price of the item, represented as a Money object.
    /// </param>
    /// <param name="kind">
    ///     The kind of price, which can be one-time or recurring, as defined by the PriceKind enum.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests during the operation.
    /// </param>
    /// <returns>
    /// A Result object containing Unit on success or an error in case of failure.
    /// </returns>
    public Task<Result<Unit>> RegisterSellableItemAsync(ProductEntity entity, Money price,
        PriceKind kind, CancellationToken cancellationToken);
}
