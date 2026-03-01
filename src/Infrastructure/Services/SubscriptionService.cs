using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Extensions.Results;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.Services;

public class SubscriptionService(
    ISubscriptionRepository subscriptionRepository,
    IPaymentRepository paymentRepository,
    StripeConfig config,
    ILogger<SubscriptionService> logger) : ISubscriptionService
{
    public Task<Result<SubscriptionEntity>> CreateSubscription(UserEntity user, SubscriptionCheckoutSessionEntity context, CancellationToken cancellationToken)
    {
        return subscriptionRepository.GetSubscriptionDetailsForUser(user.Id, cancellationToken)
            .BindTryWhen(entity => entity.SubscriptionStatus == SubscriptionStatus.Cancelled, entity =>
            {
                logger.LogWarning("Subscription already exists for user {userId}. It is cancelled, Resetting to pending", user.Id);
                entity.SubscriptionStatus = SubscriptionStatus.Created;
                entity.Status = OrderStatus.Pending;
                entity.StartDate = DateTime.UtcNow;
                entity.CheckoutSessionUrl = context.Url;
                entity.CheckoutSessionExpiresAt = context.ExpiresAt;
                return subscriptionRepository.SaveSubscriptionAsync(entity, cancellationToken);
            })
            .OnFailureCompensateWhen(error => error == SubscriptionErrors.NoSubscriptionFound, (_) =>
            {
                logger.LogInformation("No subscription found for user {userId}. Creating new subscription.", user.Id);
                SubscriptionEntity entity = new()
                {
                    SubscriptionID = Guid.NewGuid().ToString(),
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    ExternalCustomerID = user.CustomerID,
                    UserID = user.Id,
                    ExternalSubscriptionItemID = config.SubscriptionID,
                    Created = DateTime.UtcNow,
                    CreatedBy = user.CreatedBy,
                    CheckoutSessionExpiresAt = context.ExpiresAt,
                    CheckoutSessionUrl = context.Url
                };
                return subscriptionRepository.SaveSubscriptionAsync(entity, cancellationToken);
            }).TapError(error => logger.LogError(error, "Error while creating subscription for user {userId}", user.Id));
    }

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

    public Task<Result<SubscriptionEntity>> SaveSubscription(SubscriptionEntity subscriptionEntity, CancellationToken cancellationToken)
    {
        return subscriptionRepository.SaveSubscriptionAsync(subscriptionEntity, cancellationToken);
    }

    public Task<Result<Unit>> ReactivateSubscription(string userID, CancellationToken cancellationToken)
    {
        return GetSubscriptionForUser(userID, cancellationToken)
            .Ensure(entity => entity.SubscriptionStatus == SubscriptionStatus.ToBeCancelled,
                SubscriptionErrors.SubscriptionIsActive)
            .BindTry(subscriptionEntity =>
                paymentRepository.ReactivateSubscriptionForUser(subscriptionEntity, cancellationToken)
                    .Map(_ => subscriptionEntity))
            .BindTry(subscriptionEntity =>
            {
                subscriptionEntity.SubscriptionStatus = SubscriptionStatus.Active;
                return subscriptionRepository.SaveSubscriptionAsync(subscriptionEntity, cancellationToken);
            })
            .Map(_ => Unit.Value);
    }

    public Task<Result<Unit>> DeleteSubscriptionForUser(string userID, CancellationToken cancellationToken)
    {
        return GetSubscriptionForUser(userID, cancellationToken)
            .BindTry(entity => subscriptionRepository.DeleteSubscriptionForUser(entity, cancellationToken));
    }

    public Task<Result<Unit>> MarkSubscriptionAsPaymentFailedForUser(string userID, CancellationToken cancellationToken)
    {
        logger.LogInformation("Marking subscription as payment failed for user {userID}", userID);
        return GetSubscriptionForUser(userID, cancellationToken)
            .BindTry(subscriptionEntity =>
            {
                subscriptionEntity.Status = OrderStatus.PaymentFailed;
                subscriptionEntity.SubscriptionStatus = SubscriptionStatus.Inactive;
                return subscriptionRepository.SaveSubscriptionAsync(subscriptionEntity, cancellationToken);
            })
            .Map(_ => Unit.Value);
    }
}
