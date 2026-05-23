using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Commerce.Mappers;
using Infrastructure.Persistence.Commerce.Models;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Commerce.Ports.Out;
using VibraHeka.Domain.Commerce.Entities;

namespace Infrastructure.Persistence.Commerce.Adapters;

/// <summary>
/// Provides methods to write order line data to the database using DynamoDB.
/// This implementation serves as an adapter to map domain entities to database models
/// and facilitate transactional write operations.
/// </summary>
public class OrderLineWriteAdapter(OrderLineMapper Mapper, IDynamoDBContext Context)
    : IOrderLineWritePort
{
    /// <summary>
    /// Creates a new order line transactionally in the database.
    /// </summary>
    /// <param name="orderLine">
    /// The <see cref="OrderLineEntity"/> instance containing order line details to be created.
    /// </param>
    /// <returns>
    /// An <see cref="ITransactionalWriteOperation"/> instance that encapsulates the transactional write operation for the order line.
    /// </returns>
    public ITransactionalWriteOperation CreateOrderLine(OrderLineEntity orderLine)
    {
        OrderLineDBModel model = Mapper.FromDomain(orderLine);

        ITransactWrite<OrderLineDBModel> transactWrite = Context.CreateTransactWrite<OrderLineDBModel>();

        transactWrite.AddSaveItem(model);
        return new DynamoTransactionalWriteOperation(transactWrite);
    }
}
