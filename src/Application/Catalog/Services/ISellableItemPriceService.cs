using CSharpFunctionalExtensions;
using VibraHeka.Application.Abstractions.Transactions;

namespace VibraHeka.Application.Catalog.Services;

public interface ISellableItemPriceService
{
    Task<Result<IEnumerable<ITransactionalWriteOperation>>> CreateTransactionToDisableAllPricesAsync(
        string sellableItemId,
        CancellationToken cancellationToken);
}
