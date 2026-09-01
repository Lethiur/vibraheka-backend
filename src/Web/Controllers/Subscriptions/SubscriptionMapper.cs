using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Subscriptions;

namespace VibraHeka.Web.Controllers.Subscriptions;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SubscriptionMapper
{
    public partial SubscriptionResponse ToResponse(SubscriptionCheckoutSessionEntity entity);
    
    public partial SubscriptionDetailsResponse ToResponse(SubscriptionEntity entity);
}
