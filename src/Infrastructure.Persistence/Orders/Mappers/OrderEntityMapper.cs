using Infrastructure.Persistence.Orders.Entities;
using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Orders.Entities;

namespace Infrastructure.Persistence.Orders.Mappers;

[Mapper]
public partial class OrderEntityMapper
{
    public partial OrderDBModel FromDomain(OrderEntity entity);
    
    public partial OrderEntity ToDomain(OrderDBModel entity);
}
