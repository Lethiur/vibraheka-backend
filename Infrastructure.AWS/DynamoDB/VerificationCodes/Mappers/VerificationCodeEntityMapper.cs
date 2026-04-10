using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace Infrastructure.AWS.DynamoDB.VerificationCodes.Mappers;

[Mapper]
public partial class VerificationCodeEntityMapper
{
    public partial VerificationCodeDBModel FromDomain(VerificationCodeEntity entity);
    
    public partial VerificationCodeEntity ToDomain(VerificationCodeDBModel entity);
}
