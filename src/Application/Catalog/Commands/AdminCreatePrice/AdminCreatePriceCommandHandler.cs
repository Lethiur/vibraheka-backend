using System.Globalization;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using NMoneys;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Application.Catalog.Services;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Ports.Out;
using VibraHeka.Domain.Common.Interfaces;

namespace VibraHeka.Application.Catalog.Commands.AdminCreatePrice;

public class AdminCreatePriceCommandHandler(
    ISellableItemPriceWritePort sellableItemPriceWritePort,
    ISellableItemPort sellableItemPort,
    IProductCreationWritePort productCreationWritePort,
    ICurrentUserService currentUserService,
    ISellableItemPriceService sellableItemPriceService,
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
            Amount = new Money(decimal.Parse(request.Price.ToString(CultureInfo.InvariantCulture)), request.Currency),
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
            await sellableItemPriceService.CreateTransactionToDisableAllPricesAsync(request.SellableItemID, cancellationToken).Tap(productCreationBatch.AddRange);
        }

        (bool _, bool isGatewayOperationFailed, ProductGatewayCreatedResponseModel gatewayResponse, string gatewayError) =
            await productCreationWritePort.AddSellableItemPriceToProduct(sellableItemPriceEntity, cancellationToken);

        if (isGatewayOperationFailed)
        {
            return Result.Failure<string>(gatewayError);
        }
        sellableItemPriceEntity.ExternalPriceID = gatewayResponse.ProductGatewayPriceID;
        productCreationBatch.Add(sellableItemPriceWritePort.CreateSellableItemPrice(sellableItemPriceEntity));

        return await writeStore.CommitAsync(productCreationBatch, cancellationToken).Map(_ => sellableItemPriceEntity.SellableItemPriceID);
    }
}
