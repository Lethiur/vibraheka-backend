using Infrastructure.Persistence.Catalog.Models;
using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Catalog.Entities;

namespace Infrastructure.Persistence.Catalog.Mappers;

[Mapper]
public partial class SellableItemPriceEntityMapper
{
    
    public partial SellableItemPriceDBModel FromDomain(SellableItemPriceEntity entity);
    
    public partial SellableItemPriceEntity ToDomain(SellableItemPriceDBModel model);

}
