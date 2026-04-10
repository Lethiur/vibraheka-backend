using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Subscriptions.Entities;

namespace VibraHeka.Domain.Subscriptions.Ports.Out;

public interface PaymentsPort
{
    /// <summary>
    /// Initiates a subscription payment for a user and returns details of the checkout session.
    /// </summary>
    /// <param name="gatewayCustomerID">The unique identifier of the user requesting the subscription payment.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the method to complete, enabling cancellation.</param>
    /// <returns>
    /// A result containing a <see cref="SubscriptionCheckoutSessionEntity"/> object with the details of the
    /// checkout session if the operation is successful, or an error result otherwise.
    /// </returns>
    Task<Result<SubscriptionCheckoutSessionEntity>> InitiateSubscriptionPaymentAsync(string gatewayCustomerID, CancellationToken cancellationToken);
    
    /// <summary>
    /// Registers a customer by linking their profile details to the subscription system.
    /// </summary>
    /// <param name="entity">The user profile entity containing customer details to be registered.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the operation, containing a result object encapsulating success or failure.</returns>
    public Task<Result<string>> RegisterCustomerAsync(UserProfileEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the URL of the subscription management panel for a user.
    /// </summary>
    /// <param name="externalCustomerID">The unique identifier of the user requesting the subscription panel URL.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the method to complete, enabling cancellation.</param>
    /// <returns>
    /// A result containing the URL of the subscription management panel as a string if the operation is successful, or an error result otherwise.
    /// </returns>
    Task<Result<string>> GetSubscriptionPanelUrlAsync(string externalCustomerID, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels an active user subscription and updates its status in the system.
    /// </summary>
    /// <param name="externalSubscriptionID">The external ID of the subscription to cancel.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the method to complete, enabling cancellation.</param>
    /// <returns>
    /// A result indicating the success or failure of the subscription cancellation operation.
    /// </returns>
    Task<Result<Unit>> CancelSubscription(string externalSubscriptionID, CancellationToken cancellationToken);

    /// <summary>
    /// Reactivates a previously canceled subscription for a user.
    /// </summary>
    /// <param name="externalSubscriptionID"></param>
    /// <param name="cancellationToken">A token to observe while waiting for the method to complete, enabling cancellation.</param>
    /// <returns>
    /// A result indicating success or failure of the operation. If successful, a <see cref="Unit"/> value is returned.
    /// </returns>
    Task<Result<Unit>> ReactivateSubscription(string externalSubscriptionID, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels an existing subscription payment session for a user.
    /// </summary>
    /// <param name="gatewaySubscriptionID">The unique identifier of the subscription payment session to be canceled.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the method to complete, enabling cancellation.</param>
    /// <returns>
    /// A result indicating the success or failure of the cancellation operation.
    /// </returns>
    Task<Result<Unit>> CancelCheckoutSession(string sessionID, CancellationToken cancellationToken);
}
