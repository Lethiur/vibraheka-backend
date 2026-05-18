using Infrastructure.Persistence.Commerce.Models;
using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Commerce.Entities;

namespace Infrastructure.Persistence.Commerce.Mappers;

[Mapper]
public partial class OrderLineMapper
{
    [MapperIgnoreSource(nameof(OrderLineEntity.Components))]
    public partial OrderLineDBModel FromDomain(OrderLineEntity entity);

    [MapperIgnoreTarget(nameof(OrderLineEntity.Components))]
    public partial OrderLineEntity ToDomain(OrderLineDBModel entity);
}
