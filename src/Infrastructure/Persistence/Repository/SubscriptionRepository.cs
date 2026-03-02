using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.Persistence.Repository;

/// <summary>
/// Handles operations related to subscription data in the underlying DynamoDB database.
/// Implements ISubscriptionRepository for domain-specific functionality and inherits
/// from GenericDynamoRepository for common data access operations.
/// </summary>
public class SubscriptionRepository(AWSConfig config, IDynamoDBContext context, SubscriptionEntityMapper mapper, ILogger<SubscriptionRepository> logger) : GenericDynamoRepository<SubscriptionDBModel>(context, config.SubscriptionTable, logger), ISubscriptionRepository
{
    /// <summary>
    /// Retrieves the order status for a specific user based on their user ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose order status is being queried.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing a <see cref="Result"/> object with the <see cref="SubscriptionStatus"/> of the user's order.</returns>
    public Task<Result<SubscriptionEntity>> GetSubscriptionDetailsForUser(string userId, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Retrieving subscription details for user {userId}");
        return FindOneByIndex(config.SubscriptionUserIdIndex, userId, cancellationToken)
            .MapError(error =>
            {
                return error switch
                {
                    GenericPersistenceErrors.NoRecordsFound => SubscriptionErrors.NoSubscriptionFound,
                    _ => AppErrors.UnknownError
                };
            })
            .MapTry(mapper.ToDomain);
    }

    /// <summary>
    /// Saves a subscription entity to the database.
    /// </summary>
    /// <param name="subscriptionEntity">The <see cref="SubscriptionEntity"/> object containing subscription details to be saved.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing a <see cref="Result"/> object with the saved <see cref="SubscriptionEntity"/>.</returns>
    /// <exception cref="NotImplementedException">Thrown when the method is not implemented.</exception>
    public Task<Result<SubscriptionEntity>> SaveSubscriptionAsync(SubscriptionEntity subscriptionEntity,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Saving subscription details for user {UserID}", subscriptionEntity.UserID);
        return Save(mapper.ToInternal(subscriptionEntity), cancellationToken)
            .Map(_ => subscriptionEntity);
    }

    public Task<Result<Unit>> DeleteSubscriptionForUser(SubscriptionEntity subscriptionEntity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting subscription details for user {}", subscriptionEntity.UserID);
        return Delete(mapper.ToInternal(subscriptionEntity), cancellationToken);
    }
}
