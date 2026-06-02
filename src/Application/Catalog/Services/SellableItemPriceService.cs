using CSharpFunctionalExtensions;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Domain.Catalog.Ports.Out;

namespace VibraHeka.Application.Catalog.Services;

public class SellableItemPriceService(
    ISellableItemPriceWritePort sellableItemPriceWritePort,
    ISellableItemPricePort sellableItemPricePort) : ISellableItemPriceService
{
    public Task<Result<IEnumerable<ITransactionalWriteOperation>>> CreateTransactionToDisableAllPricesAsync(
        string sellableItemId, CancellationToken cancellationToken)
    {
        return sellableItemPricePort.GetAllActivePricesBySellableItemIdAsync(sellableItemId, cancellationToken)
            .Map(activePrices => activePrices.Select(sellableItemPriceWritePort.DeactivatePrice));
    }
}
