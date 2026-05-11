using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Orders.Entities;
using Infrastructure.Persistence.Orders.Mappers;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Orders.Entities;
using VibraHeka.Domain.Orders.Models;
using VibraHeka.Domain.Orders.Ports.Out;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace Infrastructure.Persistence.Orders.Adapters;

public class OrderAdapter(
    IDynamoDBContext context,
    IAmazonDynamoDB client,
    OrderEntityMapper mapper,
    AWSConfig config,
    ILogger<OrderAdapter> logger) : GenericDynamoRepository<OrderDBModel>(context, client, config.OrdersTable, logger),
    IOrderPort
{
    public Task<Result<OrderEntity>> CreateOrderAsync(OrderEntity order, CancellationToken cancellationToken)
    {
        return Save(mapper.FromDomain(order), cancellationToken)
            .Map(_ => order);
    }

    public Task<Result<OrderEntity>> GetOrderByIDAsync(string orderID, CancellationToken cancellationToken)
    {
        return FindByID(orderID, cancellationToken).Map(mapper.ToDomain);
    }

    public Task<Result<OrderEntity>> UpdatePaymentInfoAsync(OrderEntity order, CheckoutSessionCompletedModel model,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
