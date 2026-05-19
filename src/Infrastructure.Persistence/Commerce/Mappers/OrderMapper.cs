using Infrastructure.Persistence.Commerce.Models;
using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Commerce.Entities;

namespace Infrastructure.Persistence.Commerce.Mappers;

[Mapper]
public partial class OrderMapper
{
    [MapperIgnoreSource(nameof(OrderEntity.Lines))]
    public partial OrderDBModel FromDomain(OrderEntity entity);

    [MapperIgnoreTarget(nameof(OrderEntity.Lines))]
    public partial OrderEntity ToDomain(OrderDBModel entity);
}
