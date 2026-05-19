using CSharpFunctionalExtensions;
using MediatR;

namespace VibraHeka.Domain.Catalog.Ports.Out;

public interface IProductPort
{
    public Task<Result<Unit>> DeactivateProductAsync(string productId, CancellationToken cancellationToken);
    
    public Task<Result<Unit>> ActivateProductAsync(string productId, CancellationToken cancellationToken);
    
}
