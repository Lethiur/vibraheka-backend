using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Orders;

namespace VibraHeka.Application.Subscriptions.Commands.ReactivateSubscription;

public class ReactivateSubscriptionCommandHandler(
    ICurrentUserService currentUser,
    ISubscriptionService subscriptionService) : IRequestHandler<ReactivateSubscriptionCommand, Result<Unit>>
{
    public  Task<Result<Unit>> Handle(ReactivateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        return subscriptionService.ReactivateSubscription(currentUser.UserId!, cancellationToken).Map(_ => Unit.Value);
    }
}
