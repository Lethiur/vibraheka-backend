using CSharpFunctionalExtensions;
using VibraHeka.Domain.Commerce.Entities;

namespace VibraHeka.Domain.Commerce.Ports.Out;

public interface IOrderLinePort
{
    public Task<Result<IReadOnlyCollection<OrderLineEntity>>> CreateOrderLinesAsync(IReadOnlyCollection<OrderLineEntity> orderLines, CancellationToken cancellationToken);
}
