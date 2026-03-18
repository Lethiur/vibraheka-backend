using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Extensions.Results;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.Services;

/// <summary>
/// The SubscriptionService class is responsible for managing subscription-related functionalities
/// including creation, cancellation, retrieval, reactivation, and deletion of subscriptions.
/// It interacts with the subscription repository, payment repository, and utilizes configuration settings.
/// </summary>
public class SubscriptionService(
    ISubscriptionRepository subscriptionRepository,
    IPaymentRepository paymentRepository,
    StripeConfig config,
    ILogger<SubscriptionService> logger) : ISubscriptionService
{
    /// <summary>
    /// Creates a subscription for the specified user based on the provided subscription checkout session details.
    /// Prepares the subscription context, sets session-specific details, and processes the subscription creation
    /// in the repository. Logs relevant information and handles error compensation for specific failure scenarios.
    /// </summary>
    /// <param name="user">The user entity for whom the subscription is being created.</param>
    /// <param name="context">The subscription checkout session entity containing details about the subscription session.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/> object
    /// with a <see cref="SubscriptionEntity"/> representing the created or updated subscription if successful, or an error if the operation fails.</returns>
    public Task<Result<SubscriptionEntity>> CreateSubscription(SubscriptionContext preparation,
        CancellationToken cancellationToken)
    {
        return subscriptionRepository.GetSubscriptionDetailsForUser(preparation.UserID, cancellationToken)
            .BindTryWhen(entity => entity.SubscriptionStatus == SubscriptionStatus.Cancelled, entity =>
            {
                logger.LogWarning("Subscription already exists for user {userId}. It is cancelled, Resetting to pending", preparation.UserID);
                entity.SubscriptionStatus = SubscriptionStatus.Created;
                entity.Status = OrderStatus.Pending;
                entity.StartDate = DateTime.UtcNow;
                entity.CheckoutSessionUrl = preparation.CheckoutSession.Url;
                entity.CheckoutSessionExpiresAt = preparation.CheckoutSession.ExpiresAt;
                entity.ExternalCustomerID = preparation.ExternalCustomerID;
                return subscriptionRepository.SaveSubscriptionAsync(entity, cancellationToken);
            })
            .OnFailureCompensateWhen(error => error == SubscriptionErrors.NoSubscriptionFound, (_) =>
            {
                logger.LogInformation("No subscription found for user {userId}. Creating new subscription.", preparation.UserID);
                SubscriptionEntity entity = new()
                {
                    SubscriptionID = Guid.NewGuid().ToString(),
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    ExternalCustomerID = preparation.ExternalCustomerID,
                    UserID = preparation.UserID,
                    ExternalSubscriptionItemID = config.SubscriptionID,
                    Created = DateTime.UtcNow,
                    CheckoutSessionExpiresAt = preparation.CheckoutSession.ExpiresAt,
                    CheckoutSessionUrl = preparation.CheckoutSession.Url
                };
                return subscriptionRepository.SaveSubscriptionAsync(entity, cancellationToken);
            }).TapError(error => logger.LogError(error, "Error while creating subscription for user {userId}", preparation.UserID));
    }

    /// <summary>
    /// Creates a new subscription for the specified user based on the provided checkout session details.
    /// Updates the subscription context with user and session-specific information, and processes
    /// a subscription creation request in the repository.
    /// </summary>
    /// <param name="user">The user entity for whom the subscription is being created.</param>
    /// <param name="context">The subscription checkout session entity containing details about the subscription session.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/> object
    /// with a <see cref="SubscriptionEntity"/> representing the created subscription if successful, or an error if the operation fails.</returns>
    public Task<Result<SubscriptionEntity>> CreateSubscription(UserEntity user,
        SubscriptionCheckoutSessionEntity context, CancellationToken cancellationToken)
    {
        SubscriptionContext preparation = new()
        {
            UserID = user.Id,
            ExternalCustomerID = user.CustomerID,
            CheckoutSession = context
        };

        return CreateSubscription(preparation, cancellationToken);
    }

    /// <summary>
    /// Cancels the subscription for a specified user. Updates the subscription status to indicate
    /// that it is to be cancelled and invokes the payment repository to perform the cancellation.
    /// </summary>
    /// <param name="userID">The unique identifier of the user whose subscription is to be cancelled.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/> object
    /// with a <see cref="Unit"/> value if the operation succeeds, or an error if the operation fails.</returns>
    public Task<Result<Unit>> CancelSubscriptionForUser(string userID, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Canceling subscription for user {userID}");
        return GetSubscriptionForUser(userID, cancellationToken)
            .BindTry(subscriptionEntity =>
                paymentRepository.CancelSubscriptionForUser(subscriptionEntity, cancellationToken)
                    .Map(_ => subscriptionEntity))
            .BindTry(subscriptionEntity =>
            {
                subscriptionEntity.SubscriptionStatus = SubscriptionStatus.ToBeCancelled;
                return subscriptionRepository.SaveSubscriptionAsync(subscriptionEntity, cancellationToken);
            })
            .Map(_ => Unit.Value);
    }

    public Task<Result<SubscriptionEntity>> GetSubscriptionForUser(string userID, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Getting subscription details for user {userID}");
        return subscriptionRepository.GetSubscriptionDetailsForUser(userID, cancellationToken);
    }

    /// <summary>
    /// Reactivates a subscription for a given user. Validates the current subscription status
    /// and performs necessary operations to mark the subscription as active.
    /// </summary>
    /// <param name="userID">The unique identifier of the user whose subscription is to be reactivated.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/> object
    /// with a <see cref="Unit"/> value if the operation succeeds, or an error if the operation fails.</returns>
    public Task<Result<Unit>> ReactivateSubscription(string userID, CancellationToken cancellationToken)
    {
        return GetSubscriptionForUser(userID, cancellationToken)
            .Ensure(entity => entity.SubscriptionStatus == SubscriptionStatus.ToBeCancelled,
                SubscriptionErrors.SubscriptionIsActive)
            .Ensure(entity => entity.Status != OrderStatus.Cancelled && entity.Status != OrderStatus.PaymentFailed, SubscriptionErrors.SubscriptionIsCancelled)
            .BindTry(subscriptionEntity =>
                paymentRepository.ReactivateSubscriptionForUser(subscriptionEntity, cancellationToken)
                    .Map(_ => subscriptionEntity))
            .BindTry(subscriptionEntity =>
            {
                logger.LogInformation($"Reactivating subscription for user {subscriptionEntity.UserID} date {subscriptionEntity.StartDate}");
                if (subscriptionEntity.Status == OrderStatus.OrderDelayed && subscriptionEntity.StartDate > DateTime.UtcNow)
                {
                    logger.LogInformation($"Subscription for user {subscriptionEntity.UserID} is delayed. Restoring trialing");
                    subscriptionEntity.SubscriptionStatus = SubscriptionStatus.Trialing;    
                }
                else
                {
                    subscriptionEntity.SubscriptionStatus = SubscriptionStatus.Active;   
                }
                return subscriptionRepository.SaveSubscriptionAsync(subscriptionEntity, cancellationToken);
            })
            .Map(_ => Unit.Value);
    }

    /// <summary>
    /// Deletes the subscription associated with a given user. Removes the subscription record from the repository.
    /// </summary>
    /// <param name="userID">The unique identifier of the user whose subscription will be deleted.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/> object with a <see cref="Unit"/> value if the operation succeeds, or an error if the operation fails.</returns>
    public Task<Result<Unit>> DeleteSubscriptionForUser(string userID, CancellationToken cancellationToken)
    {
        return GetSubscriptionForUser(userID, cancellationToken)
            .BindTry(entity => subscriptionRepository.DeleteSubscriptionForUser(entity, cancellationToken));
    }
}
