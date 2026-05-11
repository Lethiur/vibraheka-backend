using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Orders.Entities;
using VibraHeka.Domain.Orders.Models;

namespace VibraHeka.Application.Orders.Mappers;

[Mapper]
public partial class OrderEntityMapper
{
    
    [MapProperty(nameof(ExecuteOrderModel.UserID), nameof(OrderEntity.CreatedBy))]
    [MapProperty(nameof(ExecuteOrderModel.UserID), nameof(OrderEntity.LastModifiedBy))]
    [MapperIgnoreTarget(nameof(OrderEntity.Created))]
    [MapperIgnoreTarget(nameof(OrderEntity.LastModified))]
    [MapperIgnoreTarget(nameof(OrderEntity.ExternalOrderID))]
    [MapperIgnoreTarget(nameof(OrderEntity.OrderStatus))]
    [MapperIgnoreTarget(nameof(OrderEntity.PaymentGatewayUrl))]
    private partial OrderEntity FromModelCore(
        ExecuteOrderModel model,
        string OrderID);

    public OrderEntity FromModel(
        ExecuteOrderModel model,
        DateTimeOffset dateCreated,
        string orderID)
    {
        var entity = FromModelCore(model, orderID);

        entity.Created = dateCreated;
        entity.LastModified = dateCreated;

        return entity;
    }
}
