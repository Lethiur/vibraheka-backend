using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Common.Interfaces.Orders;

public interface ISubscriptionService
{
    /// <summary>
    /// Creates a subscription for the specified user.
    /// </summary>
    /// <param name="user">The user entity for which the subscription is being created.</param>
    /// <returns>A task that represents the operation, containing the subscription ID as a string.</returns>
    public Task<Result<SubscriptionEntity>> CreateSubscription(UserEntity user, CancellationToken cancellationToken);


    /// <summary>
    /// Cancels a subscription for the specified user.
    /// </summary>
    /// <param name="userID"></param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the operation, containing a result object encapsulating success or failure.</returns>
    Task<Result<Unit>> CancelSubscriptionForUser(string userID, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the subscription details for the specified user.
    /// </summary>
    /// <param name="userID">The unique identifier of the user whose subscription details are to be retrieved.</param>
    /// <param name="cancellationToken">The token used to cancel the operation if needed.</param>
    /// <returns>A task that represents the operation, containing the subscription entity associated with the specified user.</returns>
    public Task<Result<SubscriptionEntity>> GetSubscriptionForUser(string userID, CancellationToken cancellationToken);
    
}
