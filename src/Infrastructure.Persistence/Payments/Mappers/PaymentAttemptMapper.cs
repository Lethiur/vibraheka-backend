using Infrastructure.Persistence.Payments.Models;
using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Payments.Entities;

namespace Infrastructure.Persistence.Payments.Mappers;

[Mapper]
public partial class PaymentAttemptMapper
{
    public partial PaymentAttemptDBModel FromDomain(PaymentAttemptEntity entity);

    public partial PaymentAttemptEntity ToDomain(PaymentAttemptDBModel dbModel);
}
