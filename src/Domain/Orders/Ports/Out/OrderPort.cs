using CSharpFunctionalExtensions;
using VibraHeka.Domain.Orders.Entities;

namespace VibraHeka.Domain.Orders.Ports.Out;

public interface IOrderPort
{
    public Task<Result<OrderEntity>> CreateOrderAsync(OrderEntity order, CancellationToken cancellationToken);
}
