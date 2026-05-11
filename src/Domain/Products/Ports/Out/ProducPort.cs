using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Products.Entities;
using VibraHeka.Domain.Products.Models;

namespace VibraHeka.Domain.Products.Ports.Out;

public interface IProductPort
{
    public Task<Result<ProductEntity>> GetProductByIdAsync(string productId, CancellationToken cancellationToken);
    public Task<Result<ProductEntity>> SaveProductAsync(ProductEntity productEntity, CancellationToken cancellationToken);
    public Task<Result<Unit>> DeleteProductAsync(string productId, CancellationToken cancellationToken);
    public Task<Result<IEnumerable<ProductEntity>>> GetAllProductsAsync(CancellationToken cancellationToken);
    public Task<Result<Unit>> UpdateProductStatusAsync(UpdateProductStatusModel model, CancellationToken cancellationToken);
    public Task<Result<Unit>> UpdateProductNameAsync(UpdateProductNameModel model, CancellationToken cancellationToken);
    public Task<Result<Unit>> UpdateProductDescriptionAsync(UpdateProductDescriptionModel model, CancellationToken cancellationToken);
}
