using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Domain.Catalog.Entities;

namespace Infrastructure.Persistence.Catalog.Adapters;

public class SellableItemPriceWriteAdapter(
    SellableItemPriceEntityMapper Mapper,
    IDynamoDBContext Context) : ISellableItemPriceWritePort
{
    public ITransactionalWriteOperation CreateSellableItemPrice(SellableItemPriceEntity price)
    {
        SellableItemPriceDBModel model = Mapper.FromDomain(price);
        ITransactWrite<SellableItemPriceDBModel> transaction =
            Context.CreateTransactWrite<SellableItemPriceDBModel>();
        transaction.AddSaveItem(model);
        return new DynamoTransactionalWriteOperation(transaction);
    }

    public ITransactionalWriteOperation DeactivatePrice(SellableItemPriceEntity price)
    {
        ITransactWrite<SellableItemPriceDBModel> transaction =
            Context.CreateTransactWrite<SellableItemPriceDBModel>();
        
        Expression expression = new()
        {
            ExpressionStatement = "SET #isActive = :false",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                ["#isActive"] = nameof(SellableItemPriceEntity.IsActive)
            },
            ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
            {
                [":false"] = new Primitive("0", true)
            }
        };
        
        transaction.AddSaveItem(hashKey:price.SellableItemPriceID, conditionExpression: null, updateExpression: expression);
        
        return new DynamoTransactionalWriteOperation(transaction);
    }

    public ITransactionalWriteOperation ActivatePrice(string sellableItemPrice)
    {
        ITransactWrite<SellableItemPriceDBModel> transaction =
            Context.CreateTransactWrite<SellableItemPriceDBModel>();
        
        Expression expression = new()
        {
            ExpressionStatement = "SET #isActive = :true",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                ["#isActive"] = nameof(SellableItemPriceEntity.IsActive)
            },
            ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
            {
                [":true"] = new Primitive("1", true)
            }
        };
        
        transaction.AddSaveItem(hashKey:sellableItemPrice, conditionExpression: null, updateExpression: expression);
        
        return new DynamoTransactionalWriteOperation(transaction);
    }
}
