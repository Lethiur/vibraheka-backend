using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Entities;

namespace VibraHeka.Web.Mappers;
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateSubscriptionMapper
{
    public partial SubscriptionCreationDTO toDTO(SubscriptionCheckoutSessionEntity entity);
}
