using CSharpFunctionalExtensions;
using NMoneys;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Domain.Common.Interfaces;

namespace VibraHeka.Application.Catalog.Commands.CreateProduct;

public class CreateProductCommandHandler(
    IProductWritePort productWritePort,
    IProductCreationWritePort productCreationWritePort,
    ISellableItemPriceWritePort sellableItemPriceWritePort,
    ISellableItemWritePort sellableItemWritePort,
    ICurrentUserService currentUserService,
    IAtomicWriteStore transactionStore) : IRequestHandler<CreateProductCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        ProductEntity productEntity = new ProductEntity()
        {
            ProductID = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            Created = DateTime.UtcNow,
            CreatedBy = currentUserService.UserId,
            LastModified = DateTime.UtcNow,
            LastModifiedBy = currentUserService.UserId,
        };

        SellableItemEntity sellableItemEntity = new SellableItemEntity()
        {
            Name = request.Name,
            SellableItemID = Guid.NewGuid().ToString(),
            Type = SellableItemType.Product,
            ReferenceID = productEntity.ProductID,
            IsActive = true,
            Created = DateTime.UtcNow,
            CreatedBy = currentUserService.UserId,
            LastModified = DateTime.UtcNow,
            LastModifiedBy = currentUserService.UserId,
        };

        SellableItemPriceEntity sellableItemPriceEntity = new SellableItemPriceEntity()
        {
            Amount = new Money(request.Price, request.CurrencyCode),
            SellableItemPriceID = Guid.NewGuid().ToString(),
            Kind = PriceKind.OneTime,
            SellableItemID = sellableItemEntity.SellableItemID,
            Created = DateTime.UtcNow,
            CreatedBy = currentUserService.UserId,
            LastModified = DateTime.UtcNow,
            LastModifiedBy = currentUserService.UserId,
        };

        (bool _, bool isFailure, ProductGatewayCreatedResponseModel? value, string? error) =
            await productCreationWritePort.CreateProductInGatewayAsync(productEntity, sellableItemPriceEntity,
                cancellationToken);

        if (isFailure)
        {
            return Result.Failure<string>(CatalogErrors.FailedToCreateProduct);
        }

        sellableItemPriceEntity.ExternalProductID = value.ProductGatewayID;
        sellableItemPriceEntity.ExternalPriceID = value.ProductGatewayPriceID;

        TransactionalWriteBatch productCreationBatch = new TransactionalWriteBatch(Guid.NewGuid().ToString());
        productCreationBatch.Add(productWritePort.CreateProduct(productEntity));
        productCreationBatch.Add(sellableItemWritePort.CreateSellableItem(sellableItemEntity));
        productCreationBatch.Add(sellableItemPriceWritePort.CreateSellableItemPrice(sellableItemPriceEntity));

        return await transactionStore.CommitAsync(productCreationBatch, cancellationToken)
            .Map(_ => productEntity.ProductID).MapError(_ => CatalogErrors.FailedToCreateProduct);
    }
}
