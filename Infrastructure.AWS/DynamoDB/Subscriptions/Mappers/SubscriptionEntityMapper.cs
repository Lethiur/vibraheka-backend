using Infrastructure.AWS.DynamoDB.Subscriptions.Models;
using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Entities;

namespace Infrastructure.AWS.DynamoDB.Subscriptions.Mappers;

[Mapper]
public partial class SubscriptionEntityMapper
{
    public partial SubscriptionDBModel FromDomain(SubscriptionEntity entity);
    
    public partial SubscriptionEntity ToDomain(SubscriptionDBModel entity);
    
}
