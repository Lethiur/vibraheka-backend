using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Extensions.Results;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.Subscriptions.Commands.AddSubscription;

public class AddSubscriptionCommandHandler(
    ICurrentUserService currentUserService,
    IPaymentService paymentService,
    ISubscriptionService subscriptionService,
    ILogger<AddSubscriptionCommandHandler> logger) : IRequestHandler<AddSubscriptionCommand, Result<SubscriptionCheckoutSessionEntity>>
{
    public async Task<Result<SubscriptionCheckoutSessionEntity>> Handle(AddSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        string userId = currentUserService.UserId!;
        logger.LogInformation("Executing command for creating a subscription for the user: {UserId}", userId);

        return await paymentService.PrepareSubscriptionAsync(userId, cancellationToken)
            .TapError(error => logger.LogWarning(
                "Error occurred while preparing subscription for user {UserId}. Error: {Error}",
                userId,
                error))
            .BindTry(async context =>
                await subscriptionService.CreateSubscription(context, cancellationToken)
                    .TapError(error => logger.LogWarning(
                        "Error occurred while creating subscription for user {UserId}. Error: {Error}",
                        userId,
                        error))
                    .Map(_ => context.CheckoutSession)
                    .OnFailureCompensate(async error =>
                    {
                        Result<Unit> rollbackResult =
                            await paymentService.CancelSubscriptionPayment(context.CheckoutSession, cancellationToken);

                        rollbackResult.TapError(rollbackError => logger.LogError(
                            "Failed to rollback checkout session for user {UserId}. Error: {Error}",
                            userId,
                            rollbackError));

                        return Result.Failure<SubscriptionCheckoutSessionEntity>(error);
                    }))
            .Tap(_ => logger.LogInformation("Subscription prepared for user {UserId}", userId))
            .MapError(_ => SubscriptionErrors.ErrorWhileSubscribing);
    }
}
