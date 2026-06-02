using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using NMoneys;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Ports.Out;
using VibraHeka.Domain.Common.Interfaces;

namespace VibraHeka.Application.Catalog.Commands.AdminCreatePrice;

public class AdminCreatePriceCommandHandler(
    ISellableItemPricePort sellableItemPricePort,
    ISellableItemPriceWritePort sellableItemPriceWritePort,
    ISellableItemPort sellableItemPort,
    IProductCreationWritePort productCreationWritePort,
    ICurrentUserService currentUserService,
    IAtomicWriteStore writeStore, ILogger<AdminCreatePriceCommandHandler> logger) : IRequestHandler<AdminCreatePriceCommand, Result<string>>
{
    public async Task<Result<string>> Handle(AdminCreatePriceCommand request, CancellationToken cancellationToken)
    {

        (bool _, bool isFailure, string? value, string? error) = await sellableItemPort.GetSellableItemByIdAsync(request.SellableItemID, cancellationToken)
            .Map(sellableItem => sellableItem.ExternalProductID);


        if (isFailure)
        {
            logger.LogError("Failed to get sellable item: {Error}", error);
            return Result.Failure<string>(error);
        }
        
        SellableItemPriceEntity sellableItemPriceEntity = new()
        {
            SellableItemPriceID = Guid.NewGuid().ToString(),
            Amount = new Money(request.Price, request.Currency),
            SellableItemID = request.SellableItemID,
            BillingInterval = request.Interval,
            IsActive = request.SetToActive,
            CreatedBy = currentUserService.UserId,
            Created = DateTime.UtcNow,
            ExternalProductID = value,
            LastModified = DateTime.UtcNow,
            LastModifiedBy = currentUserService.UserId,
            Kind = request.Interval.HasValue ? PriceKind.Recurring : PriceKind.OneTime
        };
        TransactionalWriteBatch productCreationBatch = new(Guid.NewGuid().ToString());
        if (request.SetToActive)
        {
            await DeactivateEverything(request.SellableItemID, cancellationToken).Tap(productCreationBatch.AddRange);
        }


        (bool _, bool isGatewayOperationFailed, ProductGatewayCreatedResponseModel gatewayResponse, string gatewayError) =
            await productCreationWritePort.AddSellableItemPriceToProduct(sellableItemPriceEntity, cancellationToken);

        if (isGatewayOperationFailed)
        {
            return Result.Failure<string>(gatewayError);
        }
        sellableItemPriceEntity.ExternalPriceID = gatewayResponse.ProductGatewayPriceID;
        productCreationBatch.Add(sellableItemPriceWritePort.CreateSellableItemPrice(sellableItemPriceEntity));
        
        return await writeStore.CommitAsync(productCreationBatch, cancellationToken).Map((_) => sellableItemPriceEntity.SellableItemPriceID);
    }


    /// <summary>
    /// Deactivates all active prices associated with the specified sellable item by creating transactional write operations for deactivation.
    /// </summary>
    /// <param name="sellableItemId">
    /// The unique identifier of the sellable item whose active prices need to be deactivated.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a Result object
    /// with a collection of transactional write operations for deactivating the active prices.
    /// </returns>
    private Task<Result<IEnumerable<ITransactionalWriteOperation>>> DeactivateEverything(string sellableItemId,
        CancellationToken cancellationToken)
    {
        return sellableItemPricePort.GetAllActivePricesBySellableItemIdAsync(sellableItemId, cancellationToken)
            .Map(activePrices =>
            {
                var operations = activePrices.Select(sellableItemPriceWritePort.DeactivatePrice);
                return operations;
            });
    }
}
