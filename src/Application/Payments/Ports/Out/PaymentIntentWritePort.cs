using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Domain.Payments.Entities;

namespace VibraHeka.Application.Payments.Ports.Out;

/// <summary>
/// Represents a port for writing and creating payment intent operations.
/// Facilitates the creation of payment intents by interacting with lower-level transactional
/// components and domain entities related to payments.
/// </summary>
public interface IPaymentAttemptWritePort
{
    /// <summary>
    /// Creates a new payment intent by invoking the transactional write operation for
    /// the given payment attempt entity.
    /// </summary>
    /// <param name="paymentIntent">
    /// The payment attempt entity containing details such as user ID, order ID, payment provider,
    /// status, amount, and related payment gateway information.
    /// </param>
    /// <returns>
    /// An instance of a transactional write operation that represents the result of the creation process.
    /// </returns>
    ITransactionalWriteOperation CreatePaymentAttempt(PaymentAttemptEntity paymentIntent);
}
