using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Common.Interfaces.Payments;

/// <summary>
/// Defines the contract for payment services, including subscription management
/// and retrieval of subscription-related details.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Prepares the subscription payment context and guarantees the user has an external Stripe customer.
    /// </summary>
    /// <param name="userID">The unique identifier of the user registering the subscription.</param>
    /// <param name="cancellationToken">A token to observe cancellation requests.</param>
    /// <returns>
    /// A result containing all required data to persist a pending subscription and expose the checkout session.
    /// </returns>
    Task<Result<SubscriptionContext>> PrepareSubscriptionAsync(string userID, CancellationToken cancellationToken);

    /// <summary>
    /// Registers a subscription for a user with the given subscription details.
    /// </summary>
    /// <param name="userID">The unique identifier of the user registering the subscription.</param>
    /// <param name="cancellationToken">A token to observe cancellation requests.</param>
    /// <returns>A result containing the generated checkout URL upon success, or an error message upon failure.</returns>
    Task<Result<SubscriptionCheckoutSessionEntity>> RegisterSubscriptionAsync(string userID,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the URL for accessing the subscription details page associated with a specific user.
    /// </summary>
    /// <param name="userID">The unique identifier of the user whose subscription details URL is to be retrieved.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation if required.</param>
    /// <returns>A result containing the subscription details URL if successful, or an error message if the operation fails.</returns>
    Task<Result<string>> GetSubscriptionDetailsUrlAsync(string userID, CancellationToken cancellationToken);

    Task<Result<Unit>> CancelSubscriptionPayment(SubscriptionCheckoutSessionEntity entity,
        CancellationToken token);
}
