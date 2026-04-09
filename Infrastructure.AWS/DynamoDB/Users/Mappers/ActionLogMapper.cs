using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace Infrastructure.AWS.DynamoDB.Users.Mappers;

[Mapper]
public partial class ActionLogMapper
{
    public partial ActionLogDBModel FromDomain(ActionLogEntity entity);
    
    public partial ActionLogEntity ToDomain(ActionLogDBModel model);
}
