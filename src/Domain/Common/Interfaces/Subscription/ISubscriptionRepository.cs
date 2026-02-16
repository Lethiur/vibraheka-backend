using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Common.Interfaces.Orders;

/// <summary>
/// Defines a repository interface for managing orders, including retrieving order statuses.
/// </summary>
public interface ISubscriptionRepository
{
    /// <summary>
    /// Retrieves the current order status for a specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose order status will be retrieved.</param>
    /// <param name="cancellationToken">The token used to halt the operation before it finishes</param>
    /// <returns>A task that represents the asynchronous operation, containing the retrieved order status as an <see cref="OrderStatus"/> enum.</returns>
    Task<Result<SubscriptionEntity>> GetSubscriptionDetailsForUser(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Saves a subscription for the specified payer and order entity.
    /// </summary>
    /// <param name="subscriptionEntity">The subscription entity containing the details of the subscription to be saved.</param>
    /// <param name="cancellationToken">The token used to halt the operation before it finishes</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the operation as a <see cref="SubscriptionEntity"/>.</returns>
    Task<Result<SubscriptionEntity>> SaveSubscriptionAsync(SubscriptionEntity subscriptionEntity, CancellationToken cancellationToken);
}
