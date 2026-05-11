using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Products.Entities;
using VibraHeka.Domain.Products.Models;
using VibraHeka.Domain.Products.Ports.Out;

namespace Infrastructure.Persistence.Products.Adapters;

public class ProductAdapter : IProductPort
{
    public Task<Result<ProductEntity>> GetProductByIdAsync(string productId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result<ProductEntity>> SaveProductAsync(ProductEntity productEntity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Unit>> DeleteProductAsync(string productId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<ProductEntity>>> GetAllProductsAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Unit>> UpdateProductStatusAsync(UpdateProductStatusModel model, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Unit>> UpdateProductNameAsync(UpdateProductNameModel model, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Unit>> UpdateProductDescriptionAsync(UpdateProductDescriptionModel model, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
