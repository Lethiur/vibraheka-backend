using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Common.Interfaces.Payments;

public interface IPaymentService
{
    Task<string> RegisterOrder(UserEntity payer, OrderEntity orderEntity);
}
