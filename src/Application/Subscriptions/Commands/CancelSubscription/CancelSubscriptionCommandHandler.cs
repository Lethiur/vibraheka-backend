using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Orders;

namespace VibraHeka.Application.Subscriptions.Commands.CancelSubscription;

public class CancelSubscriptionCommandHandler(ICurrentUserService currentUserService, ISubscriptionService subscriptionService) : IRequestHandler<CancelSubscriptionCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        return subscriptionService.CancelSubscriptionForUser(currentUserService.UserId!, cancellationToken);
    }
}
