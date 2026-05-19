using CSharpFunctionalExtensions;
using VibraHeka.Application.Payments.Models;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Payments.Entities;

namespace VibraHeka.Domain.Payments.Ports.Out;

/// <summary>
/// Provides an interface for managing payment processes and customer registration
/// with external payment systems.
/// </summary>
public interface IPaymentsPort
{
    /// <summary>
    /// Registers a customer with an external payment system based on the provided user details.
    /// </summary>
    /// <param name="user">
    /// A reference to the user entity containing user details necessary for customer registration.
    /// </param>
    /// <param name="token">
    /// A cancellation token that can be used to observe or cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a
    /// <see cref="CSharpFunctionalExtensions.Result{T}"/> object where T is a string indicating
    /// the registered customer ID if the operation is successful, or an error message in case of failure.
    /// </returns>
    public Task<Result<string>> RegisterCustomerAsync(ref readonly UserEntity user, CancellationToken token);

    /// <summary>
    /// Initiates the payment process for a given order using an external payment provider.
    /// </summary>
    /// <param name="checkoutModel">
    /// An object containing the order details and customer information needed to create a payment session.
    /// </param>
    /// <param name="token">
    /// A cancellation token that can be used to observe or cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a
    /// <see cref="CSharpFunctionalExtensions.Result{T}"/> object where T is the
    /// <see cref="VibraHeka.Domain.Payments.Entities.PaymentAttemptEntity"/> containing details
    /// of the created payment attempt if the operation succeeds, or an error message in case of failure.
    /// </returns>
    public Task<Result<PaymentAttemptEntity>> StartPaymentProcessAsync(CheckoutOrderModel checkoutModel,
        CancellationToken token);
}
