using CSharpFunctionalExtensions;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Application.Catalog.Ports.Out;

public interface IProductCreationWritePort
{
    Task<Result<ProductGatewayCreatedResponseModel>> CreateProductInGatewayAsync(ProductEntity productEntity, SellableItemPriceEntity priceEntity, CancellationToken cancellationToken);
}
