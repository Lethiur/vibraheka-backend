using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Subscriptions;

namespace VibraHeka.Web.Controllers.Subscriptions;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SubscriptionMapper
{
    [MapperIgnoreTarget(nameof(SubscriptionResponse.AdditionalProperties))]
    public partial SubscriptionResponse ToResponse(SubscriptionCheckoutSessionEntity entity);
    
    [MapperIgnoreTarget(nameof(SubscriptionDetailsResponse.AdditionalProperties))]
    public partial SubscriptionDetailsResponse ToResponse(SubscriptionEntity entity);
}
