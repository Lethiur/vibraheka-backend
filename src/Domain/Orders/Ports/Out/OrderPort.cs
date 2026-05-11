using CSharpFunctionalExtensions;
using VibraHeka.Domain.Orders.Entities;
using VibraHeka.Domain.Orders.Models;

namespace VibraHeka.Domain.Orders.Ports.Out;

public interface IOrderPort
{
    public Task<Result<OrderEntity>> CreateOrderAsync(OrderEntity order, CancellationToken cancellationToken);
    
    public Task<Result<OrderEntity>> GetOrderByIDAsync(string ordeerID,  CancellationToken cancellationToken);
    
    public Task<Result<OrderEntity>> UpdatePaymentInfoAsync(OrderEntity order, CheckoutSessionCompletedModel model, CancellationToken cancellationToken);
}
