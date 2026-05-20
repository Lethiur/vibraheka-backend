using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Commerce.Mappers;
using Infrastructure.Persistence.Commerce.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Commerce.Errors;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace Infrastructure.Persistence.Commerce.Repositories;

/// <summary>
/// Provides functionality for managing and persisting order data in the configured DynamoDB table.
/// This repository serves as the data access layer for performing CRUD operations on order entities within DynamoDB.
/// </summary>
public class OrderRepository(
    IAmazonDynamoDB client,
    IDynamoDBContext context,
    AWSConfig config,
    OrderMapper mapper,
    ILogger<OrderRepository> logger)
    : GenericDynamoRepository<OrderDBModel>(context, client, config.OrdersTable, logger)
{
    /// <summary>
    /// Saves an order asynchronously to the underlying storage.
    /// </summary>
    /// <param name="entity">The order entity to be saved.</param>
    /// <param name="cancellationToken">
    /// A token that can be used to signal cancellation of the operation.
    /// </param>
    /// <returns>
    /// A result containing the saved order entity if successful, or an error indicating the reason for failure.
    /// </returns>
    public async Task<Result<OrderEntity>> SaveOrderAsync(OrderEntity entity, CancellationToken cancellationToken)
    {
        Result<Unit> result = await Save(mapper.FromDomain(entity), cancellationToken);

        return result.Map(_ => entity).MapError(error =>
        {
            return error switch
            {
                _ => CommerceErrors.FailedToSaveOrder
            };
        });
    }
}
