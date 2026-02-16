using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.Services;

public class SubscriptionService(ISubscriptionRepository subscriptionRepository, IPaymentRepository paymentRepository, StripeConfig config) : ISubscriptionService
{
    public Task<Result<SubscriptionEntity>> CreateSubscription(UserEntity user, CancellationToken cancellationToken)
    {
        SubscriptionEntity subscription = new()
        {
             SubscriptionID = Guid.NewGuid().ToString(),
             StartDate = DateTime.UtcNow,
             ExternalCustomerID = user.CustomerID,
             UserID = user.Id,
             ExternalSubscriptionItemID = config.SubscriptionID,
             Created = DateTime.UtcNow,
             CreatedBy = user.CreatedBy,
        };
        
        return subscriptionRepository.SaveSubscriptionAsync(subscription, cancellationToken);
    }

    public Task<Result<Unit>> CancelSubscriptionForUser(string userID, CancellationToken cancellationToken)
    {
        return GetSubscriptionForUser(userID, cancellationToken)
            .BindTry(subscriptionEntity =>
            {
                subscriptionEntity.SubscriptionStatus = SubscriptionStatus.ToBeCancelled;
                return subscriptionRepository.SaveSubscriptionAsync(subscriptionEntity, cancellationToken);
            })
            .BindTry(subscriptionEntity => paymentRepository.CancelSubscriptionForUser(subscriptionEntity, cancellationToken))
            .Map(_ => Unit.Value);
    }

    public Task<Result<SubscriptionEntity>> GetSubscriptionForUser(string userID, CancellationToken cancellationToken)
    {
        return subscriptionRepository.GetSubscriptionDetailsForUser(userID, cancellationToken);
    }
}
