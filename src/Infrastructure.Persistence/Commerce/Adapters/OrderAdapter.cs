using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Commerce.Repositories;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Commerce.Errors;
using VibraHeka.Domain.Commerce.Ports.Out;

namespace Infrastructure.Persistence.Commerce.Adapters;

/// <summary>
/// Provides a mechanism to adapt and persist order-related data between domain entities
/// and the underlying storage infrastructure. Implements the <see cref="IOrderPort"/>
/// and <see cref="IOrderLinePort"/> interfaces to handle orders and order lines.
/// </summary>
public class OrderAdapter(
    OrderLineRepository orderLineRepository,
    OrderRepository orderRepository,
    ILogger<OrderAdapter> logger) : IOrderPort, IOrderLinePort
{
    public Task<Result<OrderEntity>> CreateOrderAsync(OrderEntity order, CancellationToken cancellationToken)
    {
        return Maybe.From(order)
            .ToResult(CommerceErrors.InvalidOrder)
            .BindTry(validOrder => orderRepository.SaveOrderAsync(validOrder, cancellationToken), HandleOrderException)
            .BindTry(orderEntity => CreateOrderLinesAsync(orderEntity.Lines, cancellationToken), HandleOrderLineException)
                .Map(_ => order);
    }


    /// <summary>
    /// Persists a collection of order line entities to the underlying storage.
    /// </summary>
    /// <param name="orderLines">The collection of order line entities to be saved.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used by the caller to request the operation to be canceled.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a <see cref="Result{T}"/>
    /// wrapping the read-only collection of successfully persisted order line entities, or an error code
    /// if the operation fails.
    /// </returns>
    public Task<Result<IReadOnlyCollection<OrderLineEntity>>> CreateOrderLinesAsync(
        IReadOnlyCollection<OrderLineEntity> orderLines, CancellationToken cancellationToken)
    {
       return Maybe.From(orderLines)
            .ToResult(CommerceErrors.InvalidOrderLines)
            .BindTry(validLines => orderLineRepository.SaveOrderLinesAsync(validLines, cancellationToken),
                HandleOrderLineException)
            .Map(_ => orderLines);
    }


    /// <summary>
    /// Handles exceptions that occur during order-related operations and logs the error.
    /// </summary>
    /// <param name="ex">The exception that occurred during the operation.</param>
    /// <returns>A string representing the error code for failed order operations.</returns>
    private string HandleOrderException(Exception ex)
    {
        logger.LogError(ex, "An error occured while operating with the order");
        return CommerceErrors.FailedToOperateWithOrder;
    }

    /// <summary>
    /// Handles exceptions that occur during operations related to order lines.
    /// Logs the error and returns a predefined error code indicating the failure.
    /// </summary>
    /// <param name="ex">The exception that was thrown during the operation.</param>
    /// <returns>
    /// A string representing the error code associated with the failure of the order line operation.
    /// </returns>
    private string HandleOrderLineException(Exception ex)
    {
        logger.LogError(ex, "An error occured while operating with the order lines");
        return CommerceErrors.FailedToOperateWithOrderLines;
    }
    
}
