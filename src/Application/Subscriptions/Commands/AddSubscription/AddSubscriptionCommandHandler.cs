using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.Subscriptions.Commands.AddSubscription;

public class AddSubscriptionCommandHandler(
    ICurrentUserService currentUserService,
    IPaymentService paymentService,
    IUserService userService,
    ISubscriptionService subscriptionService,
    ILogger<AddSubscriptionCommandHandler> logger) : IRequestHandler<AddSubscriptionCommand, Result<SubscriptionCheckoutSessionEntity>>
{
    public async Task<Result<SubscriptionCheckoutSessionEntity>> Handle(AddSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        string userId = currentUserService.UserId!;
        logger.LogInformation("Executing command for creating a subscription for the user: {UserId}", userId);

        Result<UserEntity> userResult = await userService.GetUserByID(userId, cancellationToken);

        if (userResult.IsFailure)
        {
            logger.LogWarning("Error occurred while creating subscription for user {UserId}. Error: {Error}",
                userId, userResult.Error);
            return Result.Failure<SubscriptionCheckoutSessionEntity>(SubscriptionErrors.ErrorWhileSubscribing);
        }

        Result<SubscriptionCheckoutSessionEntity> paymentResult =
            await paymentService.RegisterSubscriptionAsync(userResult.Value.Id, cancellationToken);

        if (paymentResult.IsFailure)
        {
            logger.LogWarning("Error occurred while creating subscription for user {UserId}. Error: {Error}",
                userId, paymentResult.Error);
            return Result.Failure<SubscriptionCheckoutSessionEntity>(SubscriptionErrors.ErrorWhileSubscribing);
        }

        SubscriptionCheckoutSessionEntity session = paymentResult.Value;

        Result<SubscriptionEntity> subscriptionResult =
            await subscriptionService.CreateSubscription(userResult.Value, session, cancellationToken);

        if (subscriptionResult.IsFailure)
        {
            Result<Unit> rollbackResult = await paymentService.CancelSubscriptionPayment(session, cancellationToken);
            if (rollbackResult.IsFailure)
            {
                logger.LogError("Failed to rollback checkout session for user {UserId}. Error: {Error}",
                    userId, rollbackResult.Error);
            }

            logger.LogWarning("Error occurred while creating subscription for user {UserId}. Error: {Error}",
                userId, subscriptionResult.Error);
            return Result.Failure<SubscriptionCheckoutSessionEntity>(SubscriptionErrors.ErrorWhileSubscribing);
        }

        logger.LogInformation("Subscription prepared for user {UserId}", userResult.Value.Id);
        return Result.Success(session);
    }
}
