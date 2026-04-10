using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Subscriptions.Ports.Out;
using VibraHeka.Domain.User.Services;

namespace VibraHeka.Application.Subscriptions.Commands.ReactivateSubscription;

public class ReactivateSubscriptionCommandHandler(
    ICurrentUserService currentUserService,
    SubscriptionPort subscriptionPort,
    PaymentsPort paymentsPort) : IRequestHandler<ReactivateSubscriptionCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(ReactivateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        return subscriptionPort
            .GetSubscriptionForUser(currentUserService.UserId!, cancellationToken)
            .BindTry(entity => ReactivateBothSides(entity, cancellationToken));
    }

    private async Task<Result<Unit>> ReactivateBothSides(SubscriptionEntity entity, CancellationToken cancellationToken)
    {
        entity.Reactivate();
        Result<Unit> cancelSubscription = await subscriptionPort.SetSubscriptionStatus(entity.SubscriptionID, entity.SubscriptionStatus, cancellationToken);

        if (cancelSubscription.IsFailure)
        {
            return cancelSubscription;
        }
        
        return await paymentsPort.ReactivateSubscription(entity.ExternalSubscriptionID, cancellationToken);
    }
}
