using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using CSharpFunctionalExtensions;
using Infrastructure.AWS.DynamoDB.Errors;
using Infrastructure.AWS.DynamoDB.Subscriptions.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.Subscriptions.Ports.Out;
using VibraHeka.Infrastructure.Entities;
using SubscriptionEntityMapper = Infrastructure.AWS.DynamoDB.Subscriptions.Mappers.SubscriptionEntityMapper;

namespace Infrastructure.AWS.DynamoDB.Subscriptions.Adapters;

/// <summary>
/// Handles operations related to subscription data in the underlying DynamoDB database.
/// Implements ISubscriptionRepository for domain-specific functionality and inherits
/// from GenericDynamoRepository for common data access operations.
/// </summary>
public class SubscriptionAdapter(
    IOptionsMonitor<AWSConfig> config,
    IDynamoDBContext context,
    IAmazonDynamoDB client,
    SubscriptionEntityMapper mapper,
    ILogger<SubscriptionAdapter> logger)
    : GenericDynamoRepository<SubscriptionDBModel>(context, client, config.CurrentValue.SubscriptionTable, logger),
        SubscriptionPort
{
    /// <summary>
    /// Retrieves the order status for a specific user based on their user ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose order status is being queried.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing a <see cref="Result"/>
    /// object with the <see cref="SubscriptionEntity"/> of the user's subscription.</returns>
    public Task<Result<SubscriptionEntity>> GetSubscriptionForUser(string userId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation($"Retrieving subscription details for user {userId}");
        return FindOneByIndex(config.CurrentValue.SubscriptionUserIdIndex, userId, cancellationToken)
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
        return Save(mapper.FromDomain(subscriptionEntity), cancellationToken)
            .Map(_ => subscriptionEntity);
    }

    public Task<Result<Unit>> DeleteSubscriptionForUser(SubscriptionEntity subscriptionEntity,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting subscription details for user {}", subscriptionEntity.UserID);
        return Delete(mapper.FromDomain(subscriptionEntity), cancellationToken);
    }

    public Task<Result<Unit>> CreateSubscription(SubscriptionEntity subscription,
        CancellationToken cancellationToken)
    {
        return Save(mapper.FromDomain(subscription), cancellationToken);
    }

    public Task<Result<SubscriptionEntity>> GetSubscription(string subscriptionID, CancellationToken cancellationToken)
    {
        return FindByID(subscriptionID, cancellationToken).Map(mapper.ToDomain);
    }


    public Task<Result<Unit>> SetSubscriptionStatus(string subscriptionID, SubscriptionStatus status,
        CancellationToken cancellationToken)
    {
        Dictionary<string, AttributeValue> key = new Dictionary<string, AttributeValue>()
        {
            { nameof(SubscriptionDBModel.SubscriptionID), new AttributeValue { S = subscriptionID } }
        };

        DynamoExpression update = new DynamoExpression
        {
            Expression = "set #status = :status",
            AttributeNames = { ["#status"] = "SubscriptionStatus" },
            AttributeValues = { { ":status", new AttributeValue { S = status.ToString() } } }
        };

        return UpdateAsync(key, update, null, cancellationToken);
    }
}
