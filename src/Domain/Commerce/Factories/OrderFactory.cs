using NMoneys;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Commerce.Enums;

namespace VibraHeka.Application.Commerce.Factories;

/// <summary>
/// A factory class responsible for creating and initializing instances of the <see cref="OrderEntity"/> class.
/// Provides methods to configure default properties for new orders tailored to specific use cases.
/// </summary>
public static class OrderFactory
{
    /// <summary>
    /// Creates a new order for the specified user with default properties set.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom the order is being created.</param>
    /// <returns>An instance of <see cref="OrderEntity"/> initialized with default values for the specified user.</returns>
    public static OrderEntity ForUser(string userId)
    {
        return new OrderEntity
        {
            OrderID = Guid.NewGuid().ToString(),
            UserID = userId,
            Status = OrderStatus.Draft,
            Lines = [],
            Subtotal = Money.Zero(),
            Total = Money.Zero(),
            TaxTotal = Money.Zero(),
            DiscountTotal = Money.Zero(),
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = userId,
            LastModifiedBy = userId,
        };
    }
}
