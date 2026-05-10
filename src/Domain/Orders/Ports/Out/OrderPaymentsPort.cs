using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

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
}
