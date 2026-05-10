using CSharpFunctionalExtensions;
using VibraHeka.Domain.Orders.Entities;
using VibraHeka.Domain.Orders.Models;

namespace VibraHeka.Domain.Orders.Ports.In;

public interface ICreateOrderPort
{
    public Task<Result<OrderEntity>> ExecuteOrderAsync(ExecuteOrderModel model, CancellationToken token);
}
