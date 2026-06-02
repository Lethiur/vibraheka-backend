using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Application.Catalog.Ports.Out;

public interface ISellableItemPriceWritePort
{
    ITransactionalWriteOperation CreateSellableItemPrice(SellableItemPriceEntity price);
    
    ITransactionalWriteOperation DeactivatePrice(SellableItemPriceEntity price);
}
