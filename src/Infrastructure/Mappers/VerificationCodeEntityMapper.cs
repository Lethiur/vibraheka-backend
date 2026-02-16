using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.Mappers;

[Mapper]
public partial class VerificationCodeEntityMapper
{
    public partial VerificationCodeDBModel ToInternal(VerificationCodeEntity entity);
    
    public partial VerificationCodeEntity ToDomain(VerificationCodeDBModel entity);
}
