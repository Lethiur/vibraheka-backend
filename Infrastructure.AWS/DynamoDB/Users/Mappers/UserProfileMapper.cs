using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace Infrastructure.AWS.DynamoDB.Users.Mappers;

[Mapper]
public partial class UserProfileMapper
{
    public partial UserProfileDBModel FromDomain(UserProfileEntity entity);
    
    public partial UserProfileEntity ToDomain(UserProfileDBModel model);
}
