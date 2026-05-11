using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Orders.Entities;
using VibraHeka.Domain.Orders.Models;

namespace VibraHeka.Application.Orders.Mappers;

[Mapper]
public partial class OrderEntityMapper
{
    
    [MapValue(nameof(OrderEntity.OrderID), Guid.NewGuid().ToString())]
    public partial OrderEntity FromModel(ExecuteOrderModel model)
}
