using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Extensions.Results;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.Subscriptions.Commands.AddSubscription;

public class AddSubscriptionCommandHandler(
    ICurrentUserService currentUserService,
    IPaymentService paymentService,
    IUserService userService,
    ISubscriptionService subscriptionService,
    ILogger<AddSubscriptionCommandHandler> logger) : IRequestHandler<AddSubscriptionCommand, Result<string>>
{
    public async Task<Result<string>> Handle(AddSubscriptionCommand request, CancellationToken cancellationToken)
    {
        string userId = currentUserService.UserId!;
        logger.LogInformation("Executing command for creating a subscription for the user: {UserId}", userId);

        return await userService.GetUserByID(userId, cancellationToken)
            .Tap(user => logger.LogInformation("User {UserId} found", user.Id))
            .BindTry(user => subscriptionService.CreateSubscription(user, cancellationToken))
            .Tap(context =>
                logger.LogInformation("Subscription prepared for user {UserId}", context.UserID))
            .BindTry(subscription => paymentService.RegisterSubscriptionAsync(userId, subscription, cancellationToken))
            .TapError( error => subscriptionService.DeleteSubscriptionForUser(userId, cancellationToken))
            .MapError(error =>
            {
                logger.LogWarning("Error occurred while creating subscription for user {UserId}. Error: {Error}",
                    userId, error);
                return SubscriptionErrors.ErrorWhileSubscribing;
            });
    }
}
