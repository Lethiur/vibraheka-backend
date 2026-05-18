using CSharpFunctionalExtensions;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Payments.Entities;

namespace VibraHeka.Domain.Payments.Ports.Out;

public interface IPaymentsPort
{
    public Task<Result<string>> RegisterCustomerAsync(UserEntity user, CancellationToken token);

    public Task<Result<PaymentAttemptEntity>> CreatePaymentIntentAsync(OrderEntity order, CancellationToken token);
}
