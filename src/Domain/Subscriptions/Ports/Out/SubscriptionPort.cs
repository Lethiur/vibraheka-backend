using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Subscriptions.Ports.Out;

public interface SubscriptionPort
{
    /// <summary>
    /// Creates a subscription for the specified user.
    /// </summary>
    /// <param name="subscription">The user entity for which the subscription is being created.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represents the operation, containing the subscription ID as a string.</returns>
    public Task<Result<Unit>> CreateSubscription(SubscriptionEntity subscription, CancellationToken cancellationToken);


    /// <summary>
    /// Updates the status of the specified subscription.
    /// </summary>
    /// <param name="subscriptionID">The unique identifier of the subscription whose status is to be updated.</param>
    /// <param name="status">The new status to be assigned to the subscription.</param>
    /// <param name="cancellationToken">The token used to cancel the operation if needed.</param>
    /// <returns>A task that represents the operation, containing a result that indicates the success or failure of the operation.</returns>
    Task<Result<Unit>> SetSubscriptionStatus(string subscriptionID, SubscriptionStatus status, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the subscription details for the specified user.
    /// </summary>
    /// <param name="subscriptionID">The unique identifier of the user whose subscription details are to be retrieved.</param>
    /// <param name="cancellationToken">The token used to cancel the operation if needed.</param>
    /// <returns>A task that represents the operation, containing the subscription entity associated with the specified user.</returns>
    public Task<Result<SubscriptionEntity>> GetSubscription(string subscriptionID, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the subscription details for a specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose subscription details are being retrieved.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the operation, containing the subscription entity associated with the user, or an error result if the operation fails.</returns>
    Task<Result<SubscriptionEntity>> GetSubscriptionForUser(string userId, CancellationToken cancellationToken);
    
}
