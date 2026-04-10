using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Subscriptions.Ports.Out;
using VibraHeka.Domain.User.Services;

namespace VibraHeka.Application.Subscriptions.Queries.GetSubscriptionDetails;

public class GetSubscriptionDetailsQueryHandler(SubscriptionPort subscriptionService, ICurrentUserService currentUserService) : IRequestHandler<GetSubscriptionDetailsQuery, Result<SubscriptionEntity>> 
{
    public Task<Result<SubscriptionEntity>> Handle(GetSubscriptionDetailsQuery request, CancellationToken cancellationToken)
    {
        return subscriptionService.GetSubscriptionForUser(currentUserService.UserId!, cancellationToken);
    }
}
