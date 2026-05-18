using Infrastructure.Persistence.Catalog.Models;
using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
namespace Infrastructure.Persistence.Catalog.Mappers;

[Mapper]
public partial class SellableItemPriceEntityMapper
{
    [MapperIgnoreSource(nameof(SellableItemPriceEntity.BillingInterval))]
    [MapperIgnoreTarget(nameof(SellableItemPriceDBModel.BillingIntervalValue))]
    public partial SellableItemPriceDBModel FromDomain(SellableItemPriceEntity entity);

    [MapperIgnoreTarget(nameof(SellableItemPriceEntity.BillingInterval))]
    [MapperIgnoreSource(nameof(SellableItemPriceDBModel.BillingIntervalValue))]
    public partial SellableItemPriceEntity ToDomain(SellableItemPriceDBModel model);
 
}
