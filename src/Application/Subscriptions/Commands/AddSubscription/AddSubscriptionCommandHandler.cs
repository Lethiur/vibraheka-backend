using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.Subscriptions.Entities;
using VibraHeka.Domain.Subscriptions.Ports.Out;
using VibraHeka.Domain.Subscriptions.Services;
using VibraHeka.Domain.User.Ports.Output;
using VibraHeka.Domain.User.Services;

namespace VibraHeka.Application.Subscriptions.Commands.AddSubscription;

public class AddSubscriptionCommandHandler(
    ICurrentUserService currentUserService,
    PaymentsPort paymentsPort,
    SubscriptionService subscriptionService,
    SubscriptionPort subscriptionPort,
    UserProfilePort userProfilePort,
    ILogger<AddSubscriptionCommandHandler> logger) :
    IRequestHandler<AddSubscriptionCommand, Result<SubscriptionCheckoutSessionEntity>>
{
    public async Task<Result<SubscriptionCheckoutSessionEntity>> Handle(AddSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        string userId = currentUserService.UserId!;
        logger.LogInformation("Executing command for creating a subscription for the user: {UserId}", userId);

        Result<string> customerIdResult = await userProfilePort.GetProfileByUserId(userId, cancellationToken)
            .BindTry(entity =>RegisterCustomerAgainstAndSavePaymentsPort(entity, cancellationToken) );
        
        if (customerIdResult.IsFailure)
        {
            return Result.Failure<SubscriptionCheckoutSessionEntity>(SubscriptionErrors.ErrorWhileSubscribing);
        }
        
        Result<SubscriptionCheckoutSessionEntity> initiateSubscriptionPaymentAsync =
            await paymentsPort.InitiateSubscriptionPaymentAsync(customerIdResult.Value, cancellationToken);

        if (initiateSubscriptionPaymentAsync.IsFailure)
        {
            logger.LogWarning("Error occurred while preparing subscription for user {UserId}. Error: {Error}",
                userId, initiateSubscriptionPaymentAsync.Error);
            return Result.Failure<SubscriptionCheckoutSessionEntity>(SubscriptionErrors.ErrorWhileSubscribing);
        }

        Result<SubscriptionEntity> subscriptionEntityResult =
            await subscriptionService.CreateSubscriptionEntityForUser(userId, cancellationToken)
                .Tap(entity => entity.PrepareForCheckout(initiateSubscriptionPaymentAsync.Value));

        if (subscriptionEntityResult.IsFailure)
        {
            logger.LogError("Error occurred while creating subscription entity for user {UserId}. Error: {Error}",
                userId, subscriptionEntityResult.Error);
            return Result.Failure<SubscriptionCheckoutSessionEntity>(SubscriptionErrors.ErrorWhileSubscribing);
        }

        SubscriptionEntity subscriptionEntity = subscriptionEntityResult.Value;

        Result<Unit> result = await subscriptionPort.CreateSubscription(subscriptionEntity, cancellationToken);
        if (result.IsFailure)
        {
            logger.LogWarning("Error occurred while creating subscription for user {UserId}. Error: {Error}",
                userId, result.Error);
            await paymentsPort
                .CancelCheckoutSession(subscriptionEntity.CheckoutSessionID, cancellationToken)
                .TapError(rollbackError =>
                    logger.LogCritical("Failed to rollback checkout session for user {UserId}. Error: {Error}", userId,
                        rollbackError));

            return Result.Failure<SubscriptionCheckoutSessionEntity>(SubscriptionErrors.ErrorWhileSubscribing);
        }


        return initiateSubscriptionPaymentAsync;
    }

    /// <summary>
    /// Registers a customer in the payments system and updates the user's profile with the assigned customer ID.
    /// If the user profile already has a CustomerID, the operation will fail.
    /// </summary>
    /// <param name="entity">The user profile entity containing the user's information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A result indicating the success or failure of the operation.
    /// On success, returns a <see cref="Result{Unit}"/>.
    /// On failure, returns an error code, such as "US-017" if the profile already has a CustomerID.
    /// </returns>
    private async Task<Result<string>> RegisterCustomerAgainstAndSavePaymentsPort(UserProfileEntity entity,
        CancellationToken cancellationToken)
    {
        if (!entity.IsConnectedToPaymentsGateway())
        {
            return await paymentsPort.RegisterCustomerAsync(entity, cancellationToken)
                .TapError(error => logger.LogError(
                    "Failed to register customer for user {UserId}. Error: {Error}",
                    entity.Id,
                    error))
                .Tap(entity.SetExternalCustomerId)
                .BindTry(customerId => userProfilePort.UpdateUserProfile(entity, entity.Id, cancellationToken))
                .Map(_ => entity.CustomerID);
        }

        return Result.Success<string>(entity.CustomerID);
    }
}
