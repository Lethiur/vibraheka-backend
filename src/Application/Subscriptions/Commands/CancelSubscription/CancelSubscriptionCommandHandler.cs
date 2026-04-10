using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Subscriptions.Ports.Out;
using VibraHeka.Domain.User.Services;

namespace VibraHeka.Application.Subscriptions.Commands.CancelSubscription;

public class CancelSubscriptionCommandHandler(
    ICurrentUserService currentUserService,
    SubscriptionPort subscriptionPort,
    PaymentsPort paymentsPort) : IRequestHandler<CancelSubscriptionCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        return subscriptionPort
            .GetSubscriptionForUser(currentUserService.UserId!, cancellationToken)
            .BindTry(entity => CancelBothSides(entity, cancellationToken));
    }

    private async Task<Result<Unit>> CancelBothSides(SubscriptionEntity entity, CancellationToken cancellationToken)
    {
        entity.MarkAsCancelled();
        Result<Unit> cancelSubscription = await subscriptionPort.SetSubscriptionStatus(entity.SubscriptionID, entity.SubscriptionStatus, cancellationToken);

        if (cancelSubscription.IsFailure)
        {
            return cancelSubscription;
        }
        
        return await paymentsPort.CancelSubscription(entity.ExternalSubscriptionID, cancellationToken);
    }
}
