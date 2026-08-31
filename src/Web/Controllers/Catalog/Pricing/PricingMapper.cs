using Riok.Mapperly.Abstractions;
using VibraHeka.Application.Catalog.Commands.AdminActivatePrice;
using VibraHeka.Application.Catalog.Commands.AdminCreatePrice;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Web.Catalog.Pricing.Controllers;

namespace VibraHeka.Web.Controllers.Catalog.Pricing;

[Mapper]
public partial class PricingMapper
{
    [MapProperty(nameof(CreatePriceRequest.ProductId),  nameof(AdminCreatePriceCommand.SellableItemID))]
    [MapProperty(nameof(CreatePriceRequest.Amount),  nameof(AdminCreatePriceCommand.Price))]
    [MapProperty(nameof(CreatePriceRequest.Currency),  nameof(AdminCreatePriceCommand.Currency))]
    [MapProperty(nameof(CreatePriceRequest.Active),  nameof(AdminCreatePriceCommand.SetToActive))]
    [MapProperty(nameof(CreatePriceRequest.BillingInterval),  nameof(AdminCreatePriceCommand.Interval))]
    [MapperIgnoreSource(nameof(CreatePriceRequest.AdditionalProperties))]
    public partial AdminCreatePriceCommand ToCommand(CreatePriceRequest request);

    [MapProperty(nameof(ActivatePriceRequest.PriceId),  nameof(AdminActivatePriceCommand.SellableItemPriceID))]
    [MapProperty(nameof(ActivatePriceRequest.ProductId),  nameof(AdminActivatePriceCommand.SellableItemID))]
    [MapperIgnoreSource(nameof(CreatePriceRequest.AdditionalProperties))]
    public partial AdminActivatePriceCommand ToCommand(ActivatePriceRequest request);
    
    [MapProperty(nameof(SellableItemEntity.SellableItemID), nameof(ProductDTO.Id))]
    [MapProperty(nameof(SellableItemEntity.Type), nameof(ProductDTO.ProductType))]
    [MapperIgnoreSource(nameof(SellableItemEntity.ExternalProductID))]
    [MapperIgnoreSource(nameof(SellableItemEntity.Created))]
    [MapperIgnoreSource(nameof(SellableItemEntity.CreatedBy))]
    [MapperIgnoreSource(nameof(SellableItemEntity.LastModified))]
    [MapperIgnoreSource(nameof(SellableItemEntity.LastModifiedBy))]
    [MapperIgnoreTarget(nameof(ProductDTO.AdditionalProperties))]
    public partial ProductDTO ToResponse(SellableItemEntity dto);

    [MapProperty(nameof(SellableItemPriceEntity.SellableItemPriceID), nameof(ProductPriceDTO.PriceId))]
    [MapProperty(nameof(SellableItemPriceEntity.SellableItemID), nameof(ProductPriceDTO.ProductId))]
    [MapProperty(nameof(SellableItemPriceEntity.Amount.Amount), nameof(ProductPriceDTO.Amount))]
    [MapProperty(nameof(SellableItemPriceEntity.Amount.CurrencyCode), nameof(ProductPriceDTO.CurrencyCode))]
    [MapProperty(nameof(SellableItemPriceEntity.Kind), nameof(ProductPriceDTO.PriceKind))]
    [MapperIgnoreSource(nameof(SellableItemPriceEntity.ExternalPriceID))]
    [MapperIgnoreSource(nameof(SellableItemPriceEntity.ExternalProductID))]
    [MapperIgnoreSource(nameof(SellableItemPriceEntity.Created))]
    [MapperIgnoreSource(nameof(SellableItemPriceEntity.CreatedBy))]
    [MapperIgnoreSource(nameof(SellableItemPriceEntity.LastModified))]
    [MapperIgnoreSource(nameof(SellableItemPriceEntity.LastModifiedBy))]
    [MapperIgnoreTarget(nameof(ProductPriceDTO.AdditionalProperties))]
    public partial ProductPriceDTO ToResponse(SellableItemPriceEntity dto);
}
