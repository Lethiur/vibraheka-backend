using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
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
        logger.LogInformation($"Executing command for creating a subscription for the user: {currentUserService.UserId}");
        
        Result<string> task = await userService.GetUserByID(currentUserService.UserId!, cancellationToken)
            .Tap(user => logger.LogInformation($"User {user.Id} found"))
            .BindTry(userEntity => subscriptionService.CreateSubscription(userEntity, cancellationToken))
            .Tap(subscriptionEntity => logger.LogInformation($"Subscription created for user {subscriptionEntity.UserID}"))
            .BindTry(s =>
                paymentService.RegisterSubscriptionAsync(currentUserService.UserId!, s, cancellationToken))
            .Tap(url => logger.LogInformation($"Redirecting to payment gateway for subscription: {url}"));


        if (task.IsFailure)
        {
            logger.LogWarning(task.Error, "Error occurred while creating subscription");
            await subscriptionService.DeleteSubscriptionForUser(currentUserService.UserId!, cancellationToken);
            return task.MapError(error => SubscriptionErrors.ErrorWhileSubscribing);
        }

        return task;
    }
}
