using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Products.Models;

namespace VibraHeka.Domain.Products.Ports.Out;

public interface IProductBundlePort : IProductPort
{
    public Task<Result<Unit>> AddProductToBundleAsync(AddProductToBundleModel model, CancellationToken cancellationToken);
}
