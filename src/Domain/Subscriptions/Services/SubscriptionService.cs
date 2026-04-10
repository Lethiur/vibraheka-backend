using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.Subscriptions.Ports.Out;

namespace VibraHeka.Domain.Subscriptions.Services;

public class SubscriptionService(SubscriptionPort subscriptionPort)
{
    public bool CanSubscriptionBeCreated(SubscriptionEntity subscription)
    {
        return subscription.SubscriptionStatus == SubscriptionStatus.Cancelled;
    }

    public async Task<Result<SubscriptionEntity>> CreateSubscriptionEntityForUser( string userId, CancellationToken cancellationToken)
    {
        Result<SubscriptionEntity> subscriptionForUser = await subscriptionPort.GetSubscriptionForUser(userId, cancellationToken);

        if (subscriptionForUser.IsFailure && subscriptionForUser.Error == SubscriptionErrors.NoSubscriptionFound)
        {
            return new SubscriptionEntity()
            {
                UserID = userId,
                SubscriptionStatus = SubscriptionStatus.Created,
                Status = OrderStatus.Pending,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                SubscriptionID = Guid.NewGuid().ToString(),
            };
        }

        return subscriptionForUser;
    }
    
}
