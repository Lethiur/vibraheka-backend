using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Products.Models;

namespace VibraHeka.Domain.Products.Ports.Out;

public interface IProductStoragePort
{
    public Task<Result<Unit>> DeleteProductAsync(string externalProductID, CancellationToken cancellationToken);
    public Task<Result<Unit>> UpdateProductStatusAsync(UpdateProductStatusModel model, CancellationToken cancellationToken);
    public Task<Result<Unit>> UpdateProductNameAsync(UpdateProductNameModel model, CancellationToken cancellationToken);
    public Task<Result<Unit>> UpdateProductDescriptionAsync(UpdateProductDescriptionModel model, CancellationToken cancellationToken);
}
