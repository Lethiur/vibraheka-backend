using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Orders.Models;

namespace VibraHeka.Domain.Orders.Ports.Out;

/// <summary>
/// Represents a secondary port interface for handling payment-related operations.
/// </summary>
public interface IPaymentsPort
{
    /// <summary>
    /// Registers a customer using the provided user information.
    /// </summary>
    /// <param name="user">
    /// The <see cref="UserEntity"/> containing customer information to be registered.
    /// </param>
    /// <param name="token">
    /// A <see cref="CancellationToken"/> to observe cancellation requests.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/>
    /// with a <see cref="string"/> indicating the outcome of the registration process.
    /// </returns>
    public Task<Result<string>> RegisterCustomerAsync(UserEntity user, CancellationToken token);

    /// <summary>
    /// Creates a checkout session for the specified product model.
    /// </summary>
    /// <param name="model">
    /// The <see cref="CheckoutProductModel"/> containing details of the product to be included in the checkout session.
    /// </param>
    /// <param name="token">
    /// A <see cref="CancellationToken"/> to propagate notification that operations should be canceled.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/>
    /// with a <see cref="CheckoutSessionCompletedModel"/> providing the details of the completed checkout session.
    /// </returns>
    public Task<Result<CheckoutSessionCompletedModel>> CreateCheckoutSessionAsync(CheckoutProductModel model,
        CancellationToken token);
}
