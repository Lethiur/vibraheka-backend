using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Infrastructure.Entities;
namespace Infrastructure.Persistence.Catalog.Adapters;

public class SellableItemPriceWriteAdapter(
    SellableItemPriceEntityMapper Mapper,
    AWSConfig Config,
    IDynamoDBContext Context) : ISellableItemPriceWritePort
{
    public ITransactionalWriteOperation CreateSellableItemPrice(SellableItemPriceEntity price)
    {
        SellableItemPriceDBModel model = Mapper.FromDomain(price);
        ITransactWrite<SellableItemPriceDBModel> transaction =
            Context.CreateTransactWrite<SellableItemPriceDBModel>(new TransactWriteConfig
            {
                OverrideTableName = Config.SellableItemPricesTable
            });
        transaction.AddSaveItem(model);
        return new DynamoTransactionalWriteOperation(transaction);
    }
}
