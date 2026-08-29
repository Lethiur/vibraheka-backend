using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Domain.Catalog.Entities;

namespace Infrastructure.Persistence.Catalog.Adapters;

public class SellableItemWriteAdapter(
    SellableItemEntityMapper Mapper,
    IDynamoDBContext Context) : ISellableItemWritePort
{
    public ITransactionalWriteOperation CreateSellableItem(SellableItemEntity product)
    {
        SellableItemDBModel model = Mapper.FromDomain(product);
        ITransactWrite<SellableItemDBModel> transaction =
            Context.CreateTransactWrite<SellableItemDBModel>();
        transaction.AddSaveItem(model);
        
        return new DynamoTransactionalWriteOperation(transaction);
    }
}
