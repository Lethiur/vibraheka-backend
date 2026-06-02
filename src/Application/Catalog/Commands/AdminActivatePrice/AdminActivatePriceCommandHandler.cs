using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Application.Catalog.Services;

namespace VibraHeka.Application.Catalog.Commands.AdminActivatePrice;

public class AdminActivatePriceCommandHandler(
    IAtomicWriteStore writeStore,
    ISellableItemPriceWritePort sellableItemPriceWritePort,
    ISellableItemPriceService sellableItemPriceService,
    ILogger<AdminActivatePriceCommandHandler> logger) : IRequestHandler<AdminActivatePriceCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AdminActivatePriceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Called AdminActivatePriceCommandHandler, activating price {SellableItemPriceID}",
            request.SellableItemPriceID);

        TransactionalWriteBatch productCreationBatch = new(Guid.NewGuid().ToString());
        await sellableItemPriceService
            .CreateTransactionToDisableAllPricesAsync(request.SellableItemID, cancellationToken)
            .Tap(productCreationBatch.AddRange);

        logger.LogInformation("Created transaction to disable all prices for SellableItemID {SellableItemID}",
            request.SellableItemID);

        ITransactionalWriteOperation transactionalWriteOperation = sellableItemPriceWritePort.ActivatePrice(request.SellableItemPriceID);
        productCreationBatch.Add(transactionalWriteOperation);

        logger.LogInformation("Commiting transaction for SellableItemID {SellableItemID}", request.SellableItemPriceID);
        return await writeStore.CommitAsync(productCreationBatch, cancellationToken);
    }
}
