using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace Infrastructure.AWS.DynamoDB.PasswordResetToken.Mappers;

[Mapper]
public partial class UsersCodeMapper
{
    public partial UserCodeDBModel FromDomain(UserCodeEntity entity);
    
    public partial UserCodeEntity ToDomain(UserCodeDBModel model);
}
