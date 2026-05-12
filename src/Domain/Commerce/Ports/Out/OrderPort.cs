using CSharpFunctionalExtensions;
using VibraHeka.Domain.Commerce.Entities;

namespace VibraHeka.Domain.Commerce.Ports.Out;

public interface IOrderPort
{
    public Task<Result<OrderEntity>> CreateOrderAsync(OrderEntity order, CancellationToken cancellationToken);
}
