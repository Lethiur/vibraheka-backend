using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Domain.Commerce.Entities;

namespace VibraHeka.Application.Commerce.Ports.Out;

public interface IOrderWritePort
{
    ITransactionalWriteOperation CreateOrder(OrderEntity order);
}
