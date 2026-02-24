using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.Mappers;

[Mapper]
public partial class SubscriptionEntityMapper
{
    public partial SubscriptionDBModel ToInternal(SubscriptionEntity entity);
    
    public partial SubscriptionEntity ToDomain(SubscriptionDBModel entity);
    
}
