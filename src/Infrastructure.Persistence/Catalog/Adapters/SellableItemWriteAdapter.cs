using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Infrastructure.Entities;
namespace Infrastructure.Persistence.Catalog.Adapters;

public class SellableItemWriteAdapter(
    SellableItemEntityMapper Mapper,
    AWSConfig Config,
    IDynamoDBContext Context) : ISellableItemWritePort
{
    public ITransactionalWriteOperation CreateSellableItem(SellableItemEntity product)
    {
        SellableItemDBModel model = Mapper.FromDomain(product);
        ITransactWrite<SellableItemDBModel> transaction =
            Context.CreateTransactWrite<SellableItemDBModel>(new TransactWriteConfig
            {
                OverrideTableName = Config.SellableItemsTable
            });
        transaction.AddSaveItem(model);
        return new DynamoTransactionalWriteOperation(transaction);
    }
}
