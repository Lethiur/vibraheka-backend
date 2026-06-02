using Infrastructure.Persistence.Catalog.Models;
using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Catalog.Entities;
namespace Infrastructure.Persistence.Catalog.Mappers;

[Mapper]
public partial class SellableItemEntityMapper
{
    [MapperIgnoreSource(nameof(SellableItemEntity.Prices))]
    public partial SellableItemDBModel FromDomain(SellableItemEntity entity);
    
    [MapperIgnoreTarget(nameof(SellableItemEntity.Prices))]
    public partial SellableItemEntity ToDomain(SellableItemDBModel model);

}
