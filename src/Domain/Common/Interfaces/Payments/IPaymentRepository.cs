using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Common.Interfaces.Payments;

/// <summary>
/// Defines a contract for payment-related operations.
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Initiates a subscription payment for the specified user and subscription order.
    /// </summary>
    /// <param name="payer">The user entity representing the payer initiating the subscription payment.</param>
    /// <param name="orderEntity">The subscription entity representing the subscription order details.</param>
    /// <param name="cancellationToken">Token used to halt the operation before it finishes</param>
    /// <returns>A task that represents the asynchronous operation, containing the payment initiation response as a string.</returns>
    Task<Result<(string url, string externalSubID)>> InitiateSubscriptionPaymentAsync(UserEntity payer,
        SubscriptionEntity orderEntity, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the URL of the subscription panel for the specified user.
    /// </summary>
    /// <param name="payer">The user entity representing the user requesting access to the subscription panel.</param>
    /// <param name="cancellationToken">Token to halt the async op before it finishes</param>
    /// <returns>A task that represents the asynchronous operation, containing the subscription panel URL as a string.</returns>
    Task<Result<string>> GetSubscriptionPanelUrlAsync(UserEntity payer, CancellationToken cancellationToken);

    /// <summary>
    /// Registers a new customer in the system based on the provided user entity details.
    /// </summary>
    /// <param name="user">The user entity containing the details for the customer to be registered.</param>
    /// <param name="cancellationToken">Token used to halt the operation before it finishes.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the customer registration process as a string.</returns>
    Task<Result<string>> RegisterCustomerAsync(UserEntity user, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels an active subscription for the specified user and subscription.
    /// </summary>
    /// <param name="subscription">The subscription entity representing the subscription to be canceled.</param>
    /// <param name="cancellationToken">Token used to halt the operation before it completes.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the cancellation process as a unit value.</returns>
    Task<Result<Unit>> CancelSubscriptionForUser(SubscriptionEntity subscription, CancellationToken cancellationToken);
}
