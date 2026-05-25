using CSharpFunctionalExtensions;
using NMoneys;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Domain.Catalog.Ports.In;
using VibraHeka.Domain.Common.Interfaces;

namespace VibraHeka.Application.Catalog.UseCases;

/// <summary>
/// Encapsulates the use case for registering a sellable product in the system.
/// </summary>
/// <remarks>
/// This class coordinates operations required to create a sellable product, including saving product data,
/// associating pricing information, and interacting with underlying data stores in a transactional context.
/// It implements the <c>IRegisterSellableItemPort</c> interface to ensure compliance with the domain's contract
/// for handling sellable items.
/// </remarks>
/// <param name="productCreationWritePort">
/// The port responsible for persisting product creation data during the registration process.
/// </param>
/// <param name="sellableItemPriceWritePort">
/// The port responsible for persisting price information for the sellable item.
/// </param>
/// <param name="sellableItemWritePort">
/// The port responsible for persisting general sellable item metadata.
/// </param>
/// <param name="currentUserService">
/// Provides data related to the current user, such as user identity, for auditing or contextual purposes.
/// </param>
/// <param name="transactionStore">
/// Manages transactional operations to ensure consistency across multiple dependent operations.
/// </param>
public class RegisterSellableProductUseCase(
    IProductCreationWritePort productCreationWritePort,
    ISellableItemPriceWritePort sellableItemPriceWritePort,
    ISellableItemWritePort sellableItemWritePort,
    ICurrentUserService currentUserService,
    IAtomicWriteStore transactionStore) : IRegisterSellableItemPort
{
    public async Task<Result<Unit>> RegisterSellableItemAsync(ProductEntity entity, Money price, PriceKind kind,
        CancellationToken cancellationToken)
    {
        SellableItemEntity sellableItemEntity = new SellableItemEntity()
        {
            Name = entity.Name,
            SellableItemID = Guid.NewGuid().ToString(),
            Type = kind == PriceKind.Recurring ? SellableItemType.SubscriptionPlan : SellableItemType.Product,
            ReferenceID = entity.ID,
            IsActive = true,
            Created = DateTime.UtcNow,
            CreatedBy = currentUserService.UserId,
            LastModified = DateTime.UtcNow,
            LastModifiedBy = currentUserService.UserId,
        };

        SellableItemPriceEntity sellableItemPriceEntity = new SellableItemPriceEntity()
        {
            Amount = price,
            SellableItemPriceID = Guid.NewGuid().ToString(),
            Kind = kind,
            SellableItemID = sellableItemEntity.SellableItemID,
            Created = DateTime.UtcNow,
            CreatedBy = currentUserService.UserId,
            LastModified = DateTime.UtcNow,
            LastModifiedBy = currentUserService.UserId,
        };

        (bool _, bool isFailure, ProductGatewayCreatedResponseModel? value, string? error) =
            await productCreationWritePort.CreateProductInGatewayAsync(entity, sellableItemPriceEntity,
                cancellationToken);

        if (isFailure)
        {
            return Result.Failure<Unit>(CatalogErrors.FailedToCreateProduct);
        }

        sellableItemPriceEntity.ExternalProductID = value.ProductGatewayID;
        sellableItemPriceEntity.ExternalPriceID = value.ProductGatewayPriceID;

        TransactionalWriteBatch productCreationBatch = new TransactionalWriteBatch(Guid.NewGuid().ToString());
        productCreationBatch.Add(sellableItemWritePort.CreateSellableItem(sellableItemEntity));
        productCreationBatch.Add(sellableItemPriceWritePort.CreateSellableItemPrice(sellableItemPriceEntity));

        return await transactionStore.CommitAsync(productCreationBatch, cancellationToken);
    }

}
