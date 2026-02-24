using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Subscriptions.Queries.GetSubscriptionDetails;

public class GetSubscriptionDetailsQueryHandler(ISubscriptionService subscriptionService, ICurrentUserService currentUserService) : IRequestHandler<GetSubscriptionDetailsQuery, Result<SubscriptionEntity>> 
{
    public Task<Result<SubscriptionEntity>> Handle(GetSubscriptionDetailsQuery request, CancellationToken cancellationToken)
    {
        return subscriptionService.GetSubscriptionForUser(currentUserService.UserId!, cancellationToken);
    }
}
