using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Commerce.Mappers;
using Infrastructure.Persistence.Commerce.Models;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Commerce.Ports.Out;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Infrastructure.Entities;

namespace Infrastructure.Persistence.Commerce.Adapters;

/// <summary>
/// Implements the functionality to persist order data into a DynamoDB data store.
/// </summary>
public class OrderWriteAdapter(OrderMapper Mapper, AWSConfig Config, IDynamoDBContext Context) : IOrderWritePort
{
    /// <summary>
    /// Creates a new order by mapping a domain order entity to a database model and preparing it
    /// for a transactional write operation in DynamoDB.
    /// </summary>
    /// <param name="order">
    /// The domain order entity representing the order to be created. This entity includes
    /// details such as order ID, user ID, order status, subtotal, discount, tax, total, and other required order information.
    /// </param>
    /// <returns>
    /// An instance of <see cref="ITransactionalWriteOperation"/> that encapsulates the prepared
    /// transactional write operation for persisting the order in the database.
    /// </returns>
    public ITransactionalWriteOperation CreateOrder(OrderEntity order)
    {
        OrderDBModel model = Mapper.FromDomain(order);
        ITransactWrite<OrderDBModel> transactWrite = Context.CreateTransactWrite<OrderDBModel>(new TransactWriteConfig()
        {
            OverrideTableName = Config.OrdersTable
        });
        
        transactWrite.AddSaveItem(model);
        return new DynamoTransactionalWriteOperation(transactWrite);
    }
}
